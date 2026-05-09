#!/usr/bin/env python3
"""
F-002 AC-8: 業務 code 禁區掃描器
禁止在 src/backend/**/*.cs 寫死：
  - Console.WriteLine
  - Serilog.Log.   (Logger 取得只能透過 ILogger<T> DI)
  - Debug.WriteLine
唯一例外：src/backend/ETOmniverse.Common/Logging/BootstrapLogger.cs（spec 明示）

Usage:
  python scripts/check-no-console-write.py                   # 全掃
  python scripts/check-no-console-write.py --staged          # 只掃 git staged .cs 檔
  python scripts/check-no-console-write.py --scan-root <p>   # 自訂 root（給 pytest 用）
"""
from __future__ import annotations
import argparse
import re
import subprocess
import sys
from pathlib import Path

FORBIDDEN_PATTERNS = [
    re.compile(r"\bConsole\.WriteLine\b"),
    re.compile(r"\bSerilog\.Log\."),
    re.compile(r"\bDebug\.WriteLine\b"),
]

DEFAULT_SCAN_ROOT = Path("src/backend")
# 白名單以「相對 SCAN_ROOT 的 posix 字串」表示，pytest 可注入自訂 SCAN_ROOT 而不必跟整 repo path 對齊
WHITELIST_RELATIVE = {
    "ETOmniverse.Common/Logging/BootstrapLogger.cs",
}


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


def is_whitelisted(path: Path, scan_root: Path) -> bool:
    try:
        rel = path.resolve().relative_to(scan_root.resolve()).as_posix()
    except ValueError:
        return False
    return rel in WHITELIST_RELATIVE


def check_file(path: Path, scan_root: Path) -> list[tuple[int, str, str]]:
    """Return list of (lineno, pattern_str, line_content) violations."""
    violations: list[tuple[int, str, str]] = []
    if is_whitelisted(path, scan_root):
        return violations
    try:
        content = path.read_text(encoding="utf-8")
    except (FileNotFoundError, UnicodeDecodeError):
        return violations
    for lineno, line in enumerate(content.splitlines(), start=1):
        # 跳過註解行（簡易：// 起手）
        stripped = line.lstrip()
        if stripped.startswith("//"):
            continue
        for pat in FORBIDDEN_PATTERNS:
            if pat.search(line):
                violations.append((lineno, pat.pattern, line.strip()))
    return violations


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="F-002 AC-8 forbidden API scanner")
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
        for lineno, pattern, line in violations:
            print(f"{f}:{lineno}: forbidden /{pattern}/ -> {line}", file=sys.stderr)
            total += 1

    if total > 0:
        print(f"\n[check-no-console-write] {total} violation(s) found. "
              f"Use ILogger<T> via DI; bootstrap path uses BootstrapLogger.",
              file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
