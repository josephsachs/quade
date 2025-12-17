using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace Quade.Behaviors;

public class PendingMessageBehavior : Behavior<TextBox>
{
    private DispatcherTimer? _timer;
    private int _ellipsisCount = 1;

    protected override void OnAttached()
    {
        base.OnAttached();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged += OnTextBoxPropertyChanged;
            CheckAndStartAnimation();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        
        StopAnimation();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged -= OnTextBoxPropertyChanged;
        }
    }

    private void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(TextBox.Text))
        {
            CheckAndStartAnimation();
        }
    }

    private void CheckAndStartAnimation()
    {
        if (AssociatedObject == null)
            return;

        var text = AssociatedObject.Text;
        
        if (text == "." || text == ".." || text == "...")
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
        if (AssociatedObject == null)
            return;

        _ellipsisCount = (_ellipsisCount % 3) + 1;
        AssociatedObject.Text = new string('.', _ellipsisCount);
    }
}