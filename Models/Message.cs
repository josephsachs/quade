using System;
using ReactiveUI;

namespace Quade.Models;

public class Message : ReactiveObject
{
    private string _content = string.Empty;
    private bool _isUser;
    private ConversationMode _mode;
    private DateTime _timestamp = DateTime.Now;
    private bool _isPending;

    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

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

    public ConversationMode Mode
    {
        get => _mode;
        set
        {
            this.RaiseAndSetIfChanged(ref _mode, value);
            this.RaisePropertyChanged(nameof(IconText));
        }
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => this.RaiseAndSetIfChanged(ref _timestamp, value);
    }

    public bool IsPending
    {
        get => _isPending;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPending, value);
            this.RaisePropertyChanged(nameof(IconText));
        }
    }

    public string IconText => IsPending ? "" : (IsUser ? "私" : Mode switch
    {
        ConversationMode.Empower => "力",
        ConversationMode.Investigate => "究",
        ConversationMode.Opine => "思",
        ConversationMode.Critique => "批",
        _ => "?"
    });

    public string IconColor => IsUser ? "#A8A8A8" : "#90EE90";
}