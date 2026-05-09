"""
F-007 AC-A2 機械驗證：用 tempfile 建虛擬 src/backend/ 樹，呼叫 main() 斷言 exit code。
不依賴 repo 真實檔案，可重現、可 CI 化。
"""
import importlib.util
from pathlib import Path

import pytest

# 用 importlib 動態載入帶連字號的檔名（Python 不能直接 import 帶 '-' 的模組名）
_script_path = Path(__file__).parent / "check-no-datetime-now.py"
_spec = importlib.util.spec_from_file_location("check_no_datetime_now", _script_path)
scanner = importlib.util.module_from_spec(_spec)  # type: ignore
_spec.loader.exec_module(scanner)  # type: ignore


@pytest.fixture
def fake_backend(tmp_path: Path) -> Path:
    """建一個 tmp_path/backend/ 模擬 src/backend/ 結構。"""
    root = tmp_path / "backend"
    (root / "ETOmniverse.Api").mkdir(parents=True)
    (root / "ETOmniverse.Infrastructure" / "Persistence" / "Migrations").mkdir(parents=True)
    return root


def test_clean_tree_exits_zero(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var time = timeProvider.GetUtcNow();\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_datetime_now_violation_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var x = DateTime.Now;\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_datetime_utcnow_violation_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var x = DateTime.UtcNow;\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_datetimeoffset_now_violation_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var x = DateTimeOffset.Now;\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_datetimeoffset_utcnow_violation_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var x = DateTimeOffset.UtcNow;\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_migrations_directory_excluded_exits_zero(fake_backend: Path):
    (fake_backend / "ETOmniverse.Infrastructure" / "Persistence" / "Migrations" / "Foo.cs").write_text(
        'var x = DateTime.UtcNow;\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_rationale_bypass_inline_comment_exits_zero(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var x = DateTime.UtcNow; // allow: enricher metadata\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_comment_only_line_does_not_trigger(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        '// example: DateTime.UtcNow\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0
