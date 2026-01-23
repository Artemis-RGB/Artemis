using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Preview;
using Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class ConfigureProfileViewModel : RoutableScreen<ProfileViewModelParameters>
{
    private readonly IProfileService _profileService;
    private readonly IProfileEditorService _profileEditorService;
    private readonly SourceList<ConfigurationSection> _configurationSections;

    [Notify] private ProfileConfiguration? _profileConfiguration;

    public ConfigureProfileViewModel(IProfileService profileService, IProfileEditorService profileEditorService, PreviewViewModel previewViewModel,
        Func<ConfigurationSection, ConfigurationSectionViewModel> getConfigurationSectionViewModel)
    {
        _profileService = profileService;
        _profileEditorService = profileEditorService;
        ParameterSource = ParameterSource.Route;

        PreviewViewModel = previewViewModel;

        _configurationSections = new SourceList<ConfigurationSection>();
        _configurationSections.Connect()
            .Filter(s => s.Slot == 0)
            .Transform(getConfigurationSectionViewModel)
            .Bind(out ReadOnlyObservableCollection<ConfigurationSectionViewModel> bottomLeftSections)
            .Subscribe();
        _configurationSections.Connect()
            .Filter(s => s.Slot == 1)
            .Transform(getConfigurationSectionViewModel)
            .Bind(out ReadOnlyObservableCollection<ConfigurationSectionViewModel> bottomRightSections)
            .Subscribe();
        _configurationSections.Connect()
            .Filter(s => s.Slot == 2)
            .Transform(getConfigurationSectionViewModel)
            .Bind(out ReadOnlyObservableCollection<ConfigurationSectionViewModel> sideSections)
            .Subscribe();

        BottomLeftSections = bottomLeftSections;
        BottomRightSections = bottomRightSections;
        SideSections = sideSections;
    }

    public PreviewViewModel PreviewViewModel { get; }
    public ReadOnlyObservableCollection<ConfigurationSectionViewModel> BottomLeftSections { get; private set; }
    public ReadOnlyObservableCollection<ConfigurationSectionViewModel> BottomRightSections { get; private set; }
    public ReadOnlyObservableCollection<ConfigurationSectionViewModel> SideSections { get; private set; }

    /// <inheritdoc />
    public override async Task OnNavigating(ProfileViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        ProfileConfiguration? profileConfiguration = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == parameters.ProfileId);

        // If the profile doesn't exist, cancel navigation
        if (profileConfiguration == null)
        {
            args.Cancel();
            return;
        }

        await _profileEditorService.ChangeCurrentProfileConfiguration(profileConfiguration);
        ProfileConfiguration = profileConfiguration;
        _configurationSections.Edit(editableSections =>
        {
            editableSections.Clear();
            editableSections.AddRange(profileConfiguration.ConfigurationSections);
        });
    }

    /// <inheritdoc />
    public override async Task OnClosing(NavigationArguments args)
    {
        if (!args.Path.StartsWith("profile"))
        {
            ProfileConfiguration = null;
            await _profileEditorService.ChangeCurrentProfileConfiguration(null);
        }
    }
}