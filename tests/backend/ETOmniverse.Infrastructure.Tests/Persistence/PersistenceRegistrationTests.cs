namespace ETOmniverse.Infrastructure.Tests.Persistence;

using ETOmniverse.Domain.Common.Ports;
using ETOmniverse.Infrastructure.DependencyInjection;
using ETOmniverse.Infrastructure.Identity;
using ETOmniverse.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class PersistenceRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersDbContextUnitOfWorkAndRepository()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=localhost;Database=ETOmniverseTest;Trusted_Connection=True;TrustServerCertificate=True",
                ["ExternalServices:SampleEcho:BaseUrl"] = "http://localhost"
            })
            .Build();

        services.AddETOmniverseInfrastructure(configuration);

        services.Should().Contain(d => d.ServiceType == typeof(EtOmniverseDbContext));
        services.Should().Contain(d => d.ServiceType == typeof(ICurrentUser)
            && d.ImplementationType == typeof(HttpContextCurrentUser)
            && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(IUnitOfWork) && d.ImplementationType == typeof(UnitOfWork));
        services.Should().Contain(d => d.ServiceType == typeof(IRepository<>) && d.ImplementationType == typeof(RepositoryBase<>));
    }

    [Fact]
    public void AddInfrastructure_FailsFastWhenConnectionStringMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalServices:SampleEcho:BaseUrl"] = "http://localhost"
            })
            .Build();

        var act = () => services.AddETOmniverseInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:Default*");
    }

    [Fact]
    public void DomainPorts_DoNotReferenceEfCore()
    {
        typeof(IRepository<>).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should()
            .NotContain(name => name != null && name.StartsWith("Microsoft.EntityFrameworkCore"));
    }
}
