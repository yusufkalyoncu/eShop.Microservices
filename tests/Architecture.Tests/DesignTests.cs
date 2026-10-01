using FluentAssertions;
using NetArchTest.Rules;
using BuildingBlocks.Core.Domain;
using Xunit.Abstractions;

namespace Architecture.Tests;

public class DesignTests(ITestOutputHelper testOutputHelper) : BaseArchitectureTest
{
    [Fact]
    public void Entities_ShouldResideIn_DomainNamespace()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .Inherit(typeof(Entity<>))
            .Should()
            .ResideInNamespaceMatching(".*\\.Domain\\..*")
            .GetResult();
        
        if (!result.IsSuccessful)
        {
            foreach(var type in result.FailingTypes)
                testOutputHelper.WriteLine($"Failing Entity: {type.FullName}");
        }

        result.IsSuccessful.Should().BeTrue();
    }
}