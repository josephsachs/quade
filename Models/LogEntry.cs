using System;

namespace Quade.Models;

public enum LogEntryType
{
    ModeDetectionStart,
    ModePrompt,
    ModeResponse,
    SystemPrompt,
    Info
}

public class LogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public LogEntryType Type { get; set; }
    public string Content { get; set; } = string.Empty;
}