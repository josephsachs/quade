using System.Collections.ObjectModel;
using Quade.Models;
using Quade.Services;

namespace Quade.ViewModels;

public class ThoughtProcessViewModel : ViewModelBase
{
    private readonly ThoughtProcessLogger _logger;

    public ObservableCollection<LogEntry> Entries => _logger.Entries;

    public ThoughtProcessViewModel(ThoughtProcessLogger logger)
    {
        _logger = logger;
    }
}