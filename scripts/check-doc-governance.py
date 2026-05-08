"""
Enforce documentation governance for code and config changes.

Usage:
  python scripts/check-doc-governance.py
  python scripts/check-doc-governance.py --staged

Default mode checks working-tree changes against HEAD. `--staged` checks files
staged for commit. This is intentionally conservative for a new project:
changes to code/config must be accompanied by either the relevant docs update or
a docs/no-doc-update-*.md rationale.
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent


@dataclass(frozen=True)
class Rule:
    name: str
    changed_prefixes: tuple[str, ...]
    required_prefixes: tuple[str, ...]
    message: str
    allow_rationale: bool = True


RULES = (
    Rule(
        name="source-code-requires-spec-or-kb",
        changed_prefixes=("src/", "tests/"),
        required_prefixes=(
            ".planning/",
            "docs/specs/",
            "docs/patterns/",
            "docs/ACCESS-CONTROL.md",
            "docs/ARCHITECTURE.md",
            "docs/CONVENTIONS.md",
            "docs/DECISIONS.md",
            "docs/GLOSSARY.md",
            "docs/INFRA.md",
        ),
        message="src/tests changed: update a GSD phase artifact (.planning/), spec, or relevant KB doc.",
    ),
    Rule(
        name="infra-requires-infra-doc",
        changed_prefixes=("docker/", "ci/", ".githooks/", "scripts/"),
        required_prefixes=("docs/INFRA.md", "docs/WORKFLOW.md", "docs/DOCUMENTATION.md"),
        message="infra/tooling changed: update INFRA, WORKFLOW, or DOCUMENTATION.",
    ),
    Rule(
        name="adr-summary-sync",
        changed_prefixes=("docs/decisions/",),
        required_prefixes=("docs/DECISIONS.md",),
        message="ADR files changed: update DECISIONS summary.",
        allow_rationale=False,
    ),
)

RATIONALE_PREFIX = "docs/no-doc-update-"


def run_git(args: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *args],
        cwd=REPO_ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False,
    )


def is_git_repo() -> bool:
    result = run_git(["rev-parse", "--is-inside-work-tree"])
    return result.returncode == 0 and result.stdout.strip() == "true"


def changed_files(staged: bool) -> list[str]:
    if not is_git_repo():
        print("WARN not a git repository; documentation governance check skipped")
        return []

    args = ["diff", "--name-only", "--diff-filter=ACMR"]
    if staged:
        args.insert(1, "--cached")

    result = run_git(args)
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        return []

    files = [line.replace("\\", "/") for line in result.stdout.splitlines() if line.strip()]

    # Include untracked files in non-staged CI/local mode.
    if not staged:
        untracked = run_git(["ls-files", "--others", "--exclude-standard"])
        if untracked.returncode == 0:
            files.extend(line.replace("\\", "/") for line in untracked.stdout.splitlines() if line.strip())

    return sorted(set(files))


def matches_any(path: str, prefixes: tuple[str, ...]) -> bool:
    return any(path == prefix.rstrip("/") or path.startswith(prefix) for prefix in prefixes)


def has_rationale(files: list[str]) -> bool:
    return any(
        path.startswith(RATIONALE_PREFIX)
        and path.endswith(".md")
        and path != "docs/no-doc-update-_template.md"
        for path in files
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--staged", action="store_true", help="check staged files only")
    args = parser.parse_args()

    files = changed_files(args.staged)
    if not files:
        print("OK   no changed files to check")
        return 0

    rationale = has_rationale(files)
    violations: list[tuple[Rule, list[str]]] = []

    for rule in RULES:
        triggering = [path for path in files if matches_any(path, rule.changed_prefixes)]
        if not triggering:
            continue

        has_required = any(matches_any(path, rule.required_prefixes) for path in files)
        can_use_rationale = rationale and rule.allow_rationale
        if not has_required and not can_use_rationale:
            violations.append((rule, triggering))

    if violations:
        print("ERR  documentation governance check failed", file=sys.stderr)
        for rule, triggering in violations:
            print(f"\n[{rule.name}] {rule.message}", file=sys.stderr)
            for path in triggering:
                print(f"  changed: {path}", file=sys.stderr)
            if rule.allow_rationale:
                print("  fix: update the required docs, or add docs/no-doc-update-<topic>.md with a clear reason.", file=sys.stderr)
            else:
                print("  fix: update the required docs; this rule cannot be bypassed by no-doc-update rationale.", file=sys.stderr)
        return 1

    checked = len(files)
    if rationale:
        print(f"OK   documentation governance passed for {checked} changed file(s) with rationale")
    else:
        print(f"OK   documentation governance passed for {checked} changed file(s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
