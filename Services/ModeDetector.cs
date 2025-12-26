using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ModeDetector
{
    private readonly ModelProviderResolver _providerResolver;
    private readonly ThoughtProcessLogger _logger;
    private readonly ConfigService _configService;

    private const string MODE_SELECT_IS_QUESTION = @"Is the user asking a question?";

    private const string MODE_SELECT_IS_STATEMENT_CLEAR = @"Is the user's meaning clear enough to respond to with confidence?";

    private const string MODE_SELECT_IS_INFORMATIONAL = @"Is the user's question informational?";

    private const string MODE_SELECT_IS_PERSONAL = @"Is the user's statement personal?";

    private const string MODE_SELECT_IS_CASUAL = @"Is the user's statement casual?";

    private const string MODE_SELECT_IS_JOKING = @"Is the user joking or being silly?";

    private const string MODE_SELECT_IS_PLAN = @"Does the user's statement a plan or course of action?";

    private const string MODE_SELECT_IS_REASONABLE = @"Is the user's statement reasonable and safe?";

    public ModeDetector(
        ModelProviderResolver providerResolver, 
        ThoughtProcessLogger logger,
        ConfigService configService)
    {
        _providerResolver = providerResolver;
        _logger = logger;
        _configService = configService;
    }

    private async Task<bool> ModeQuery(List<Message> message, string prompt) 
    {
        _logger.LogModePrompt(prompt);

        var config = await _configService.LoadConfigAsync();
        var provider = _providerResolver.GetProviderForModel(config.ThoughtModel);

        for (var attempts = 0; attempts < 4; attempts++) 
        {
            var requestConfig = new ModelRequestConfig
            {
                Model = config.ThoughtModel,
                MaxTokens = 1
            };

            var response = await provider.SendMessageAsync(
                requestConfig,
                message,
                $"{prompt} Answer the question with a single word, YES or NO."
            );

            _logger.LogModeResponse(response);

            var result = response?.ToUpperInvariant() switch 
            {
                "YES" => (bool?)true,
                "NO" => (bool?)false,
                _ => null
            };

            if (result.HasValue) 
                return result.Value;
        }

        _logger.LogModeResponse("Tried three times without valid response");

        return false;
    }

    public async Task<ConversationMode> DetectMode(List<Message> recentMessages)
    {
        if (recentMessages.Count == 0)
        {
            return ConversationMode.Empower;
        }

        var lastMessage = recentMessages.TakeLast(1).ToList();

        var isQuestion = await ModeQuery(lastMessage, MODE_SELECT_IS_QUESTION);

        if (isQuestion) 
        {
            return await HandleQuestion(lastMessage);
        } 
        else 
        {
            var isCasual = await ModeQuery(lastMessage, MODE_SELECT_IS_CASUAL);

            return isCasual ? await HandleCasual(lastMessage) : await HandleNonCasual(lastMessage);
        }
    }

    public async Task<ConversationMode> HandleQuestion(List<Message> lastMessage) 
    {
        var isInformational = await ModeQuery(lastMessage, MODE_SELECT_IS_INFORMATIONAL);

        if (isInformational) 
        {
            var isClear = await ModeQuery(lastMessage, MODE_SELECT_IS_STATEMENT_CLEAR); 

            return isClear ? ConversationMode.Opine : ConversationMode.Investigate;
        } 
        else 
        {
            return ConversationMode.Opine;
        }
    }

    public async Task<ConversationMode> HandleCasual(List<Message> lastMessage) 
    {
        var isJoking = await ModeQuery(lastMessage, MODE_SELECT_IS_JOKING);

        if (isJoking) 
        {
            return ConversationMode.Amuse;
        }

        var isPersonal = await ModeQuery(lastMessage, MODE_SELECT_IS_PERSONAL);
        var isReasonable = await ModeQuery(lastMessage, MODE_SELECT_IS_REASONABLE);

        if (isPersonal) 
        {
            return isReasonable ? ConversationMode.Empower : ConversationMode.Opine;
        } 
        else 
        {
            var isClear = await ModeQuery(lastMessage, MODE_SELECT_IS_STATEMENT_CLEAR); 

            if (!isClear) 
            {
                return ConversationMode.Investigate;
            }

            return isReasonable ? ConversationMode.Opine : ConversationMode.Critique;
        }
    }

    public async Task<ConversationMode> HandleNonCasual(List<Message> lastMessage) 
    {
        var isPlan = await ModeQuery(lastMessage, MODE_SELECT_IS_PLAN);
        var isClear = await ModeQuery(lastMessage, MODE_SELECT_IS_STATEMENT_CLEAR); 

        if (isPlan) 
        {
            if (isClear) 
            {
                return ConversationMode.Investigate;
            } 
            else 
            {
                var isReasonable = await ModeQuery(lastMessage, MODE_SELECT_IS_REASONABLE);

                return isReasonable ? ConversationMode.Empower : ConversationMode.Critique;
            }
        } 
        else 
        {
            return isClear ? ConversationMode.Opine : ConversationMode.Investigate;
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

            ConversationMode.Amuse => 
                "The user is being humorous. Respond unseriously, with lightness, irony, silliness or jokes.",
            
            _ => "Respond as you see fit."
        };
    }
}