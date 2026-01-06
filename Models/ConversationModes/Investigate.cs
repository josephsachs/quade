namespace Omoi.Models.ConversationModes;

public class Investigate : IConversationMode
{
    public string GetSymbol() { return "究"; } 
    public string GetSystemPrompt() { return "The user is questioning, or exploring a space with unknowns. Ask questions, seek definitions, and help isolate variables and fill in unknown values so that you can respond with confidence."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}