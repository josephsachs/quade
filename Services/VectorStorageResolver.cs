using System;
using Omoi.Models;

namespace Omoi.Services;

public class VectorStorageResolver
{
    private readonly IVectorStorage _supabaseStorage;
    private readonly IVectorStorage _qdrantStorage;

    public VectorStorageResolver(IVectorStorage supabaseStorage, IVectorStorage qdrantStorage)
    {
        _supabaseStorage = supabaseStorage;
        _qdrantStorage = qdrantStorage;
    }

    public IVectorStorage GetStorage(VectorStorageProvider provider)
    {
        return provider switch
        {
            VectorStorageProvider.Supabase => _supabaseStorage,
            VectorStorageProvider.Qdrant => _qdrantStorage,
            _ => throw new InvalidOperationException($"Unknown vector storage provider: {provider}")
        };
    }
}