using System;
using Quade.Models;

namespace Quade.Services;

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