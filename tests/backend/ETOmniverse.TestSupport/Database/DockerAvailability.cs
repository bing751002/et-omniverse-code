namespace ETOmniverse.TestSupport.Database;

internal static class DockerAvailability
{
    private static readonly Lazy<bool> s_isAvailable = new(CheckDockerAvailable);

    public static bool IsAvailable => s_isAvailable.Value;

    private static bool CheckDockerAvailable()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = System.Diagnostics.Process.Start(psi);
            return process is not null && process.WaitForExit(2_000) && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
