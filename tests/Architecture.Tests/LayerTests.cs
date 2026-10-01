using FluentAssertions;
using NetArchTest.Rules;
using Xunit.Abstractions;

namespace Architecture.Tests;

public class LayerTests(ITestOutputHelper testOutputHelper) : BaseArchitectureTest
{
    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_OtherLayers()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .ResideInNamespace("Domain") 
            .ShouldNot()
            .HaveDependencyOnAny("Features", "Infrastructure", "Application")
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                testOutputHelper.WriteLine($"Failing Type: {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue();
    }
}