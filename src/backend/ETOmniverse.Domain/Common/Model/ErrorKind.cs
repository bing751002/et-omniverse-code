namespace ETOmniverse.Domain.Common.Model;

/// <summary>
/// Domain-level error category. HTTP-agnostic — HTTP status mapping 由 API 層
/// ResultHttpExtensions 處理（per F-003 spec In scope）。7 個值固定：API surface
/// stable，禁止任意增刪。要加新 kind 需開 ADR。
/// </summary>
public enum ErrorKind
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    ExternalDependency,
    Unexpected
}
