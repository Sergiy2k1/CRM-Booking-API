using System.Reflection;
using Xunit;

namespace BookingHub.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly =
        typeof(Domain.Abstractions.Entity).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(Application.DependencyInjection).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void DomainShouldNotReferenceApplication()
    {
        AssertDoesNotReference(
            DomainAssembly,
            "BookingHub.Application");
    }

    [Fact]
    public void DomainShouldNotReferenceInfrastructure()
    {
        AssertDoesNotReference(
            DomainAssembly,
            "BookingHub.Infrastructure");
    }

    [Fact]
    public void DomainShouldNotReferenceApi()
    {
        AssertDoesNotReference(
            DomainAssembly,
            "BookingHub.Api");
    }

    [Fact]
    public void DomainShouldNotReferenceEntityFrameworkCore()
    {
        AssertDoesNotReferencePrefix(
            DomainAssembly,
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void ApplicationShouldNotReferenceInfrastructure()
    {
        AssertDoesNotReference(
            ApplicationAssembly,
            "BookingHub.Infrastructure");
    }

    [Fact]
    public void ApplicationShouldNotReferenceApi()
    {
        AssertDoesNotReference(
            ApplicationAssembly,
            "BookingHub.Api");
    }

    [Fact]
    public void ApplicationShouldNotReferenceEntityFrameworkCore()
    {
        AssertDoesNotReferencePrefix(
            ApplicationAssembly,
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void ApplicationShouldNotReferenceRabbitMq()
    {
        AssertDoesNotReferencePrefix(
            ApplicationAssembly,
            "RabbitMQ");
    }

    [Fact]
    public void InfrastructureShouldNotReferenceApi()
    {
        AssertDoesNotReference(
            InfrastructureAssembly,
            "BookingHub.Api");
    }

    private static void AssertDoesNotReference(
        Assembly assembly,
        string forbiddenAssemblyName)
    {
        var referencedAssemblyNames =
            assembly
                .GetReferencedAssemblies()
                .Select(x => x.Name)
                .Where(x => x is not null)
                .ToArray();

        Assert.DoesNotContain(
            forbiddenAssemblyName,
            referencedAssemblyNames);
    }

    private static void AssertDoesNotReferencePrefix(
        Assembly assembly,
        string forbiddenAssemblyNamePrefix)
    {
        var forbiddenReference =
            assembly
                .GetReferencedAssemblies()
                .Select(x => x.Name)
                .FirstOrDefault(
                    x =>
                        x is not null &&
                        x.StartsWith(
                            forbiddenAssemblyNamePrefix,
                            StringComparison.Ordinal));

        Assert.Null(forbiddenReference);
    }
}
