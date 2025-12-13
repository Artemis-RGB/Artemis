using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public partial class EntryListInputViewModel : ViewModelBase
{
    private static string? _lastSearch;
    private readonly PluginSetting<int> _sortBy;
    private string? _search;
    [Notify] private string _searchWatermark = "Search";
    [Notify] private int _totalCount;

    public EntryListInputViewModel(ISettingsService settingsService)
    {
        _search = _lastSearch;
        _sortBy = settingsService.GetSetting("Workshop.SortBy", 0);
        _sortBy.AutoSave = true;
    }
    
    public string? Search
    {
        get => _search;
        set
        {
            RaiseAndSetIfChanged(ref _search, value);
            _lastSearch = value;
        }
    }
    
    public int SortBy
    {
        get => _sortBy.Value;
        set
        {
            _sortBy.Value = value;
            this.RaisePropertyChanged();
        }
    }

    public void ClearLastSearch()
    {
        _lastSearch = null;
    }
}