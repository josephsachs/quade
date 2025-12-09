using Avalonia.Controls;
using Avalonia.Interactivity;
using Quade.ViewModels;

namespace Quade.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SendMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SendMessageAsync();
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
}