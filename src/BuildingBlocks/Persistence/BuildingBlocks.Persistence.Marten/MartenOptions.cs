using FluentValidation;

namespace BuildingBlocks.Persistence.Marten;

public class MartenOptions
{
    public const string SectionName = "Marten";

    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string Host { get; init; } = null!;
    public int Port { get; init; } = 5432;
    public string Database { get; init; } = null!;
    
    public string ConnectionString =>
        $"User ID={Username};Password={Password};Host={Host};Port={Port};Database={Database};";
        
    public string SchemaName { get; init; } = "public";
}

public class MartenOptionsValidator : AbstractValidator<MartenOptions>
{
    public MartenOptionsValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Database).NotEmpty();
        RuleFor(x => x.Port).GreaterThan(0);
        RuleFor(x => x.ConnectionString).NotEmpty();
        RuleFor(x => x.SchemaName).NotEmpty();
    }
}
