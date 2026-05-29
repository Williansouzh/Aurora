using System.Reflection;
using Aurora.API.Controllers;
using Aurora.Application.Abstractions.Common;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Cache;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Aurora.Tests.Architecture;

public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ICacheService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(RedisCacheService).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AuthController).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Aurora.Application", "Aurora.Infrastructure", "Aurora.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Aurora.Infrastructure", "Aurora.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Aurora.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Api_should_not_be_referenced_by_core_layers()
    {
        var coreAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly };

        foreach (var assembly in coreAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn(ApiAssembly.GetName().Name)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
        }
    }

    private static string BuildFailureMessage(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
        {
            return "No failing types were reported.";
        }

        return "Invalid dependencies: " + string.Join(", ", result.FailingTypes.Select(x => x.FullName));
    }
}
