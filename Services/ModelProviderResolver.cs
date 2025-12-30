using System;
using Omoi.Models;

namespace Omoi.Services;

public class ModelProviderResolver
{
    private readonly IModelProvider _anthropicProvider;
    private readonly IModelProvider _openAiProvider;

    public ModelProviderResolver(IModelProvider anthropicProvider, IModelProvider openAiProvider)
    {
        _anthropicProvider = anthropicProvider;
        _openAiProvider = openAiProvider;
    }

    public IModelProvider GetProviderForModel(string modelId)
    {
        if (modelId.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
        {
            return _anthropicProvider;
        }

        if (OpenAiModelRegistry.IsSupported(modelId))
        {
            return _openAiProvider;
        }

        throw new InvalidOperationException($"No provider registered for model: {modelId}");
    }
}