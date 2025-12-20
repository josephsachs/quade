using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Quade.Services;
using Quade.ViewModels;
using Quade.Views;

namespace Quade;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var configService = new ConfigService();
            var credentialsService = new CredentialsService();
            var apiClient = new ApiClient();
            var logger = new ThoughtProcessLogger();
            var conversationService = new ConversationService();
            var contextBuilder = new ChatContextBuilder();
            var modeDetector = new ModeDetector(apiClient, logger);
            var chatService = new ChatService(apiClient, modeDetector, configService, logger, contextBuilder);

            var hasApiKey = await credentialsService.HasApiKeyAsync(CredentialsService.ANTHROPIC);
            
            if (!hasApiKey)
            {
                var welcomeWindow = new WelcomeWindow();
                welcomeWindow.Show();
            }

            var anthropicKey = await credentialsService.GetApiKeyAsync(CredentialsService.ANTHROPIC);
            if (!string.IsNullOrWhiteSpace(anthropicKey))
            {
                apiClient.SetApiKey(anthropicKey);
            }

            var viewModel = new MainWindowViewModel(
                chatService, 
                configService, 
                apiClient, 
                logger, 
                conversationService,
                credentialsService);

            await viewModel.InitializeAsync();

            var config = await configService.LoadConfigAsync();
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            bool hasCustomPosition = config.MainWindowX != 0 || config.MainWindowY != 0;
            bool hasCustomSize = config.MainWindowWidth > 0 && config.MainWindowHeight > 0;

            if (hasCustomPosition || hasCustomSize)
            {
                desktop.MainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                
                if (hasCustomSize)
                {
                    desktop.MainWindow.Width = config.MainWindowWidth;
                    desktop.MainWindow.Height = config.MainWindowHeight;
                }

                if (hasCustomPosition)
                {
                    desktop.MainWindow.Position = new PixelPoint((int)config.MainWindowX, (int)config.MainWindowY);
                }
            }

            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}