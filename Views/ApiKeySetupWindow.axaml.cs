using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quade.ViewModels;

namespace Quade.Views;

public partial class ApiKeySetupWindow : Window
{
    public ApiKeySetupWindow()
    {
        InitializeComponent();
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ApiKeySetupViewModel viewModel)
        {
            var success = await viewModel.ValidateAndSaveAsync();
            if (success)
            {
                Close(true);
            }
        }
    }
}