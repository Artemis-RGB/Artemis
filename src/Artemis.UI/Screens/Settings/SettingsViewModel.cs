using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using DryIoc;

namespace Artemis.UI.Screens.Settings;

public class SettingsViewModel : Routable, IMainScreenViewModel
{
    private readonly IRouter _router;
    private SettingsTab? _selectedTab;

    public SettingsViewModel(IRouter router)
    {
        _router = router;
        SettingTabs = new ObservableCollection<SettingsTab> {new("home", "Home"), new("plugins", "Plugins"), new("devices", "Devices"), new("about", "About"),};
    }

    public ObservableCollection<SettingsTab> SettingTabs { get; }

    public SettingsTab? SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public ViewModelBase? TitleBarViewModel => null;

    #region Overrides of Routable

    /// <inheritdoc />
    public override async Task<bool> Activate(RouteResolution routeResolution, IContainer container)
    {
        if (routeResolution.Child == null)
            return await _router.Navigate("settings/home");

        bool result = await base.Activate(routeResolution, container);
        SelectedTab = SettingTabs.FirstOrDefault(t => t.Matches(routeResolution.Path));
        return result;
    }

    #endregion
}