namespace Quade.Models;

public class AppConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ConversationalModel { get; set; } = "claude-sonnet-4-5-20250929";
    public string SavedConversationsPath { get; set; } = "~/.quade/conversations/";
    public string Theme { get; set; } = "dark";
}