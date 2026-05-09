namespace ETOmniverse.Common.Http;

public static class OutboundHttpLogProperties
{
    public const string ServiceName = "OutboundServiceName";
    public const string PathTemplate = "OutboundPathTemplate";
    public const string RetryAttempt = "OutboundRetryAttempt";

    public static readonly HttpRequestOptionsKey<string> ServiceNameKey = new(ServiceName);
    public static readonly HttpRequestOptionsKey<string> PathTemplateKey = new(PathTemplate);
    public static readonly HttpRequestOptionsKey<int> RetryAttemptKey = new(RetryAttempt);
}
