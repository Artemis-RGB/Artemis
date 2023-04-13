using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Plugins.Help;

public class PluginHelpWindowViewModel : ActivatableViewModelBase
{
    private IPluginHelpPage? _selectedHelpPage;

    public PluginHelpWindowViewModel(Plugin plugin, string? preselectId)
    {
        Plugin = plugin;
        DisplayName = $"{Plugin.Info.Name} | Help";

        // Populate help pages by wrapping MarkdownHelpPages into a MarkdownHelpPageViewModel
        // other types are used directly, up to them to implement a VM directly as well
        HelpPages = new ReadOnlyCollection<IPluginHelpPage>(plugin.HelpPages.Select(p => p is MarkdownPluginHelpPage m ? new MarkdownPluginHelpPageViewModel(m) : p).ToList());

        _selectedHelpPage = preselectId != null ? HelpPages.FirstOrDefault(p => p.Id == preselectId) : HelpPages.FirstOrDefault();
    }

    public Plugin Plugin { get; }
    public ReadOnlyCollection<IPluginHelpPage> HelpPages { get; }

    public IPluginHelpPage? SelectedHelpPage
    {
        get => _selectedHelpPage;
        set => RaiseAndSetIfChanged(ref _selectedHelpPage, value);
    }
}