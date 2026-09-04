using FluentValidation;

namespace BuildingBlocks.Persistence.PostgreSql;

public sealed class PostgresDbContextOptions
{
    public const string SectionName = "Postgres";

    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string Host { get; init; } = null!;
    public int Port { get; init; } = 5432;
    public string Database { get; init; } = null!;

    public string ConnectionString =>
        $"User ID={Username};Password={Password};Host={Host};Port={Port};Database={Database};";

    public int MaxRetryCount { get; init; } = 3;
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(5);
    public bool UseSnakeCaseNamingConvention { get; init; } = true;
}

public class PostgresDbContextOptionsValidator : AbstractValidator<PostgresDbContextOptions>
{
    public PostgresDbContextOptionsValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Database).NotEmpty();
        RuleFor(x => x.Port).GreaterThan(0);
        RuleFor(x => x.ConnectionString).NotEmpty();
    }
}