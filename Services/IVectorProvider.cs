using System.Threading.Tasks;

namespace Omoi.Services;

public interface IVectorProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
    void SetApiKey(string apiKey);
}