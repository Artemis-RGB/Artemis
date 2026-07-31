using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Preview;
using Artemis.UI.Screens.ProfileEditor.DesignPanels.Section;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class DesignProfileViewModel : RoutableScreen<ProfileViewModelParameters>
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;

    [Notify] private ProfileConfiguration? _profileConfiguration;
    [Notify] private ReadOnlyObservableCollection<ConfigurationSectionDesignViewModel>? _sections;

    public DesignProfileViewModel(IProfileService profileService, IProfileEditorService profileEditorService, PreviewViewModel previewViewModel,
        Func<ConfigurationSection, ConfigurationSectionDesignViewModel> getConfigurationSectionDesignViewModel)
    {
        _profileService = profileService;
        _profileEditorService = profileEditorService;
        ParameterSource = ParameterSource.Route;

        PreviewViewModel = previewViewModel;

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(vm => vm.ProfileConfiguration)
                .Select(p => p?.ConfigurationSections.ToObservableChangeSet() ?? Observable.Empty<IChangeSet<ConfigurationSection>>())
                .Switch()
                .Transform(getConfigurationSectionDesignViewModel)
                .Bind(out ReadOnlyObservableCollection<ConfigurationSectionDesignViewModel> sections)
                .Subscribe()
                .DisposeWith(d);

            Sections = sections;
        });
    }

    public PreviewViewModel PreviewViewModel { get; }

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

    public async Task AddSection()
    {
        ProfileConfiguration?.ConfigurationSections.Add(new ConfigurationSection(ProfileConfiguration)
        {
            Name = $"New Section {ProfileConfiguration.ConfigurationSections.Count(s => s.Name.StartsWith("New Section")) + 1}"
        });
    }
}