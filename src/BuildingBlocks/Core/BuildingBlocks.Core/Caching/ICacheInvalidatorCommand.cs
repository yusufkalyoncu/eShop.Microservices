namespace BuildingBlocks.Core.Caching;

public interface ICacheInvalidatorCommand
{
    IEnumerable<string> CacheKeys { get; }
}