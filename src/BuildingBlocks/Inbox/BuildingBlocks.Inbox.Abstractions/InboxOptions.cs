using FluentValidation;

namespace BuildingBlocks.Inbox.Abstractions;

public sealed class InboxOptions
{
    public int BatchSize { get; set; } = 100;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan PollTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxDegreeOfParallelism { get; set; } = 1;
    public TimeSpan ProcessedMessageRetention { get; set; } = TimeSpan.FromDays(14);
}

public class InboxOptionsValidator : AbstractValidator<InboxOptions>
{
    public InboxOptionsValidator()
    {
        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("Inbox BatchSize must be between 1 and 1000.");

        RuleFor(x => x.MaxRetryCount)
            .InclusiveBetween(0, 20)
            .WithMessage("Inbox MaxRetryCount must be between 0 and 20.");
    }
}