using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quade.Services;
using Quade.ViewModels;

namespace Quade.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.LoadKeysAsync();
        }
    }

    private async void AddAnthropicKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.AddOrReplaceKeyAsync(CredentialsService.ANTHROPIC);
        }
    }

    private async void DeleteAnthropicKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.DeleteKeyAsync(CredentialsService.ANTHROPIC);
        }
    }

    private async void AddOpenaiKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.AddOrReplaceKeyAsync(CredentialsService.OPENAI);
        }
    }

    private async void DeleteOpenaiKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.DeleteKeyAsync(CredentialsService.OPENAI);
        }
    }

    private async void AddAnlatanKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.AddOrReplaceKeyAsync(CredentialsService.ANLATAN);
        }
    }

    private async void DeleteAnlatanKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.DeleteKeyAsync(CredentialsService.ANLATAN);
        }
    }

        private async void AddSupabaseKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.AddOrReplaceKeyAsync(CredentialsService.SUPABASE);
        }
    }

        private async void DeleteSupabaseKey_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            await viewModel.DeleteKeyAsync(CredentialsService.SUPABASE);
        }
    }
}