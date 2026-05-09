#!/usr/bin/env python3
"""
F-007 AC-C6 / D-22: Test endpoint namespace consistency scanner.

規則：
  1. 任何 src/backend/**/*.cs 內出現 `app.Map{Get,Post,Put,Delete,Patch}("/test/...")`
     （少了 `/api/` 前綴）→ 違規（防舊 namespace 偷渡）
  2. 任何 path 開頭 `/api/test/` 但檔案不在
     `src/backend/ETOmniverse.Api/Features/Test/` 底下 → 違規
  3. 例外：`/api/common/ping/fail`（F-003 ping sample，per D-22 白名單）

Usage:
  python scripts/check-test-endpoints.py
  python scripts/check-test-endpoints.py --staged
  python scripts/check-test-endpoints.py --scan-root <p>
"""
from __future__ import annotations
import argparse
import re
import subprocess
import sys
from pathlib import Path

# 偵測 endpoint 註冊：app/group/endpoints/routeBuilder.MapGet("/path"...
ENDPOINT_PATTERN = re.compile(
    r'(?:app|group|endpoints|routeBuilder)\.Map(?:Get|Post|Put|Delete|Patch)\s*\(\s*"([^"]+)"'
)

DEFAULT_SCAN_ROOT = Path("src/backend")
# 相對 SCAN_ROOT 的 posix 字串；只有此 prefix 底下的檔可以註冊 /api/test/* path
ALLOWED_DIR_PREFIX = "ETOmniverse.Api/Features/Test/"
# /test/* 與 /api/test/* 規則之外的白名單（per D-22）
WHITELIST_PATHS = {"/api/common/ping/fail"}


def staged_cs_files(scan_root: Path) -> list[Path]:
    result = subprocess.run(
        ["git", "diff", "--cached", "--name-only", "--diff-filter=ACMR"],
        capture_output=True, text=True, check=True,
    )
    prefix = scan_root.as_posix() + "/"
    return [Path(p) for p in result.stdout.splitlines()
            if p.endswith(".cs") and p.startswith(prefix)]


def all_cs_files(scan_root: Path) -> list[Path]:
    return list(scan_root.rglob("*.cs"))


def relative_posix(path: Path, scan_root: Path) -> str:
    try:
        return path.resolve().relative_to(scan_root.resolve()).as_posix()
    except ValueError:
        return path.as_posix()


def check_file(path: Path, scan_root: Path) -> list[tuple[int, str, str]]:
    """Return list of (lineno, path_literal, reason)."""
    violations: list[tuple[int, str, str]] = []
    rel = relative_posix(path, scan_root)
    in_features_test = rel.startswith(ALLOWED_DIR_PREFIX)
    try:
        content = path.read_text(encoding="utf-8")
    except (FileNotFoundError, UnicodeDecodeError):
        return violations

    for lineno, line in enumerate(content.splitlines(), start=1):
        stripped = line.lstrip()
        if stripped.startswith("//"):
            continue
        for match in ENDPOINT_PATTERN.finditer(line):
            url = match.group(1)
            if url in WHITELIST_PATHS:
                continue
            # Rule 1: /test/* without /api/ prefix
            if url.startswith("/test/"):
                violations.append((
                    lineno, url,
                    f"path '{url}' missing '/api/' prefix — must use '/api/test/*' (D-22)",
                ))
                continue
            # Rule 2: /api/test/* outside Features/Test/
            if url.startswith("/api/test/") and not in_features_test:
                violations.append((
                    lineno, url,
                    f"path '{url}' declared outside Features/Test/ (file: {rel}) — "
                    f"test-only endpoints MUST live in {ALLOWED_DIR_PREFIX} (D-22)",
                ))
    return violations


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="F-007 AC-C6 test endpoint namespace consistency scanner")
    parser.add_argument("--staged", action="store_true",
                        help="Scan only git staged .cs files under SCAN_ROOT")
    parser.add_argument("--scan-root", type=Path, default=DEFAULT_SCAN_ROOT,
                        help="Override scan root（pytest 機械測試用）")
    args = parser.parse_args(argv)

    scan_root: Path = args.scan_root
    files = staged_cs_files(scan_root) if args.staged else all_cs_files(scan_root)
    if not files:
        return 0

    total = 0
    for f in files:
        violations = check_file(f, scan_root)
        for lineno, url, reason in violations:
            print(f"{f}:{lineno}: {reason}", file=sys.stderr)
            total += 1

    if total > 0:
        print(
            f"\n[check-test-endpoints] {total} violation(s). "
            f"Test-only endpoints MUST live in Features/Test/ and use /api/test/* namespace.",
            file=sys.stderr,
        )
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
