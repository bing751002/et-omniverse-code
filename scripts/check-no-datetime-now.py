#!/usr/bin/env python3
"""
F-007 AC-A2 / D-20: TimeProvider 全棧強制 — backend 禁區掃描器
禁止在 src/backend/**/*.cs 寫死：
  - DateTime.Now
  - DateTime.UtcNow
  - DateTimeOffset.Now
  - DateTimeOffset.UtcNow

排除：Migrations/、bin/、obj/（auto-generated / build output）
Rationale-bypass：同行加 inline `// allow: <reason>` 即豁免（per D-18）

Usage:
  python scripts/check-no-datetime-now.py                   # 全掃
  python scripts/check-no-datetime-now.py --staged          # 只掃 git staged .cs 檔
  python scripts/check-no-datetime-now.py --scan-root <p>   # 自訂 root（給 pytest 用）
"""
from __future__ import annotations
import argparse
import re
import subprocess
import sys
from pathlib import Path

FORBIDDEN_PATTERNS = [
    re.compile(r"\bDateTime\.Now\b"),
    re.compile(r"\bDateTime\.UtcNow\b"),
    re.compile(r"\bDateTimeOffset\.Now\b"),
    re.compile(r"\bDateTimeOffset\.UtcNow\b"),
]

# 同行 `// allow: <reason>` 視為 inline rationale-bypass（per D-18）
# 至少要有一個非空白字元當 reason，避免 `// allow:` 空白濫用
RATIONALE_BYPASS = re.compile(r"//\s*allow:\s*\S")

DEFAULT_SCAN_ROOT = Path("src/backend")

# 排除目錄（auto-generated / build output）
EXCLUDED_PATH_FRAGMENTS = ("/Migrations/", "/bin/", "/obj/")


def _is_excluded(path: Path) -> bool:
    posix = path.as_posix()
    return any(frag in posix for frag in EXCLUDED_PATH_FRAGMENTS)


def staged_cs_files(scan_root: Path) -> list[Path]:
    result = subprocess.run(
        ["git", "diff", "--cached", "--name-only", "--diff-filter=ACMR"],
        capture_output=True, text=True, check=True,
    )
    prefix = scan_root.as_posix() + "/"
    files = [Path(p) for p in result.stdout.splitlines()
             if p.endswith(".cs") and p.startswith(prefix)]
    return [p for p in files if not _is_excluded(p)]


def all_cs_files(scan_root: Path) -> list[Path]:
    return [p for p in scan_root.rglob("*.cs") if not _is_excluded(p)]


def check_file(path: Path) -> list[tuple[int, str, str]]:
    """Return list of (lineno, pattern_str, line_content) violations."""
    violations: list[tuple[int, str, str]] = []
    try:
        content = path.read_text(encoding="utf-8")
    except (FileNotFoundError, UnicodeDecodeError):
        return violations
    for lineno, line in enumerate(content.splitlines(), start=1):
        # 跳過純註解行（簡易：// 起手）
        stripped = line.lstrip()
        if stripped.startswith("//"):
            continue
        for pat in FORBIDDEN_PATTERNS:
            if pat.search(line):
                # inline rationale-bypass 豁免
                if RATIONALE_BYPASS.search(line):
                    continue
                violations.append((lineno, pat.pattern, line.strip()))
    return violations


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="F-007 AC-A2 forbidden DateTime.* scanner")
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
        violations = check_file(f)
        for lineno, pattern, line in violations:
            print(f"{f}:{lineno}: forbidden /{pattern}/ -> {line}", file=sys.stderr)
            total += 1

    if total > 0:
        print(f"\n[check-no-datetime-now] {total} violation(s) found. "
              f"Use TimeProvider via DI; legitimate exceptions need '// allow: <reason>' inline.",
              file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
