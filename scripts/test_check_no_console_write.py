"""
F-002 AC-8 機械驗證：用 tempfile 建虛擬 src/backend/ 樹，呼叫 main() 斷言 exit code。
不依賴 repo 真實檔案，可重現、可 CI 化。
"""
import importlib.util
import sys
from pathlib import Path

import pytest

# 用 importlib 動態載入帶連字號的檔名（Python 不能直接 import 帶 '-' 的模組名）
_script_path = Path(__file__).parent / "check-no-console-write.py"
_spec = importlib.util.spec_from_file_location("check_no_console_write", _script_path)
scanner = importlib.util.module_from_spec(_spec)  # type: ignore
_spec.loader.exec_module(scanner)  # type: ignore


@pytest.fixture
def fake_backend(tmp_path: Path) -> Path:
    """建一個 tmp_path/backend/ 模擬 src/backend/ 結構。"""
    root = tmp_path / "backend"
    (root / "ETOmniverse.Common" / "Logging").mkdir(parents=True)
    (root / "ETOmniverse.Api").mkdir(parents=True)
    return root


def test_clean_tree_exits_zero(fake_backend: Path):
    # 純淨：BootstrapLogger 自身可以含 Console（白名單），其他檔不含禁區字樣
    (fake_backend / "ETOmniverse.Common" / "Logging" / "BootstrapLogger.cs").write_text(
        'WriteTo.Console(new RenderedCompactJsonFormatter());\n', encoding="utf-8"
    )
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'var builder = WebApplication.CreateBuilder(args);\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_violation_in_business_code_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'Console.WriteLine("oops");\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_serilog_log_static_call_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'Serilog.Log.Information("static call leak");\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_debug_writeline_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        'Debug.WriteLine("dev only leak");\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_bootstrap_logger_whitelist_exits_zero(fake_backend: Path):
    # BootstrapLogger 自身可以含禁區字樣（spec 明示例外）
    (fake_backend / "ETOmniverse.Common" / "Logging" / "BootstrapLogger.cs").write_text(
        'Console.WriteLine("bootstrap path is allowed");\n'
        'Serilog.Log.Logger = new LoggerConfiguration()...\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_comment_line_does_not_trigger(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Program.cs").write_text(
        '// example: Console.WriteLine("foo");\n', encoding="utf-8"
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0
