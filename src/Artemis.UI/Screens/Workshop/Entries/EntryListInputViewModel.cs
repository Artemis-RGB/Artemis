using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public class EntryListInputViewModel : ViewModelBase
{
    private static string? _lastSearch;
    private readonly PluginSetting<int> _entriesPerPage;
    private readonly PluginSetting<int> _sortBy;
    private string? _search;
    private string _searchWatermark = "Search";
    private int _totalCount;

    public EntryListInputViewModel(ISettingsService settingsService)
    {
        _search = _lastSearch;
        _entriesPerPage = settingsService.GetSetting("Workshop.EntriesPerPage", 10);
        _sortBy = settingsService.GetSetting("Workshop.SortBy", 10);
        _entriesPerPage.AutoSave = true;
        _sortBy.AutoSave = true;
    }

    public string SearchWatermark
    {
        get => _searchWatermark;
        set => RaiseAndSetIfChanged(ref _searchWatermark, value);
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

    public int EntriesPerPage
    {
        get => _entriesPerPage.Value;
        set
        {
            _entriesPerPage.Value = value;
            this.RaisePropertyChanged();
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

    public int TotalCount
    {
        get => _totalCount;
        set => RaiseAndSetIfChanged(ref _totalCount, value);
    }

    public void ClearLastSearch()
    {
        _lastSearch = null;
    }

}