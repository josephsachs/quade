using System;

namespace Quade.Models;

public class Message
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public ConversationMode Mode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string IconText => IsUser ? "私" : Mode switch
    {
        ConversationMode.Empower => "力",
        ConversationMode.Investigate => "究",
        ConversationMode.Opine => "思",
        ConversationMode.Critique => "批",
        _ => "?"
    };

    public string IconColor => IsUser ? "#A8A8A8" : "#90EE90";
}