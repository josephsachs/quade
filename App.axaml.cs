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
            var anthropicClient = new AnthropicClient();
            var openAiClient = new OpenAiClient();
            var supabaseClient = new SupabaseClient();
            var logger = new ThoughtProcessLogger();
            var conversationService = new ConversationService();
            var contextBuilder = new ChatContextBuilder();
            
            var providerResolver = new ModelProviderResolver(anthropicClient, openAiClient);
            var vectorProviderResolver = new VectorProviderResolver(openAiClient);
            
            var modeDetector = new ModeDetector(providerResolver, logger, configService);
            var chatMemoryStorer = new ChatMemoryStorer(
                providerResolver, 
                vectorProviderResolver, 
                supabaseClient, 
                logger, 
                configService
            );
            var chatService = new ChatService(
                providerResolver, 
                modeDetector, 
                chatMemoryStorer, 
                configService, 
                logger, 
                contextBuilder
            );

            var hasApiKey = await credentialsService.HasApiKeyAsync(CredentialsService.ANTHROPIC);
            
            if (!hasApiKey)
            {
                var welcomeWindow = new WelcomeWindow();
                welcomeWindow.Show();
            }

            var anthropicKey = await credentialsService.GetApiKeyAsync(CredentialsService.ANTHROPIC);
            if (!string.IsNullOrWhiteSpace(anthropicKey))
            {
                anthropicClient.SetApiKey(anthropicKey);
            }

            var openAiKey = await credentialsService.GetApiKeyAsync(CredentialsService.OPENAI);
            if (!string.IsNullOrWhiteSpace(openAiKey))
            {
                openAiClient.SetApiKey(openAiKey);
            }

            var supabaseKey = await credentialsService.GetApiKeyAsync(CredentialsService.SUPABASE);
            if (!string.IsNullOrWhiteSpace(supabaseKey))
            {
                var appConfig = await configService.LoadConfigAsync();
                var supabaseUrl = appConfig.SupabaseUrl;
                
                if (!string.IsNullOrWhiteSpace(supabaseUrl))
                {
                    supabaseClient.SetApiKey(supabaseKey, supabaseUrl);
                    
                    try
                    {
                        await supabaseClient.EnsureTableExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogInfo($"Failed to initialize Supabase: {ex.Message}");
                    }
                }
            }

            var viewModel = new MainWindowViewModel(
                chatService, 
                configService, 
                anthropicClient,
                openAiClient,
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