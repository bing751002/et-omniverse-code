namespace ETOmniverse.TestSupport.Database;

using Xunit;

public sealed class DockerTheoryAttribute : TheoryAttribute
{
    public DockerTheoryAttribute()
    {
        if (!DockerAvailability.IsAvailable)
        {
            Skip = "Requires Docker daemon for Testcontainers MSSQL.";
        }
    }
}
