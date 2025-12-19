using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private string _selectedModelId = string.Empty;
    private string _errorMessage = string.Empty;
    private Message? _editingMessage;

    public ObservableCollection<Message> Messages { get; } = new();
    public ObservableCollection<ModelInfo> AvailableModels { get; } = new();
    
    public ThoughtProcessLogger Logger => _logger;

    public string InputMessage
    {
        get => _inputMessage;
        set => this.RaiseAndSetIfChanged(ref _inputMessage, value);
    }

    public bool IsSending
    {
        get => _isSending || _editingMessage != null;
        set => this.RaiseAndSetIfChanged(ref _isSending, value);
    }

    public ConversationMode CurrentMode
    {
        get => _currentMode;
        set => this.RaiseAndSetIfChanged(ref _currentMode, value);
    }

    public string SelectedModelId
    {
        get => _selectedModelId;
        set => this.RaiseAndSetIfChanged(ref _selectedModelId, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
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

    public async Task InitializeAsync()
    {
        var models = await _configService.LoadModelsAsync();
        
        if (models.Count == 0)
        {
            await RefreshModelsAsync();
        }
        else
        {
            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }
        }

        var config = await _configService.LoadConfigAsync();
        SelectedModelId = config.ConversationalModel;
    }

    public async Task RefreshModelsAsync()
    {
        try
        {
            var models = await _apiClient.GetAvailableModelsAsync();
            
            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }
            
            await _configService.SaveModelsAsync(models);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to refresh models: {ex.Message}");
        }
    }

    public async Task SelectModelAsync(string modelId)
    {
        SelectedModelId = modelId;
        
        var config = await _configService.LoadConfigAsync();
        config.ConversationalModel = modelId;
        await _configService.SaveConfigAsync(config);
    }

    public void CanSendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputMessage))
            throw new Exception("Message cannot be empty.");
        
        if (IsSending)
            throw new Exception("Already sending a message.");
        
        if (!AvailableModels.Any(m => m.Id == SelectedModelId))
            throw new Exception("Selected model is not available. Please select a valid model from the Model menu.");
    }

    public void StartEditingMessage(Message message)
    {
        if (_editingMessage != null)
        {
            _editingMessage.IsEditing = false;
        }

        _editingMessage = message;
        message.IsEditing = true;
        
        this.RaisePropertyChanged(nameof(IsSending));
    }

    public async Task SubmitEditedMessageAsync(Message message)
    {
        if (_editingMessage != message)
            return;

        var index = Messages.IndexOf(message);
        if (index == -1)
            return;

        var editedContent = message.Content;
        
        var messagesToRemove = Messages.Skip(index).ToList();
        foreach (var msg in messagesToRemove)
        {
            Messages.Remove(msg);
        }

        var chatMessages = Messages.ToList();
        _chatService.LoadConversation(chatMessages);

        _editingMessage = null;
        this.RaisePropertyChanged(nameof(IsSending));

        IsSending = true;
        ErrorMessage = string.Empty;

        var userMessage = new Message
        {
            Content = editedContent,
            IsUser = true,
            Mode = CurrentMode,
            Timestamp = DateTime.Now
        };
        Messages.Add(userMessage);

        var placeholder = new Message
        {
            Content = "...",
            IsUser = false,
            Mode = CurrentMode,
            IsPending = true,
            Timestamp = DateTime.Now
        };
        Messages.Add(placeholder);

        try
        {
            var (response, newMode) = await _chatService.SendMessageAsync(editedContent);
            
            placeholder.Content = response.Content;
            placeholder.IsPending = false;
            placeholder.Mode = newMode;
            placeholder.Timestamp = response.Timestamp;
            
            CurrentMode = newMode;
        }
        catch (Exception ex)
        {
            Messages.Remove(placeholder);
            Messages.Remove(userMessage);
            ErrorMessage = ex.Message;
            InputMessage = editedContent;
        }
        finally
        {
            IsSending = false;
        }
    }

    public async Task<bool> TrySendMessageAsync(string messageText)
    {
        IsSending = true;
        ErrorMessage = string.Empty;

        var userMessage = new Message
        {
            Content = messageText,
            IsUser = true,
            Mode = CurrentMode,
            Timestamp = DateTime.Now
        };
        Messages.Add(userMessage);

        var placeholder = new Message
        {
            Content = "...",
            IsUser = false,
            Mode = CurrentMode,
            IsPending = true,
            Timestamp = DateTime.Now
        };
        Messages.Add(placeholder);

        try
        {
            var (response, newMode) = await _chatService.SendMessageAsync(messageText);
            
            placeholder.Content = response.Content;
            placeholder.IsPending = false;
            placeholder.Mode = newMode;
            placeholder.Timestamp = response.Timestamp;
            
            CurrentMode = newMode;
            
            return true;
        }
        catch (Exception ex)
        {
            Messages.Remove(placeholder);
            Messages.Remove(userMessage);
            ErrorMessage = ex.Message;
            return false;
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

    public async Task<AppConfig> GetConfigAsync()
    {
        return await _configService.LoadConfigAsync();
    }

    public async Task SaveWindowStateAsync(
        int mainX,
        int mainY,
        double mainWidth,
        double mainHeight,
        int thoughtX,
        int thoughtY,
        double thoughtWidth,
        double thoughtHeight,
        bool thoughtWasOpen)
    {
        var config = await _configService.LoadConfigAsync();
        config.MainWindowX = mainX;
        config.MainWindowY = mainY;
        config.MainWindowWidth = mainWidth;
        config.MainWindowHeight = mainHeight;
        config.ThoughtWindowX = thoughtX;
        config.ThoughtWindowY = thoughtY;
        config.ThoughtWindowWidth = thoughtWidth;
        config.ThoughtWindowHeight = thoughtHeight;
        config.ThoughtWindowWasOpen = thoughtWasOpen;
        await _configService.SaveConfigAsync(config);
    }
}