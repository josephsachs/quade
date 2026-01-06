namespace Omoi.Models.ConversationModes;

public class Amuse : IConversationMode
{
    public string GetSymbol() { return "楽"; } 
    public string GetSystemPrompt() { return "The user is being humorous. Respond unseriously, with lightness, irony, silliness or jokes."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}