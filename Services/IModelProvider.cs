using System.Collections.Generic;
using System.Threading.Tasks;
using Omoi.Models;

namespace Omoi.Services;

public interface IModelProvider
{
    Task<string> SendMessageAsync(
        ModelRequestConfig config,
        List<Message> messages,
        string? systemPrompt = null);
        
    void SetApiKey(string apiKey);
}