"""
F-007 AC-C6 機械驗證：用 tempfile 建虛擬 src/backend/ 樹，呼叫 main() 斷言 exit code。
"""
import importlib.util
from pathlib import Path

import pytest

_script_path = Path(__file__).parent / "check-test-endpoints.py"
_spec = importlib.util.spec_from_file_location("check_test_endpoints", _script_path)
scanner = importlib.util.module_from_spec(_spec)  # type: ignore
_spec.loader.exec_module(scanner)  # type: ignore


@pytest.fixture
def fake_backend(tmp_path: Path) -> Path:
    """建一個 tmp_path/backend/ 模擬 src/backend/ 結構。"""
    root = tmp_path / "backend"
    (root / "ETOmniverse.Api" / "Features" / "Common" / "Ping").mkdir(parents=True)
    (root / "ETOmniverse.Api" / "Features" / "Test").mkdir(parents=True)
    (root / "ETOmniverse.Api" / "Features" / "Test" / "Auth").mkdir(parents=True)
    (root / "ETOmniverse.Api" / "Middleware").mkdir(parents=True)
    return root


def test_clean_tree_with_centralized_extension_exits_zero(fake_backend: Path):
    # /api/test/throw 在 Features/Test/ 底下 → OK
    (fake_backend / "ETOmniverse.Api" / "Features" / "Test"
        / "MapTestOnlyEndpointsExtensions.cs").write_text(
        'app.MapGet("/api/test/throw", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_api_test_path_outside_features_test_exits_one(fake_backend: Path):
    # /api/test/foo 出現在 Middleware/ → 違規
    (fake_backend / "ETOmniverse.Api" / "Middleware" / "Some.cs").write_text(
        'app.MapGet("/api/test/foo", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_legacy_test_path_without_api_prefix_get_exits_one(fake_backend: Path):
    # 即使在 Features/Test/ 也不該用舊 /test/ namespace
    (fake_backend / "ETOmniverse.Api" / "Features" / "Test" / "Old.cs").write_text(
        'app.MapGet("/test/foo", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_legacy_test_path_anywhere_post_exits_one(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Middleware" / "Bad.cs").write_text(
        'app.MapPost("/test/bar", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 1


def test_ping_fail_whitelist_exits_zero(fake_backend: Path):
    # /api/common/ping/fail 在 Features/Common/Ping/ → 白名單 per D-22
    (fake_backend / "ETOmniverse.Api" / "Features" / "Common" / "Ping"
        / "PingEndpoints.cs").write_text(
        'app.MapGet("/api/common/ping/fail", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_api_test_auth_in_features_test_subdir_exits_zero(fake_backend: Path):
    # Features/Test/Auth/ 是 Features/Test/ 子目錄 → OK
    (fake_backend / "ETOmniverse.Api" / "Features" / "Test" / "Auth"
        / "TestAuthEndpoints.cs").write_text(
        'app.MapGet("/api/test/auth/whoami", (ClaimsPrincipal u) => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_api_test_in_features_test_subdir_exits_zero(fake_backend: Path):
    # Features/Test/Sub/Foo.cs → 子目錄都 OK
    (fake_backend / "ETOmniverse.Api" / "Features" / "Test" / "Sub").mkdir()
    (fake_backend / "ETOmniverse.Api" / "Features" / "Test" / "Sub" / "Foo.cs").write_text(
        'app.MapGet("/api/test/foo", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0


def test_comment_line_does_not_trigger(fake_backend: Path):
    (fake_backend / "ETOmniverse.Api" / "Middleware" / "WithComment.cs").write_text(
        '// example: app.MapGet("/test/leak", () => Results.Ok());\n',
        encoding="utf-8",
    )
    rc = scanner.main(["--scan-root", str(fake_backend)])
    assert rc == 0
