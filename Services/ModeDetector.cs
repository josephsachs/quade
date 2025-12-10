using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ModeDetector
{
    private readonly ApiClient _apiClient;
    private readonly ThoughtProcessLogger _logger;
    private const string MODE_SELECTOR_MODEL = "claude-3-5-haiku-20241022";
    
    private const string CLASSIFICATION_PROMPT = @"Choose a mode based on the recent messages.

        Do the messages contain objective questions or factual queries? Are there undefined or ambiguous terms? Do you need more information to answers? Use INVESTIGATE.
        Do the messages contain subjective questions or prompts for an opinion? Do they state a subjective opinion without asking a question? Is it reasonable? Use OPINE.
        Do the messages contain propositions seek an affirmative response? Are they reasonable? Is straightforward validation or emotional support appropriate? Use EMPOWER.
        Do the messages contain propositions of a dubious nature? Are they factually doubtful? Are they plans or schemas requiring analysis or careful consideration of fail states? Use CRITIQUE.

        Please respond with ONLY one word, the mode name (empower, investigate, opine, or critique). The real response will be requested later."
    ;

    public ModeDetector(ApiClient apiClient, ThoughtProcessLogger logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<ConversationMode> DetectMode(List<Message> recentMessages)
    {
        if (recentMessages.Count == 0)
        {
            return ConversationMode.Empower;
        }

        _logger.LogModeDetectionStart();
        _logger.LogModePrompt(CLASSIFICATION_PROMPT);

        var response = await _apiClient.SendMessageAsync(
            recentMessages.TakeLast(3).ToList(),
            CLASSIFICATION_PROMPT,
            MODE_SELECTOR_MODEL
        );

        _logger.LogModeResponse(response);

        return ParseMode(response);
    }

    private ConversationMode ParseMode(string response)
    {
        var modeString = response.Trim().ToLower();
        
        return modeString switch
        {
            "empower" => ConversationMode.Empower,
            "investigate" => ConversationMode.Investigate,
            "opine" => ConversationMode.Opine,
            "critique" => ConversationMode.Critique,
            _ => ConversationMode.Empower
        };
    }

    public static string GetSystemPromptForMode(ConversationMode mode)
    {
        return mode switch
        {
            ConversationMode.Empower => 
                "You are encouraging and supportive. Build confidence, validate ideas, and help the user feel empowered. Use 'yes, and...' energy.",
            
            ConversationMode.Investigate => 
                "You are focused on clarification and understanding. Ask Socratic questions, seek definitions, and help isolate variables to build epistemically solid foundations.",
            
            ConversationMode.Opine => 
                "You are speculative and exploratory. Blue-sky freely, present contrasting viewpoints, and explore ideas without necessarily committing to any single perspective.",
            
            ConversationMode.Critique => 
                "You are skeptical and rigorous. Find faults, challenge assumptions, play devil's advocate, and apply tough-minded critical analysis.",
            
            _ => "You are a thoughtful conversational partner."
        };
    }
}