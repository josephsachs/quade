using System;
using System.Collections.Generic;

namespace Quade.Services;

public class ModelProviderResolver
{
    private readonly Dictionary<string, IModelProvider> _providersByPrefix = new();

    public void RegisterProvider(string prefix, IModelProvider provider)
    {
        _providersByPrefix[prefix] = provider;
    }

    public IModelProvider GetProviderForModel(string modelId)
    {
        foreach (var (prefix, provider) in _providersByPrefix)
        {
            if (modelId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return provider;
            }
        }

        throw new InvalidOperationException($"No provider registered for model: {modelId}");
    }
}