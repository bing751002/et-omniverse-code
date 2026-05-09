namespace ETOmniverse.Common.Logging;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Hardcoded baseline mask fields (per F-002 AC-4)。
/// appsettings 只能透過 Logging:Mask:AdditionalFields 加欄位，
/// 不能覆蓋（spec Q-F002-002 已 resolved — ops 手滑清空 = secret leak）。
/// </summary>
public static class MaskFields
{
    public static IReadOnlySet<string> Baseline { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "token",
            "apiKey",
            "secret",
            "authorization",
            "cookie",
            "x-api-key",
        };

    public const string MaskedValue = "***";

    public static IReadOnlySet<string> GetEffectiveSet(IEnumerable<string>? additional)
    {
        var set = new HashSet<string>(Baseline, StringComparer.OrdinalIgnoreCase);
        if (additional is not null)
        {
            foreach (var f in additional.Where(s => !string.IsNullOrWhiteSpace(s)))
                set.Add(f);
        }
        return set;
    }
}
