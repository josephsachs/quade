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
        Do the messages contain subjective questions or prompts for an opinion? Is it reasonable? Use OPINE.
        Do the messages contain propositions? Are they reasonable? Use EMPOWER.
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
            MODE_SELECTOR_MODEL,
            maxTokens: 3
        );

        _logger.LogModeResponse(response);

        return ParseMode(response);
    }

    private ConversationMode ParseMode(string response)
    {
        var modeString = response.Trim().ToLower();

        return modeString switch
        {
            var s when s.Contains("empower") => ConversationMode.Empower,
            var s when s.Contains("investigate") => ConversationMode.Investigate,
            var s when s.Contains("opine") => ConversationMode.Opine,
            var s when s.Contains("critique") => ConversationMode.Critique,
            _ => ConversationMode.Empower
        };
    }

    public static string GetSystemPromptForMode(ConversationMode mode)
    {
        return mode switch
        {
            ConversationMode.Empower => 
                "You are encouraging and supportive. Offer validation, positive energy and motivation. Look on the bright side and keep your eyes on the prize.",
            
            ConversationMode.Investigate => 
                "You are focused on clarification and definition. Ask questions, seek definitions, and help isolate variables and fill in unknown values.",
            
            ConversationMode.Opine => 
                "You are speculative and exploratory. Blue-sky freely, present contrasting viewpoints, and explore ideas without necessarily committing to them.",
            
            ConversationMode.Critique => 
                "You are skeptical and rigorous. Find faults, challenge assumptions, play devil's advocate, and apply tough-minded critical analysis.",
            
            _ => "You are a thoughtful conversational partner."
        };
    }
}