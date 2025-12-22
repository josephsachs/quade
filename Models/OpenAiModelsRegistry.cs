/**
*
*   Hardcoded class for OpenAI model metadata since their /models endpoint does not 
*   return any regarding URIs, formats and capabilities and these vary between models.
*
*   This is less bad than any of the alternatives even though it is horrible. 
*   Fix your shit OpenAI.
*
**/

using System.Collections.Generic;

namespace Quade.Models;

public enum OpenAiEndpoint
{
    ChatCompletions,
    Responses,
    Completions,
    Embeddings
}

public enum OpenAiParameterFormat
{
    Standard,
    Reasoning
}

public class OpenAiModelCapabilities
{
    public OpenAiEndpoint Endpoint { get; set; }
    public OpenAiParameterFormat ParameterFormat { get; set; }
    public List<string> Categories { get; set; } = new();
    public bool SupportsVision { get; set; }
    public bool SupportsAudio { get; set; }
}

public static class OpenAiModelRegistry
{
    private static readonly Dictionary<string, OpenAiModelCapabilities> _registry = new()
    {
        // GPT-4o family - standard chat models with vision
        ["gpt-4o"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        ["gpt-4o-mini"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        
        // GPT-4.1 family - improved instruction following, 1M token context
        ["gpt-4.1"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        ["gpt-4.1-mini"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        ["gpt-4.1-nano"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        
        // Legacy GPT-4 models
        ["gpt-4-turbo"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        ["gpt-4"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" }
        },
        
        // GPT-3.5 Turbo
        ["gpt-3.5-turbo"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "chat", "thought" }
        },
        
        // O-series reasoning models
        ["o1-preview"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o1-mini"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o1-pro"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o1"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o3-mini"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o3-pro"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o3"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" }
        },
        ["o4-mini"] = new()
        {
            Endpoint = OpenAiEndpoint.ChatCompletions,
            ParameterFormat = OpenAiParameterFormat.Reasoning,
            Categories = new() { "chat", "thought" },
            SupportsVision = true
        },
        
        // Embedding models
        ["text-embedding-3-large"] = new()
        {
            Endpoint = OpenAiEndpoint.Embeddings,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "memory" }
        },
        ["text-embedding-3-small"] = new()
        {
            Endpoint = OpenAiEndpoint.Embeddings,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "memory" }
        },
        ["text-embedding-ada-002"] = new()
        {
            Endpoint = OpenAiEndpoint.Embeddings,
            ParameterFormat = OpenAiParameterFormat.Standard,
            Categories = new() { "memory" }
        }
    };

    public static OpenAiModelCapabilities? GetCapabilities(string modelId)
    {
        foreach (var (prefix, capabilities) in _registry)
        {
            if (modelId.StartsWith(prefix))
            {
                return capabilities;
            }
        }
        return null;
    }

    public static bool IsSupported(string modelId)
    {
        return GetCapabilities(modelId) != null;
    }
}