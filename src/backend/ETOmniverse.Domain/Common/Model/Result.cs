namespace ETOmniverse.Domain.Common.Model;

public sealed record Result(bool IsSuccess, string? ErrorCode = null, string? ErrorMessage = null)
{
  public static Result Success() => new(true);

  public static Result Failure(string errorCode, string errorMessage) =>
    new(false, errorCode, errorMessage);
}
