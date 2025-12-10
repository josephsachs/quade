using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ModeDetector
{
    private readonly ApiClient _apiClient;
    private readonly ThoughtProcessLogger _logger;
    private const string MODE_SELECTOR_MODEL = "claude-haiku-4-5-20251001";
    
    private const string CLASSIFICATION_PROMPT = @"Given the recent conversation, which mode is most appropriate?

empower: User needs encouragement, validation, or support
investigate: User needs clarification, definition, or deeper understanding
opine: User wants exploration, speculation, or alternative perspectives
critique: User needs critical feedback, challenge, or rigorous analysis

Respond with ONLY the mode name (empower, investigate, opine, or critique).";

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