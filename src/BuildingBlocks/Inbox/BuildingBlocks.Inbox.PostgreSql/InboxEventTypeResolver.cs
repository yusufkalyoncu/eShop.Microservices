using System.Collections.Concurrent;
using System.Reflection;
using BuildingBlocks.Messaging.Abstractions;

namespace BuildingBlocks.Inbox.PostgreSql;

public static class InboxEventTypeResolver
{
    private static readonly Lazy<Dictionary<string, Type>> AllEventTypes = new(() =>
    {
        var map = new Dictionary<string, Type>();
        
        foreach (var type in GetAllTypes())
        {
            if (type.IsInterface || type.IsAbstract || !typeof(IIntegrationEvent).IsAssignableFrom(type))
                continue;

            var property = type.GetProperty(nameof(IIntegrationEvent.EventName), 
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
        return NameToTypeMap.GetOrAdd(
            eventName,
            name => AllEventTypes.Value.GetValueOrDefault(name));
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