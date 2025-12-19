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
            var apiClient = new ApiClient();
            var logger = new ThoughtProcessLogger();
            var conversationService = new ConversationService();
            var contextBuilder = new ChatContextBuilder();
            var modeDetector = new ModeDetector(apiClient, logger);
            var chatService = new ChatService(apiClient, modeDetector, configService, logger, contextBuilder);

            var hasApiKey = await configService.HasApiKeyAsync();
            
            if (!hasApiKey)
            {
                var setupWindow = new ApiKeySetupWindow
                {
                    DataContext = new ApiKeySetupViewModel(configService, apiClient)
                };
                
                var tcs = new TaskCompletionSource<bool>();
                setupWindow.Closed += (s, e) => tcs.SetResult(true);
                
                setupWindow.Show();
                await tcs.Task;
                
                var hasKeyNow = await configService.HasApiKeyAsync();
                if (!hasKeyNow)
                {
                    desktop.Shutdown();
                    return;
                }
            }

            var config = await configService.LoadConfigAsync();
            apiClient.SetApiKey(config.ApiKey);

            var viewModel = new MainWindowViewModel(
                chatService, 
                configService, 
                apiClient, 
                logger, 
                conversationService);

            await viewModel.InitializeAsync();

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