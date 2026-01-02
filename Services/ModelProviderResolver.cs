using System;
using Omoi.Models;

namespace Omoi.Services;

public class ModelProviderResolver
{
    private readonly IModelProvider _anthropicProvider;
    private readonly IModelProvider _openAiProvider;
    private readonly IModelProvider _deepSeekProvider;

    public ModelProviderResolver(IModelProvider anthropicProvider, IModelProvider openAiProvider, IModelProvider deepSeekProvider)
    {
        _anthropicProvider = anthropicProvider;
        _openAiProvider = openAiProvider;
        _deepSeekProvider = deepSeekProvider;
    }

    public IModelProvider GetProviderForModel(string modelId)
    {
        if (modelId.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
        {
            return _anthropicProvider;
        }

        if (modelId.StartsWith("deepseek-", StringComparison.OrdinalIgnoreCase))
        {
            return _deepSeekProvider;
        }

        if (OpenAiModelRegistry.IsSupported(modelId))
        {
            return _openAiProvider;
        }

        throw new InvalidOperationException($"No provider registered for model: {modelId}");
    }
}