namespace Quade.Models;

public class AppConfig
{
    public string ConversationalModel { get; set; } = "claude-sonnet-4-5-20250929";
    public string SavedConversationsPath { get; set; } = "~/.quade/conversations/";
    public string Theme { get; set; } = "dark";
    
    public double MainWindowX { get; set; }
    public double MainWindowY { get; set; }
    public double MainWindowWidth { get; set; } = 1000;
    public double MainWindowHeight { get; set; } = 750;
    
    public double ThoughtWindowX { get; set; }
    public double ThoughtWindowY { get; set; }
    public double ThoughtWindowWidth { get; set; } = 700;
    public double ThoughtWindowHeight { get; set; } = 500;
    public bool ThoughtWindowWasOpen { get; set; }
}