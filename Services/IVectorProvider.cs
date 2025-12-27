using System.Threading.Tasks;

namespace Quade.Services;

public interface IVectorProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
    void SetApiKey(string apiKey);
}