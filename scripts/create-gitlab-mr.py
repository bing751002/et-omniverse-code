"""
Create a GitLab merge request using the repo MR template.

Required for real mode:
  GITLAB_URL       e.g. https://gitlab.internal
  GITLAB_TOKEN     personal/project token with api scope
  GITLAB_PROJECT_ID or a configured origin remote that maps to a GitLab project

Examples:
  python scripts/create-gitlab-mr.py --dry-run --source-branch feat/p1/foundation --title "P1.0 foundation"
  python scripts/create-gitlab-mr.py --push --title "F-021 Create Batch"
"""

from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
import urllib.parse
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_TEMPLATE = REPO_ROOT / ".gitlab" / "merge_request_templates" / "Default.md"


def run_git(args: list[str], check: bool = True) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=REPO_ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False,
    )
    if check and result.returncode != 0:
        raise RuntimeError(result.stderr.strip())
    return result.stdout.strip()


def current_branch() -> str:
    return run_git(["branch", "--show-current"])


def latest_commit_subject() -> str:
    subject = run_git(["log", "-1", "--pretty=%s"], check=False)
    return subject or current_branch()


def origin_url() -> str:
    return run_git(["remote", "get-url", "origin"], check=False)


def project_id_from_env_or_origin() -> str | None:
    if os.environ.get("GITLAB_PROJECT_ID"):
        return os.environ["GITLAB_PROJECT_ID"]

    remote = origin_url()
    if not remote:
        return None

    # Supports:
    # - git@gitlab.example.com:group/project.git
    # - ssh://git@gitlab.example.com/group/project.git
    # - https://gitlab.example.com/group/project.git
    path = remote
    if ":" in remote and remote.startswith("git@"):
        path = remote.split(":", 1)[1]
    elif "://" in remote:
        parsed = urllib.parse.urlparse(remote)
        path = parsed.path.lstrip("/")

    if path.endswith(".git"):
        path = path[:-4]

    return urllib.parse.quote(path, safe="")


def load_description(description_file: str | None) -> str:
    path = Path(description_file) if description_file else DEFAULT_TEMPLATE
    if not path.is_absolute():
        path = REPO_ROOT / path
    return path.read_text(encoding="utf-8")


def maybe_push(branch: str) -> None:
    print(f"INFO pushing branch {branch} to origin")
    subprocess.run(["git", "push", "-u", "origin", branch], cwd=REPO_ROOT, check=True)


def create_merge_request(payload: dict[str, object]) -> dict[str, object]:
    gitlab_url = os.environ.get("GITLAB_URL", "").rstrip("/")
    token = os.environ.get("GITLAB_TOKEN")
    project_id = project_id_from_env_or_origin()

    missing = [
        name for name, value in (
            ("GITLAB_URL", gitlab_url),
            ("GITLAB_TOKEN", token),
            ("GITLAB_PROJECT_ID or origin remote", project_id),
        )
        if not value
    ]
    if missing:
        raise RuntimeError("missing required GitLab settings: " + ", ".join(missing))

    url = f"{gitlab_url}/api/v4/projects/{project_id}/merge_requests"
    body = json.dumps(payload).encode("utf-8")
    request = urllib.request.Request(
        url,
        data=body,
        method="POST",
        headers={
            "PRIVATE-TOKEN": token or "",
            "Content-Type": "application/json",
        },
    )

    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.loads(response.read().decode("utf-8"))
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"GitLab API failed: HTTP {exc.code}: {detail}") from exc


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--title", help="merge request title")
    parser.add_argument("--source-branch", help="source branch; defaults to current branch")
    parser.add_argument("--target-branch", default="main")
    parser.add_argument("--description-file", help="markdown file to use as MR description")
    parser.add_argument("--label", action="append", default=[])
    parser.add_argument("--assignee-id", type=int)
    parser.add_argument("--reviewer-id", action="append", type=int, default=[])
    parser.add_argument("--draft", action="store_true")
    parser.add_argument("--push", action="store_true", help="push current branch before creating MR")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    source_branch = args.source_branch or current_branch()
    if not source_branch:
        print("ERR  cannot determine source branch", file=sys.stderr)
        return 1
    if source_branch == args.target_branch:
        print("ERR  source branch must differ from target branch", file=sys.stderr)
        return 1

    title = args.title or latest_commit_subject()
    if args.draft and not title.lower().startswith(("draft:", "wip:")):
        title = "Draft: " + title

    payload: dict[str, object] = {
        "source_branch": source_branch,
        "target_branch": args.target_branch,
        "title": title,
        "description": load_description(args.description_file),
        "remove_source_branch": True,
        "squash": True,
    }
    if args.label:
        payload["labels"] = ",".join(args.label)
    if args.assignee_id is not None:
        payload["assignee_id"] = args.assignee_id
    if args.reviewer_id:
        payload["reviewer_ids"] = args.reviewer_id

    if args.push and not args.dry_run:
        maybe_push(source_branch)

    if args.dry_run:
        print(json.dumps(payload, ensure_ascii=False, indent=2))
        return 0

    try:
        result = create_merge_request(payload)
    except RuntimeError as exc:
        print(f"ERR  {exc}", file=sys.stderr)
        return 1

    print("OK   merge request created")
    print(result.get("web_url", ""))
    return 0


if __name__ == "__main__":
    sys.exit(main())
