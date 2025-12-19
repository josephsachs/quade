using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Quade.Models;

namespace Quade.Behaviors;

public class FocusTrackingBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        
        if (AssociatedObject != null)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }
    }

    private void OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.DataContext is Message message)
        {
            message.IsFocused = true;
        }
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.DataContext is Message message)
        {
            message.IsFocused = false;
        }
    }
}