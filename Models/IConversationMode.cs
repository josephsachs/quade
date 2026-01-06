namespace Omoi.Models;

public interface IConversationMode
{
    public string GetSymbol();
    public string GetSystemPrompt();

    public int GetContextMessageDepth();
    public int GetMemoryTopK();
    
    //public int getMemoryIterations();
}