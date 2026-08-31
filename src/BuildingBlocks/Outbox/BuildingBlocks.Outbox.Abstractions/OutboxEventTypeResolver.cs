using System.Collections.Concurrent;
using System.Reflection;

namespace BuildingBlocks.Outbox.Abstractions;

public static class OutboxEventTypeResolver
{
    private static readonly Lazy<Dictionary<string, Type>> AllEventTypes = new(() =>
    {
        var map = new Dictionary<string, Type>();
        
        foreach (var type in GetAllTypes())
        {
            if (type.IsInterface || type.IsAbstract || !typeof(IOutboxEvent).IsAssignableFrom(type))
                continue;

            var property = type.GetProperty(nameof(IOutboxEvent.EventName), 
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            
            if (property?.GetValue(null) is string name && !string.IsNullOrWhiteSpace(name))
            {
                map[name] = type;
            }
        }

        return map;
    });

    private static readonly ConcurrentDictionary<string, Type?> NameToTypeMap = new();

    public static Type? GetEventType(string eventName)
    {
        return NameToTypeMap.GetOrAdd(eventName, name => 
        {
            var type = Type.GetType(name);
            if (type != null) return type;
            
            return AllEventTypes.Value.GetValueOrDefault(name);
        });
    }

    private static IEnumerable<Type> GetAllTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                yield return type;
            }
        }
    }
}