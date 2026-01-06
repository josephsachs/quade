namespace Omoi.Models.ConversationModes;

public class Soothe : IConversationMode
{
    public string GetSymbol() { return "助"; } 
    public string GetSystemPrompt() { return "The user is distressed or anxious. Provide solidarity and comfort. Optimism, reassurance and hope about them or their wishes are appropriate, provided the wishes are themselves safe."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}