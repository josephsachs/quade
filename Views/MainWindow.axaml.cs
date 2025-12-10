using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Quade.ViewModels;

namespace Quade.Views;

public partial class MainWindow : Window
{
    private ThoughtProcessWindow? _thoughtProcessWindow;
    private bool _isUserAtBottom = true;

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
            await viewModel.LoadAutoSaveAsync();
            ScrollToBottom();
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            var offset = scrollViewer.Offset.Y;
            var extent = scrollViewer.Extent.Height;
            var viewport = scrollViewer.Viewport.Height;
            var maxOffset = extent - viewport;
            
            _isUserAtBottom = (maxOffset - offset) <= 25;
        }
    }

    private async void OnKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (e.Source == this.FindControl<TextBox>("InputTextBox") &&
            e.Key == Key.Enter && 
            !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            
            if (DataContext is MainWindowViewModel viewModel && !viewModel.IsSending)
            {
                await viewModel.SendMessageAsync();
                ScrollToBottom();  // Always scroll when user sends
            }
        }
    }

    private async void SendMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SendMessageAsync();
            ScrollToBottom();  // Always scroll when user sends
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

    private void ScrollToBottomIfNeeded()
    {
        if (_isUserAtBottom)
        {
            ScrollToBottom();
        }
    }
}