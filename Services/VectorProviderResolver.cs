using System;
using Omoi.Models;

namespace Omoi.Services;

public class VectorProviderResolver
{
    private readonly IVectorProvider _openAiProvider;

    public VectorProviderResolver(IVectorProvider openAiProvider)
    {
        _openAiProvider = openAiProvider;
    }

    public IVectorProvider GetProviderForModel(string modelId)
    {
        if (OpenAiModelRegistry.IsSupported(modelId))
        {
            return _openAiProvider;
        }

        throw new InvalidOperationException($"No vector provider registered for model: {modelId}");
    }
}