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

    private const string MODE_SELECT_IS_QUESTION = @"Is the message a question?";

    private const string MODE_SELECT_IS_MORE_INFORMATION_NEEDED = @"Do you need more information to be confident expressing a view?";

    private const string MODE_SELECT_IS_INFORMATIONAL = @"Is the question informational?";

    private const string MODE_SELECT_IS_PERSONAL = @"Is the statement personal?";

    private const string MODE_SELECT_IS_CASUAL = @"Is the statement casual?";

    private const string MODE_SELECT_IS_PLAN = @"Is the statement a plan?";

    private const string MODE_SELECT_IS_REASONABLE = @"Is the statement reasonable and valid?";

    public ModeDetector(ApiClient apiClient, ThoughtProcessLogger logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    private async Task<string> ModeQuery(List<Quade.Models.Message> message, string prompt) {
        _logger.LogModeDetectionStart();
        _logger.LogModePrompt(prompt);

        var response = await _apiClient.SendMessageAsync(
            message, 
            $"{prompt} Respond with ONLY a YES or NO: ",
            MODE_SELECTOR_MODEL,
            maxTokens: 1
        );

        _logger.LogModeResponse(response);

        return response;
    }

    public async Task<ConversationMode> DetectMode(List<Message> recentMessages)
    {
        if (recentMessages.Count == 0)
        {
            return ConversationMode.Empower;
        }

        var lastMessage = recentMessages.TakeLast(1).ToList();

        var isQuestion = await ModeQuery(lastMessage, MODE_SELECT_IS_QUESTION);

        if (isQuestion.Contains("Y")) {
            var isInformational = await ModeQuery(lastMessage, MODE_SELECT_IS_INFORMATIONAL);

            if (isInformational.Contains("Y")) {
                return ConversationMode.Investigate;
            } else {
                return ConversationMode.Opine;
            }

        } else {
            var isCasual = await ModeQuery(lastMessage, MODE_SELECT_IS_CASUAL);

            if (isCasual.Contains("Y")) {
                var isPersonal = await ModeQuery(lastMessage, MODE_SELECT_IS_PERSONAL);

                if (isPersonal.Contains("Y")) {
                    var isReasonable = await ModeQuery(lastMessage, MODE_SELECT_IS_REASONABLE);

                    if (isReasonable.Contains("Y")) {
                        return ConversationMode.Empower;
                    } else {
                        return ConversationMode.Opine;
                    }
                } else {
                    // Not personal
                    var needMoreInfo = await ModeQuery(lastMessage, MODE_SELECT_IS_MORE_INFORMATION_NEEDED); 

                    if (needMoreInfo.Contains("Y")) {
                        return ConversationMode.Investigate;
                    } else {
                        var isReasonable = await ModeQuery(lastMessage, MODE_SELECT_IS_REASONABLE);

                        if (isReasonable.Contains("Y")) {
                            return ConversationMode.Opine;
                        } else {
                            return ConversationMode.Critique;
                        }
                    }
                }
            } else {
                // Not a casual statement
                var isPlan = await ModeQuery(lastMessage, MODE_SELECT_IS_PLAN);

                if (isPlan.Contains("Y")) {
                    var needMoreInfo = await ModeQuery(lastMessage, MODE_SELECT_IS_MORE_INFORMATION_NEEDED); 

                    if (needMoreInfo.Contains("Y")) {
                        return ConversationMode.Investigate;
                    } else {
                        var isReasonable = await ModeQuery(lastMessage, MODE_SELECT_IS_REASONABLE);

                        if (isReasonable.Contains("Y")) {
                            return ConversationMode.Empower;
                        } else {
                            return ConversationMode.Critique;
                        }
                    }
                } else {
                    // Not a plan
                    var needMoreInfo = await ModeQuery(lastMessage, MODE_SELECT_IS_MORE_INFORMATION_NEEDED); 

                    if (needMoreInfo.Contains("Y")) {
                        return ConversationMode.Investigate;
                    } else {
                        return ConversationMode.Opine;
                    }
                }
            }
        }
    }

    public static string GetSystemPromptForMode(ConversationMode mode)
    {
        return mode switch
        {
            ConversationMode.Empower => 
                "The user is sharing thoughts and ideas. Encouragement and support is appropriate. Be helpful and positive, encourage or assist, help elaborate or offer thoughtful engagement with their point of view, as appropriate.",
            
            ConversationMode.Investigate => 
                "The user is questioning, or exploring a space with unknowns. Ask questions, seek definitions, and help isolate variables and fill in unknown values so that you can respond with confidence.",
            
            ConversationMode.Opine => 
                "The user is sharing an opinion in a conversational way. Feel free to share one in return, whether that be agreement, a contrasting viewpoint, a different subjective take, something tangential or something speculative and uncommitted. Little rigor is required.",
            
            ConversationMode.Critique => 
                "The user is expressing something dubious. Challenge this, play devil's advocate, and/or apply tough-minded critical analysis. The idea needs, at minimum, to be approached with skepticism, and might require clear pushback.",
            
            _ => "Respond as you see fit."
        };
    }
}