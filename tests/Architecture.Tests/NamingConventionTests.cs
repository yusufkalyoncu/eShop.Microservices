using FluentAssertions;
using NetArchTest.Rules;
using BuildingBlocks.Core.CQRS;
using Xunit.Abstractions;

namespace Architecture.Tests;

public class NamingConventionTests(ITestOutputHelper testOutputHelper) : BaseArchitectureTest
{
    [Fact]
    public void Commands_ShouldHave_CommandPostfix()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandlers_ShouldHave_HandlerPostfix()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        if (!result.IsSuccessful)
        {
            foreach(var type in result.FailingTypes)
                testOutputHelper.WriteLine($"Failing CommandHandler: {type.FullName}");
        }

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Queries_ShouldHave_QueryPostfix()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void QueryHandlers_ShouldHave_HandlerPostfix()
    {
        var result = Types.InAssemblies(MicroserviceAssemblies)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        if (!result.IsSuccessful)
        {
            foreach(var type in result.FailingTypes)
                testOutputHelper.WriteLine($"Failing QueryHandler: {type.FullName}");
        }

        result.IsSuccessful.Should().BeTrue();
    }
}