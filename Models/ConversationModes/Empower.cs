namespace Omoi.Models.ConversationModes;

public class Empower : IConversationMode
{
    public string GetSymbol() { return "力"; } 
    public string GetSystemPrompt() { return "The user is sharing ideas and thoughts. Be encouraging and enthusiastic in a thoughtfully engaged way."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}