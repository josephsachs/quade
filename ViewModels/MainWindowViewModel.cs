using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using Quade.Models;
using Quade.Services;

namespace Quade.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private readonly ConfigService _configService;
    private readonly ApiClient _apiClient;
    private readonly ThoughtProcessLogger _logger;
    private readonly ConversationService _conversationService;

    private string _inputMessage = string.Empty;
    private bool _isSending;
    private ConversationMode _currentMode = ConversationMode.Empower;

    public ObservableCollection<Message> Messages { get; } = new();
    
    public ThoughtProcessLogger Logger => _logger;

    public string InputMessage
    {
        get => _inputMessage;
        set => this.RaiseAndSetIfChanged(ref _inputMessage, value);
    }

    public bool IsSending
    {
        get => _isSending;
        set => this.RaiseAndSetIfChanged(ref _isSending, value);
    }

    public ConversationMode CurrentMode
    {
        get => _currentMode;
        set => this.RaiseAndSetIfChanged(ref _currentMode, value);
    }

    public MainWindowViewModel(
        ChatService chatService,
        ConfigService configService,
        ApiClient apiClient,
        ThoughtProcessLogger logger,
        ConversationService conversationService)
    {
        _chatService = chatService;
        _configService = configService;
        _apiClient = apiClient;
        _logger = logger;
        _conversationService = conversationService;
    }

    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputMessage) || IsSending)
            return;

        var message = InputMessage;
        InputMessage = string.Empty;
        IsSending = true;

        try
        {
            Messages.Add(new Message
            {
                Content = message,
                IsUser = true,
                Mode = CurrentMode,
                Timestamp = DateTime.Now
            });

            var (response, newMode) = await _chatService.SendMessageAsync(message);
            
            CurrentMode = newMode;
            Messages.Add(response);
        }
        catch (Exception ex)
        {
            Messages.Add(new Message
            {
                Content = $"Error: {ex.Message}",
                IsUser = false,
                Mode = CurrentMode,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsSending = false;
        }
    }

    public async Task NewConversationAsync()
    {
        await AutoSaveAsync();
        Messages.Clear();
        _chatService.ClearConversation();
        CurrentMode = ConversationMode.Empower;
    }

    public async Task SaveConversationAsync(string filepath)
    {
        await _conversationService.SaveConversationAsync(
            new System.Collections.Generic.List<Message>(Messages),
            CurrentMode,
            filepath);
    }

    public async Task LoadConversationAsync(string filepath)
    {
        await AutoSaveAsync();
        
        var data = await _conversationService.LoadConversationAsync(filepath);
        if (data != null)
        {
            Messages.Clear();
            foreach (var msg in data.Messages)
            {
                Messages.Add(msg);
            }
            CurrentMode = data.CurrentMode;
            _chatService.LoadConversation(data.Messages);
        }
    }

    public async Task AutoSaveAsync()
    {
        if (Messages.Count > 0)
        {
            await _conversationService.AutoSaveAsync(
                new System.Collections.Generic.List<Message>(Messages),
                CurrentMode);
        }
    }

    public async Task LoadAutoSaveAsync()
    {
        var data = await _conversationService.LoadAutoSaveAsync();
        if (data != null && data.Messages.Count > 0)
        {
            Messages.Clear();
            foreach (var msg in data.Messages)
            {
                Messages.Add(msg);
            }
            CurrentMode = data.CurrentMode;
            _chatService.LoadConversation(data.Messages);
        }
    }

    public string GenerateTimestampedFilename()
    {
        return _conversationService.GenerateTimestampedFilename();
    }

    public string GetConversationsDirectory()
    {
        return _conversationService.GetConversationsDirectory();
    }
}