using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Quade.ViewModels;

namespace Quade.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        if (this.FindControl<TextBox>("InputTextBox") is TextBox textBox)
        {
            textBox.KeyDown += InputTextBox_KeyDown;
        }
    }

    private async void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            
            if (DataContext is MainWindowViewModel viewModel && !viewModel.IsSending)
            {
                await viewModel.SendMessageAsync();
                ScrollToBottom();
            }
        }
    }

    private async void SendMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SendMessageAsync();
            ScrollToBottom();
        }
    }

    private void NewConversation_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.NewConversation();
        }
    }

    private void Quit_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ScrollToBottom()
    {
        if (this.FindControl<ScrollViewer>("MessageScrollViewer") is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}