namespace ETOmniverse.Domain.Common.Model;

/// <summary>Unit type 給 non-content Result 使用（語意：成功但無回傳 value）。</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}

/// <summary>
/// Generic Result：domain operation 結果。Success → 帶 value；Failure → 帶
/// ErrorKind + code + message。Domain 不知 HTTP — HTTP status / ProblemDetails
/// 由 API 層 ResultHttpExtensions 處理（F-003 spec In scope）。
/// </summary>
public record Result<T>(bool IsSuccess, T? Value, ErrorKind? Kind, string? Code, string? Message)
{
    public static Result<T> Success(T value) => new(true, value, null, null, null);

    public static Result<T> Failure(string code, string message, ErrorKind kind) =>
        new(false, default, kind, code, message);
}

/// <summary>
/// Non-generic Result：Result&lt;Unit&gt; 的別名 / 子記錄，no-content 場景使用。
/// Helper / endpoint / use case 對 generic 統一型別呼叫，避免雙 API surface
/// （per CONTEXT D-A1）。
/// </summary>
public sealed record Result : Result<Unit>
{
    private Result(bool isSuccess, Unit value, ErrorKind? kind, string? code, string? message)
        : base(isSuccess, value, kind, code, message)
    {
    }

    public static Result Success() => new(true, Unit.Value, null, null, null);

    public static new Result Failure(string code, string message, ErrorKind kind) =>
        new(false, default, kind, code, message);
}
