using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class OpenAiClient : IModelProvider
{
    private readonly HttpClient _httpClient;
    private const string BASE_URL = "https://api.openai.com/v1";

    public OpenAiClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BASE_URL);
    }

    public void SetApiKey(string apiKey)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<List<ModelInfo>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync("https://api.openai.com/v1/models");
        
        var content = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();
        
        var result = JsonSerializer.Deserialize<ModelsResponse>(content);
        
        var models = new List<ModelInfo>();
        
        if (result?.Data == null)
            return models;

        foreach (var model in result.Data)
        {
            var categories = new List<string>();
            
            if (model.Id.Contains("embedding"))
            {
                categories.Add("memory");
            }
            else if (model.Id.StartsWith("gpt-"))
            {
                categories.Add("chat");
                categories.Add("thought");
            }

            if (categories.Count > 0)
            {
                models.Add(new ModelInfo
                {
                    Id = model.Id,
                    DisplayName = FormatDisplayName(model.Id),
                    Type = "model",
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(model.Created).DateTime,
                    Categories = categories
                });
            }
        }
        
        return models.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task<string> SendMessageAsync(
        ModelRequestConfig config,
        List<Message> messages,
        string? systemPrompt = null)
    {
        var apiMessages = new List<object>();
        
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            apiMessages.Add(new
            {
                role = "system",
                content = systemPrompt
            });
        }
        
        apiMessages.AddRange(messages.Select(m => new
        {
            role = m.IsUser ? "user" : "assistant",
            content = m.Content
        }));

        var request = new
        {
            model = config.Model,
            max_tokens = config.MaxTokens,
            messages = apiMessages.ToArray()
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorJson = JsonSerializer.Deserialize<JsonDocument>(errorContent);
            var errorMessage = errorJson?.RootElement.GetProperty("error").GetProperty("message").GetString() 
                ?? "Unknown API error";
            throw new HttpRequestException(errorMessage);
        }
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);
        
        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    private string FormatDisplayName(string modelId)
    {
        return modelId switch
        {
            var id when id.Contains("gpt-4o") => "GPT-4o",
            var id when id.Contains("gpt-4-turbo") => "GPT-4 Turbo",
            var id when id.Contains("gpt-4") => "GPT-4",
            var id when id.Contains("gpt-3.5-turbo") => "GPT-3.5 Turbo",
            var id when id.Contains("text-embedding-3-large") => "Text Embedding 3 Large",
            var id when id.Contains("text-embedding-3-small") => "Text Embedding 3 Small",
            var id when id.Contains("text-embedding-ada-002") => "Text Embedding Ada 002",
            _ => modelId
        };
    }

    private class ModelsResponse
    {
        [JsonPropertyName("data")]
        public List<OpenAiModel> Data { get; set; } = new();
    }

    private class OpenAiModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("created")]
        public long Created { get; set; }
    }

    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new();
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public MessageContent Message { get; set; } = new();
    }

    private class MessageContent
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}