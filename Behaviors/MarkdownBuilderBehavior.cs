using Avalonia;
using Avalonia.Xaml.Interactivity;
using LiveMarkdown.Avalonia;
using Quade.Models;

namespace Quade.Behaviors;

public class MarkdownBuilderBehavior : Behavior<MarkdownRenderer>
{
    private Message? _message;

    protected override void OnAttached()
    {
        base.OnAttached();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged += OnDataContextChanged;
            UpdateBuilder();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        
        if (_message != null)
        {
            _message.PropertyChanged -= OnMessagePropertyChanged;
        }
        
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
        }
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_message != null)
        {
            _message.PropertyChanged -= OnMessagePropertyChanged;
        }
        
        _message = AssociatedObject?.DataContext as Message;
        
        if (_message != null)
        {
            _message.PropertyChanged += OnMessagePropertyChanged;
        }
        
        UpdateBuilder();
    }

    private void OnMessagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Message.Content) || 
            e.PropertyName == nameof(Message.IsPending) ||
            e.PropertyName == nameof(Message.IsStreaming))
        {
            UpdateBuilder();
        }
    }

    private void UpdateBuilder()
    {
        if (AssociatedObject?.DataContext is Message message)
        {
            AssociatedObject.MarkdownBuilder = message.GetOrCreateBuilder();
        }
    }
}