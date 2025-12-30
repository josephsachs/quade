using System.Collections.ObjectModel;
using Omoi.Models;
using Omoi.Services;

namespace Omoi.ViewModels;

public class ThoughtProcessViewModel : ViewModelBase
{
    private readonly ThoughtProcessLogger _logger;

    public ObservableCollection<LogEntry> Entries => _logger.Entries;

    public ThoughtProcessViewModel(ThoughtProcessLogger logger)
    {
        _logger = logger;
    }
}