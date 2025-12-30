using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Omoi.Models;

public class ModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();
}