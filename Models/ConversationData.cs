using System;
using System.Collections.Generic;

namespace Quade.Models;

public class ConversationData
{
    public List<Message> Messages { get; set; } = new();
    public DateTime SavedAt { get; set; }
    public ConversationMode CurrentMode { get; set; }
}