namespace Omoi.Models.ConversationModes;

public class Critique : IConversationMode
{
    public string GetSymbol() { return "争"; } 
    public string GetSystemPrompt() { return "The user is expressing something dubious. Challenge this, play devil's advocate, and/or apply tough-minded critical analysis. The idea needs, at minimum, to be approached with skepticism, and might require clear pushback."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}