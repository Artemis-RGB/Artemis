using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.UI.Shared;
using Material.Icons;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarScreenViewModel : ViewModelBase
{
    private bool _isExpanded;

    public SidebarScreenViewModel(MaterialIconKind icon, string displayName, string path, string? rootPath = null, ObservableCollection<SidebarScreenViewModel>? screens = null)
    {
        Icon = icon;
        Path = path;
        RootPath = rootPath ?? path;
        DisplayName = displayName;
        Screens = screens ?? new ObservableCollection<SidebarScreenViewModel>();
    }

    public MaterialIconKind Icon { get; }
    public string Path { get; }
    public string RootPath { get; }

    public ObservableCollection<SidebarScreenViewModel> Screens { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool Matches(string? path)
    {
        if (path == null)
            return false;
        return path.StartsWith(RootPath, StringComparison.InvariantCultureIgnoreCase);
    }

    public SidebarScreenViewModel? GetMatch(string path)
    {
        foreach (SidebarScreenViewModel sidebarScreenViewModel in Screens)
        {
            SidebarScreenViewModel? match = sidebarScreenViewModel.GetMatch(path);
            if (match != null)
                return match;
        }

        return Screens.FirstOrDefault(s => s.Matches(path));
    }

    public void ExpandIfRequired(SidebarScreenViewModel selected)
    {
        if (selected == this)
            return;

        if (Screens.Contains(selected))
            IsExpanded = true;

        foreach (SidebarScreenViewModel sidebarScreenViewModel in Screens)
            sidebarScreenViewModel.ExpandIfRequired(selected);
    }
}