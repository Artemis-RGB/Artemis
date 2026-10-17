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
using Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class ConfigureProfileViewModel : RoutableScreen<ProfileViewModelParameters>
{
    private readonly IProfileService _profileService;
    private readonly IProfileEditorService _profileEditorService;

    [Notify] private ProfileConfiguration? _profileConfiguration;
    [Notify] private ReadOnlyObservableCollection<ConfigurationSectionViewModel>? _sections;

    public ConfigureProfileViewModel(IProfileService profileService, IProfileEditorService profileEditorService, PreviewViewModel previewViewModel,
        Func<ConfigurationSection, ConfigurationSectionViewModel> getConfigurationSectionViewModel)
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
                .Transform(getConfigurationSectionViewModel)
                .Bind(out ReadOnlyObservableCollection<ConfigurationSectionViewModel> sections)
                .Subscribe()
                .DisposeWith(d);

            Sections = sections;

            this.WhenAnyValue(vm => vm.ProfileConfiguration)
                .Select(p => p != null
                    ? Observable.FromEventPattern(h => p.ConfigurationChanged += h, h => p.ConfigurationChanged -= h).Select(_ => p)
                    : Observable.Empty<ProfileConfiguration>())
                .Switch()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => _profileService.SaveProfileCategory(p.Category))
                .DisposeWith(d);
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
        // Don't lose changes that are still waiting on the throttled save
        if (ProfileConfiguration != null)
            _profileService.SaveProfileCategory(ProfileConfiguration.Category);

        if (!args.Path.StartsWith("profile"))
        {
            ProfileConfiguration = null;
            await _profileEditorService.ChangeCurrentProfileConfiguration(null);
        }
    }
}