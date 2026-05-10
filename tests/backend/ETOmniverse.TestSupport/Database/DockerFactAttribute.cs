namespace ETOmniverse.TestSupport.Database;

using Xunit;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerAvailability.IsAvailable)
        {
            Skip = "Requires Docker daemon for Testcontainers MSSQL.";
        }
    }
}
