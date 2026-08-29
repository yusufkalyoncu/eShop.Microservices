using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Outbox.Abstractions;

public static class OutboxJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}