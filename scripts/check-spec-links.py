"""
Verify that all `src/...` paths referenced in docs/specs/*.md actually exist.

Usage: python scripts/check-spec-links.py

Exits 0 if all links resolve, 1 with list of broken refs otherwise.
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
SPECS_DIR = REPO_ROOT / "docs" / "specs"

PATH_PATTERN = re.compile(r"`(src[/\\][^`\s]+?)`")
EXCLUDE_NAMES = {"_template.md", "README.md"}
PLACEHOLDER_FRAGMENTS = ("<", ">", "...", "Module", "Feature", "XXX")


def is_placeholder(p: str) -> bool:
    return any(frag in p for frag in PLACEHOLDER_FRAGMENTS)


def walk_specs() -> list[Path]:
    if not SPECS_DIR.exists():
        return []
    return [
        p for p in SPECS_DIR.rglob("*.md")
        if p.name not in EXCLUDE_NAMES
    ]


def main() -> int:
    files = walk_specs()
    if not files:
        print("INFO no spec files yet (docs/specs/F-*.md is empty)")
        return 0

    broken: list[tuple[Path, int, str]] = []
    checked = 0

    for path in files:
        for lineno, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            for m in PATH_PATTERN.finditer(line):
                ref = m.group(1).replace("\\", "/")
                if is_placeholder(ref):
                    continue
                checked += 1
                target = REPO_ROOT / ref
                if not target.exists():
                    broken.append((path.relative_to(REPO_ROOT), lineno, ref))

    if broken:
        print(f"ERR  {len(broken)} broken spec link(s):", file=sys.stderr)
        for f, ln, ref in broken:
            print(f"     {f}:{ln}  ->  {ref}", file=sys.stderr)
        return 1

    plural = "s" if len(files) != 1 else ""
    print(f"OK   {checked} spec link(s) verified across {len(files)} file{plural}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
