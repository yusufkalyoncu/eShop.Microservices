using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Inbox.Abstractions;

public static class InboxJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}