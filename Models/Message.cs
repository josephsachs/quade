using System;

namespace Quade.Models;

public class Message
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public ConversationMode Mode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}