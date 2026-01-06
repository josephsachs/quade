namespace Omoi.Models.ConversationModes;

public class Opine : IConversationMode
{
    public string GetSymbol() { return "思"; } 
    public string GetSystemPrompt() { return "The user is sharing an opinion in a conversational way. Feel free to share one in return, whether that be agreement, a contrasting viewpoint, a different subjective take, something tangential or something speculative and uncommitted. Little rigor is required."; }
    public int GetContextMessageDepth() { return 1; }
    public int GetMemoryTopK() { return 2; }
    
    //public int getMemoryIterations();
}