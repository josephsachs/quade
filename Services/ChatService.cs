using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ChatService
{
    private readonly ApiClient _apiClient;
    private readonly ModeDetector _modeDetector;
    private readonly ConfigService _configService;
    private readonly ThoughtProcessLogger _logger;
    private readonly ChatContextBuilder _contextBuilder;
    
    private List<Message> _messages = new();
    private ConversationMode _currentMode = ConversationMode.Empower;

    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public ConversationMode CurrentMode => _currentMode;

    public ChatService(
        ApiClient apiClient, 
        ModeDetector modeDetector,
        ConfigService configService,
        ThoughtProcessLogger logger,
        ChatContextBuilder contextBuilder)
    {
        _apiClient = apiClient;
        _modeDetector = modeDetector;
        _configService = configService;
        _logger = logger;
        _contextBuilder = contextBuilder;
    }

    public async Task<(Message response, ConversationMode newMode)> SendMessageAsync(string userMessage)
    {
        var config = await _configService.LoadConfigAsync();
        
        var userMsg = new Message
        {
            Content = userMessage,
            IsUser = true,
            Mode = _currentMode,
            Timestamp = DateTime.Now
        };
        
        _messages.Add(userMsg);

        var newMode = await _modeDetector.DetectMode(_messages);
        _currentMode = newMode;

        var systemPrompt = ModeDetector.GetSystemPromptForMode(newMode);
        _logger.LogSystemPrompt(newMode, systemPrompt);

        var contextMessages = _contextBuilder.BuildContext(_messages);

        var responseText = await _apiClient.SendMessageAsync(
            contextMessages,
            systemPrompt,
            config.ConversationalModel
        );

        var responseMsg = new Message
        {
            Content = responseText,
            IsUser = false,
            Mode = newMode,
            Timestamp = DateTime.Now
        };
        
        _messages.Add(responseMsg);

        return (responseMsg, newMode);
    }

    public void ClearConversation()
    {
        _messages.Clear();
        _currentMode = ConversationMode.Empower;
        _logger.Clear();
    }

    public void LoadConversation(List<Message> messages)
    {
        _messages = new List<Message>(messages);
        _currentMode = messages.Count > 0 
            ? messages[^1].Mode 
            : ConversationMode.Empower;
    }
}