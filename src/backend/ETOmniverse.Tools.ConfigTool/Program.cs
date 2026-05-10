using System.Text.Json;
using System.Text.Json.Nodes;

var parsed = Args.Parse(args);
if (parsed.ShowHelp)
{
    await Console.Out.WriteLineAsync("""
        ET-Omniverse ConfigTool

        Usage:
          validate [--environment <name>] [--root <repo-root>]
          print --redacted [--environment <name>] [--root <repo-root>]
        """);
    return 0;
}

var repoRoot = parsed.Root ?? FindRepoRoot();
var config = ConfigLoader.Load(repoRoot, parsed.Environment);

return parsed.Command switch
{
    "validate" => await Validate(config),
    "print" when parsed.Redacted => await Print(config, redacted: true),
    _ => await Fail("Unknown command. Use --help for usage.")
};

static async Task<int> Validate(JsonObject config)
{
    var errors = new List<string>();

    RequireNonEmpty(config, "ConnectionStrings:Default", errors);
    RequireAbsoluteUri(config, "Errors:TypeBaseUrl", errors);
    RequireAbsoluteUri(config, "ExternalServices:SampleEcho:BaseUrl", errors);
    RequirePositiveInt(config, "Logging:RequestBody:MaxBytes", errors);
    RequirePositiveInt(config, "ExternalServices:SampleEcho:TimeoutSeconds", errors);
    RequirePositiveInt(config, "ExternalServices:SampleEcho:Retry:MaxAttempts", errors);
    RequirePositiveInt(config, "ExternalServices:SampleEcho:Retry:BaseDelayMs", errors);

    if (errors.Count > 0)
    {
        foreach (var error in errors)
        {
            await Console.Error.WriteLineAsync(error);
        }
        return 1;
    }

    await Console.Out.WriteLineAsync("OK config validation passed");
    return 0;
}

static async Task<int> Print(JsonObject config, bool redacted)
{
    var output = redacted ? Redact(config.DeepClone()) : config;
    await Console.Out.WriteLineAsync(output.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}

static async Task<int> Fail(string message)
{
    await Console.Error.WriteLineAsync(message);
    return 1;
}

static void RequireNonEmpty(JsonObject config, string path, List<string> errors)
{
    var value = GetPath(config, path)?.GetValue<string>();
    if (string.IsNullOrWhiteSpace(value))
    {
        errors.Add($"Missing required config: {path}");
    }
}

static void RequireAbsoluteUri(JsonObject config, string path, List<string> errors)
{
    var value = GetPath(config, path)?.GetValue<string>();
    if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value, UriKind.Absolute, out _))
    {
        errors.Add($"Config must be an absolute URI: {path}");
    }
}

static void RequirePositiveInt(JsonObject config, string path, List<string> errors)
{
    var node = GetPath(config, path);
    if (node is null || !TryGetInt(node, out var value) || value <= 0)
    {
        errors.Add($"Config must be a positive integer: {path}");
    }
}

static bool TryGetInt(JsonNode node, out int value)
{
    try
    {
        value = node.GetValue<int>();
        return true;
    }
    catch (InvalidOperationException)
    {
        value = 0;
        return false;
    }
    catch (FormatException)
    {
        value = 0;
        return false;
    }
}

static JsonNode? GetPath(JsonObject config, string path)
{
    JsonNode? current = config;
    foreach (var segment in path.Split(':', StringSplitOptions.RemoveEmptyEntries))
    {
        current = current?[segment];
    }
    return current;
}

static JsonNode Redact(JsonNode node)
{
    if (node is JsonObject obj)
    {
        foreach (var property in obj.ToArray())
        {
            obj[property.Key] = IsSecretKey(property.Key)
                ? "***REDACTED***"
                : property.Value is null ? null : Redact(property.Value);
        }
    }
    else if (node is JsonArray arr)
    {
        for (var i = 0; i < arr.Count; i++)
        {
            if (arr[i] is not null)
            {
                arr[i] = Redact(arr[i]!);
            }
        }
    }
    return node;
}

static bool IsSecretKey(string key) =>
    key.Contains("password", StringComparison.OrdinalIgnoreCase)
    || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
    || key.Contains("token", StringComparison.OrdinalIgnoreCase)
    || key.Contains("apikey", StringComparison.OrdinalIgnoreCase)
    || key.Contains("api_key", StringComparison.OrdinalIgnoreCase)
    || key.Contains("connectionstring", StringComparison.OrdinalIgnoreCase);

static string FindRepoRoot()
{
    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "ETOmniverse.sln")))
        {
            return current.FullName;
        }
        current = current.Parent;
    }
    return Directory.GetCurrentDirectory();
}

internal sealed record Args(string Command, string? Environment, string? Root, bool Redacted, bool ShowHelp)
{
    public static Args Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            return new Args("", null, null, false, true);
        }

        var command = args[0];
        string? environment = null;
        string? root = null;
        var redacted = false;

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--environment" when i + 1 < args.Length:
                    environment = args[++i];
                    break;
                case "--root" when i + 1 < args.Length:
                    root = args[++i];
                    break;
                case "--redacted":
                    redacted = true;
                    break;
            }
        }

        return new Args(command, environment, root, redacted, ShowHelp: false);
    }
}

internal static class ConfigLoader
{
    public static JsonObject Load(string repoRoot, string? environment)
    {
        var config = ReadJson(Path.Combine(repoRoot, "src", "backend", "ETOmniverse.Api", "appsettings.json"));
        if (!string.IsNullOrWhiteSpace(environment))
        {
            var envPath = Path.Combine(repoRoot, "src", "backend", "ETOmniverse.Api", $"appsettings.{environment}.json");
            if (File.Exists(envPath))
            {
                Merge(config, ReadJson(envPath));
            }
        }
        return config;
    }

    private static JsonObject ReadJson(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonNode.Parse(stream)?.AsObject()
            ?? throw new InvalidOperationException($"Invalid JSON config: {path}");
    }

    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            if (value is JsonObject sourceObj && target[key] is JsonObject targetObj)
            {
                Merge(targetObj, sourceObj);
                continue;
            }
            target[key] = value?.DeepClone();
        }
    }
}
