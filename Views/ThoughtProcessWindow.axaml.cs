using System.Collections.Specialized;
using Avalonia.Controls;
using Omoi.ViewModels;

namespace Omoi.Views;

public partial class ThoughtProcessWindow : Window
{
    private bool _isUserAtBottom = true;

    public ThoughtProcessWindow()
    {
        InitializeComponent();
        
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        
        if (this.FindControl<ScrollViewer>("LogScrollViewer") is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is ThoughtProcessViewModel viewModel)
        {
            viewModel.Entries.CollectionChanged += OnEntriesChanged;
        }
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ScrollToBottomIfNeeded();
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
            
            _isUserAtBottom = (maxOffset - offset) <= 50;
        }
    }

    private void ScrollToBottom()
    {
        if (this.FindControl<ScrollViewer>("LogScrollViewer") is ScrollViewer scrollViewer)
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }
}