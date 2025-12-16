using System.Collections.Generic;
using System.Linq;
using Quade.Models;

namespace Quade.Services;

public class ChatContextBuilder
{
    private const int MAX_CONTEXT_MESSAGES = 16;

    public List<Message> BuildContext(List<Message> allMessages)
    {
        if (allMessages.Count <= MAX_CONTEXT_MESSAGES)
        {
            return allMessages;
        }

        return allMessages.TakeLast(MAX_CONTEXT_MESSAGES).ToList();
    }
}