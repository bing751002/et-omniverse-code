namespace ETOmniverse.Api.Features.Common.Ping;

using FluentValidation;

/// <summary>
/// F-003 AC-3 / AC-7 — EchoRequest 的 FluentValidation validator。
/// 規則：Message 必填、長度 1-50 chars。
/// </summary>
public sealed class EchoRequestValidator : AbstractValidator<EchoRequest>
{
    public EchoRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(1).WithMessage("Message must be at least 1 character.")
            .MaximumLength(50).WithMessage("Message must be at most 50 characters.");
    }
}
