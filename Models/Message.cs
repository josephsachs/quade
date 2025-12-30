using System;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace Omoi.Models;

public class Message : ReactiveObject
{
    private string _content = string.Empty;
    private bool _isUser;
    private ConversationMode _mode;
    private DateTime _timestamp = DateTime.Now;
    private bool _isPending;
    private bool _isEditing;
    private bool _isFocused;
    private bool _isMemorized;

    [JsonPropertyName("Content")]
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    [JsonPropertyName("IsUser")]
    public bool IsUser
    {
        get => _isUser;
        set
        {
            this.RaiseAndSetIfChanged(ref _isUser, value);
            this.RaisePropertyChanged(nameof(IconText));
            this.RaisePropertyChanged(nameof(IconColor));
        }
    }

    [JsonPropertyName("Mode")]
    public ConversationMode Mode
    {
        get => _mode;
        set
        {
            this.RaiseAndSetIfChanged(ref _mode, value);
            this.RaisePropertyChanged(nameof(IconText));
        }
    }

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp
    {
        get => _timestamp;
        set => this.RaiseAndSetIfChanged(ref _timestamp, value);
    }

    [JsonPropertyName("IsMemorized")]
    public bool IsMemorized
    {
        get => _isMemorized;
        set
        {
            this.RaiseAndSetIfChanged(ref _isMemorized, value);
            this.RaisePropertyChanged(nameof(ShowEditButton));
        }
    }

    [JsonIgnore]
    public bool IsPending
    {
        get => _isPending;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPending, value);
            this.RaisePropertyChanged(nameof(IconText));
        }
    }

    [JsonIgnore]
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            this.RaiseAndSetIfChanged(ref _isEditing, value);
            this.RaisePropertyChanged(nameof(ShowEditButton));
            this.RaisePropertyChanged(nameof(EditButtonIcon));
        }
    }

    [JsonIgnore]
    public bool IsFocused
    {
        get => _isFocused;
        set
        {
            this.RaiseAndSetIfChanged(ref _isFocused, value);
            this.RaisePropertyChanged(nameof(ShowEditButton));
        }
    }

    [JsonIgnore]
    public bool ShowEditButton => IsUser && (IsFocused || IsEditing);

    [JsonIgnore]
    public string EditButtonIcon => IsEditing ? "✓" : "✏️";

    [JsonIgnore]
    public string IconText => IsPending ? "" : (IsUser ? "私" : Mode switch
    {
        ConversationMode.Empower => "力",
        ConversationMode.Investigate => "究",
        ConversationMode.Opine => "思",
        ConversationMode.Critique => "批",
        ConversationMode.Amuse => "楽",
        _ => "?"
    });

    [JsonIgnore]
    public string IconColor => IsUser ? "#A8A8A8" : "#90EE90";
}