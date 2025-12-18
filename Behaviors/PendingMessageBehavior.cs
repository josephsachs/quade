using System;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using LiveMarkdown.Avalonia;
using Quade.Models;

namespace Quade.Behaviors;

public class PendingMessageBehavior : Behavior<MarkdownRenderer>
{
    private DispatcherTimer? _timer;
    private int _ellipsisCount = 1;
    private Message? _message;

    protected override void OnAttached()
    {
        base.OnAttached();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged += OnDataContextChanged;
            CheckAndStartAnimation();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        
        StopAnimation();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
        }
        
        if (_message != null)
        {
            _message.PropertyChanged -= OnMessagePropertyChanged;
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
        
        CheckAndStartAnimation();
    }

    private void OnMessagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Message.IsPending))
        {
            CheckAndStartAnimation();
        }
    }

    private void CheckAndStartAnimation()
    {
        if (AssociatedObject == null || _message == null)
            return;

        if (_message.IsPending)
        {
            if (_timer == null)
            {
                StartAnimation();
            }
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (_timer != null)
            return;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopAnimation()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (AssociatedObject == null || _message == null)
            return;

        _ellipsisCount = (_ellipsisCount % 3) + 1;
        
        var builder = _message.GetOrCreateBuilder();
        builder.Clear();
        builder.Append(new string('.', _ellipsisCount));
    }
}