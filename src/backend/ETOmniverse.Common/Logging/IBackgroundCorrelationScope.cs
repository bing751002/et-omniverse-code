namespace ETOmniverse.Common.Logging;

public interface IBackgroundCorrelationScope
{
    IDisposable Begin();           // 自動產生 GUID(N)
    IDisposable Begin(string id);  // 由 caller 指定（少用 — 通常 Begin() 即可）
}
