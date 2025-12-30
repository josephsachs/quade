namespace Omoi.Models;

public class ModelRequestConfig
{
    public string Model { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 4096;
}