using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Quade.Models;
using Quade.ViewModels;

namespace Quade.Views;

public partial class MainWindow : Window
{
    private ThoughtProcessWindow? _thoughtProcessWindow;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
        
        AddHandler(KeyDownEvent, OnKeyDownTunnel, RoutingStrategies.Tunnel);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.AvailableModels.CollectionChanged += OnModelsChanged;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.Messages.CollectionChanged += OnMessagesChanged;
            
            await viewModel.LoadAutoSaveAsync();
            ScrollToBottom();
            
            BuildModelMenu();
        }
    }

    private void OnModelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildModelMenu();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedModelId))
        {
            BuildModelMenu();
        }
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (Message msg in e.NewItems)
            {
                if (!msg.IsUser)
                {
                    ScrollToBottom();
                }
            }
        }
    }

    private void BuildModelMenu()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var modelMenu = this.FindControl<MenuItem>("ModelMenu");
        if (modelMenu == null)
            return;

        modelMenu.Items.Clear();

        foreach (var model in viewModel.AvailableModels)
        {
            var menuItem = new MenuItem
            {
                Header = model.DisplayName,
                Tag = model.Id
            };

            if (model.Id == viewModel.SelectedModelId)
            {
                menuItem.Icon = new TextBlock { Text = "✓" };
            }

            menuItem.Click += async (s, e) => await OnModelSelected(s, e);
            modelMenu.Items.Add(menuItem);
        }

        if (viewModel.AvailableModels.Count > 0)
        {
            modelMenu.Items.Add(new Separator());
        }

        var refreshItem = new MenuItem { Header = "_Refresh" };
        refreshItem.Click += async (s, e) => await OnRefreshModels(s, e);
        modelMenu.Items.Add(refreshItem);
    }

    private async Task OnModelSelected(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Tag is string modelId &&
            DataContext is MainWindowViewModel viewModel)
        {
            try
            {
                await viewModel.SelectModelAsync(modelId);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Model Selection Error", ex.Message);
            }
        }
    }

    private async Task OnRefreshModels(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            try
            {
                await viewModel.RefreshModelsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Model Refresh Error", ex.Message);
            }
        }
    }

    private async Task ShowErrorDialog(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var stack = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        stack.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };

        okButton.Click += (s, e) => dialog.Close();
        stack.Children.Add(okButton);

        dialog.Content = stack;
        await dialog.ShowDialog(this);
    }

    private async void OnKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (e.Source == this.FindControl<TextBox>("InputTextBox") &&
            e.Key == Key.Enter && 
            !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            
            if (DataContext is MainWindowViewModel viewModel)
            {
                try
                {
                    viewModel.CanSendMessage();
                    
                    var messageText = viewModel.InputMessage;
                    viewModel.InputMessage = string.Empty;
                    
                    var success = await viewModel.TrySendMessageAsync(messageText);
                    
                    if (!success)
                    {
                        viewModel.InputMessage = messageText;
                    }
                    
                    ScrollToBottom();
                }
                catch (Exception ex)
                {
                    viewModel.ErrorMessage = ex.Message;
                }
            }
        }
    }

    private async void SendMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            try
            {
                viewModel.CanSendMessage();
                
                var messageText = viewModel.InputMessage;
                viewModel.InputMessage = string.Empty;
                
                var success = await viewModel.TrySendMessageAsync(messageText);
                
                if (!success)
                {
                    viewModel.InputMessage = messageText;
                }
                
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = ex.Message;
            }
        }
    }

    private async void NewConversation_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.NewConversationAsync();
        }
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var filepath = viewModel.GenerateTimestampedFilename();
            await viewModel.SaveConversationAsync(filepath);
        }
    }

    private async void SaveAs_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var conversationsDir = viewModel.GetConversationsDirectory();
            var folder = await StorageProvider.TryGetFolderFromPathAsync(conversationsDir);
            
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Conversation",
                DefaultExtension = "json",
                SuggestedFileName = $"conversation_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json",
                SuggestedStartLocation = folder,
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new("JSON Files") { Patterns = new[] { "*.json" } },
                    new("All Files") { Patterns = new[] { "*" } }
                }
            });

            if (file != null)
            {
                await viewModel.SaveConversationAsync(file.Path.LocalPath);
            }
        }
    }

    private async void Load_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var conversationsDir = viewModel.GetConversationsDirectory();
            var folder = await StorageProvider.TryGetFolderFromPathAsync(conversationsDir);
            
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load Conversation",
                AllowMultiple = false,
                SuggestedStartLocation = folder,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new("JSON Files") { Patterns = new[] { "*.json" } },
                    new("All Files") { Patterns = new[] { "*" } }
                }
            });

            if (files.Count > 0)
            {
                await viewModel.LoadConversationAsync(files[0].Path.LocalPath);
                ScrollToBottom();
            }
        }
    }

    private async void Quit_Click(object? sender, RoutedEventArgs e)
    {
        await HandleQuitAsync();
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!e.Cancel)
        {
            e.Cancel = true;
            await HandleQuitAsync();
            Closing -= OnWindowClosing;
            
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }
    }

    private async Task HandleQuitAsync()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.AutoSaveAsync();
        }
    }

    private void ShowThoughtProcess_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (_thoughtProcessWindow == null)
            {
                _thoughtProcessWindow = new ThoughtProcessWindow
                {
                    DataContext = new ThoughtProcessViewModel(viewModel.Logger)
                };
            }

            if (_thoughtProcessWindow.IsVisible)
            {
                _thoughtProcessWindow.Hide();
            }
            else
            {
                _thoughtProcessWindow.Show();
            }
        }
    }

    private void ScrollToBottom()
    {
        if (this.FindControl<ScrollViewer>("MessageScrollViewer") is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}