using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class OpenAiClient
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
}