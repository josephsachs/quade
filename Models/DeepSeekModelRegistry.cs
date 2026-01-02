using System.Collections.Generic;

namespace Omoi.Models;

public enum DeepSeekEndpoint
{
    ChatCompletions
}

public class DeepSeekModelCapabilities
{
    public DeepSeekEndpoint Endpoint { get; set; }
    public List<string> Categories { get; set; } = new();
}

public static class DeepSeekModelRegistry
{
    private static readonly Dictionary<string, DeepSeekModelCapabilities> _registry = new()
    {
        ["deepseek-chat"] = new()
        {
            Endpoint = DeepSeekEndpoint.ChatCompletions,
            Categories = new() { "chat", "thought", "memory" }
        }
    };

    public static DeepSeekModelCapabilities? GetCapabilities(string modelId)
    {
        return _registry.TryGetValue(modelId, out var capabilities) ? capabilities : null;
    }

    public static bool IsSupported(string modelId)
    {
        return _registry.ContainsKey(modelId);
    }

    public static IEnumerable<KeyValuePair<string, DeepSeekModelCapabilities>> GetAllModels()
    {
        return _registry;
    }
}