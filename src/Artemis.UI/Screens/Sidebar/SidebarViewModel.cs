using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.ReactiveUI;
using DryIoc;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarViewModel : ActivatableViewModelBase
{
    private readonly IScreen _hostScreen;
    private readonly IContainer _container;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileEditorVmFactory _profileEditorVmFactory;
    private readonly IWindowService _windowService;
    private SidebarScreenViewModel? _selectedSidebarScreen;
    private ReadOnlyObservableCollection<SidebarCategoryViewModel> _sidebarCategories = new(new ObservableCollection<SidebarCategoryViewModel>());

    public SidebarViewModel(IScreen hostScreen,
        IContainer container,
        IProfileService profileService,
        IWindowService windowService,
        IProfileEditorService profileEditorService,
        ISidebarVmFactory sidebarVmFactory,
        IProfileEditorVmFactory profileEditorVmFactory)
    {
        _hostScreen = hostScreen;
        _container = container;
        _windowService = windowService;
        _profileEditorService = profileEditorService;
        _profileEditorVmFactory = profileEditorVmFactory;

        SidebarScreens = new ObservableCollection<SidebarScreenViewModel>
        {
            new SidebarScreenViewModel<HomeViewModel>(MaterialIconKind.Home, "Home"),
#if DEBUG
            new SidebarScreenViewModel<WorkshopViewModel>(MaterialIconKind.TestTube, "Workshop"),
#endif
            new SidebarScreenViewModel<SurfaceEditorViewModel>(MaterialIconKind.Devices, "Surface Editor"),
            new SidebarScreenViewModel<SettingsViewModel>(MaterialIconKind.Cog, "Settings")
        };

        AddCategory = ReactiveCommand.CreateFromTask(ExecuteAddCategory);

        SourceList<ProfileCategory> profileCategories = new();

        this.WhenActivated(d =>
        {
            this.WhenAnyObservable(vm => vm._hostScreen.Router.CurrentViewModel).WhereNotNull()
                .Subscribe(c => SelectedSidebarScreen = SidebarScreens.FirstOrDefault(s => s.ScreenType == c.GetType()))
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.SelectedSidebarScreen).WhereNotNull().Subscribe(NavigateToScreen);
            this.WhenAnyObservable(vm => vm._profileEditorService.ProfileConfiguration).Subscribe(NavigateToProfile).DisposeWith(d);

            Observable.FromEventPattern<ProfileCategoryEventArgs>(x => profileService.ProfileCategoryAdded += x, x => profileService.ProfileCategoryAdded -= x)
                .Subscribe(e => profileCategories.Add(e.EventArgs.ProfileCategory))
                .DisposeWith(d);
            Observable.FromEventPattern<ProfileCategoryEventArgs>(x => profileService.ProfileCategoryRemoved += x, x => profileService.ProfileCategoryRemoved -= x)
                .Subscribe(e => profileCategories.Remove(e.EventArgs.ProfileCategory))
                .DisposeWith(d);

            profileCategories.Edit(c =>
            {
                c.Clear();
                c.AddRange(profileService.ProfileCategories);
            });

            profileCategories.Connect()
                .AutoRefresh(p => p.Order)
                .Sort(SortExpressionComparer<ProfileCategory>.Ascending(p => p.Order))
                .Transform(sidebarVmFactory.SidebarCategoryViewModel)
                .ObserveOn(AvaloniaScheduler.Instance)
                .Bind(out ReadOnlyObservableCollection<SidebarCategoryViewModel> categoryViewModels)
                .Subscribe()
                .DisposeWith(d);

            SidebarCategories = categoryViewModels;
            SelectedSidebarScreen = SidebarScreens.First();
        });
    }

    public ObservableCollection<SidebarScreenViewModel> SidebarScreens { get; }

    public ReadOnlyObservableCollection<SidebarCategoryViewModel> SidebarCategories
    {
        get => _sidebarCategories;
        set => RaiseAndSetIfChanged(ref _sidebarCategories, value);
    }

    public SidebarScreenViewModel? SelectedSidebarScreen
    {
        get => _selectedSidebarScreen;
        set => RaiseAndSetIfChanged(ref _selectedSidebarScreen, value);
    }

    public ReactiveCommand<Unit, Unit> AddCategory { get; }

    private async Task ExecuteAddCategory()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Add new category")
            .WithViewModel(out SidebarCategoryEditViewModel vm, ProfileCategory.Empty)
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();
    }

    private void NavigateToProfile(ProfileConfiguration? profile)
    {
        if (profile == null && _hostScreen.Router.GetCurrentViewModel() is ProfileEditorViewModel)
            SelectedSidebarScreen = SidebarScreens.FirstOrDefault();
        else if (profile != null && _hostScreen.Router.GetCurrentViewModel() is not ProfileEditorViewModel)
            _hostScreen.Router.Navigate.Execute(_profileEditorVmFactory.ProfileEditorViewModel(_hostScreen));
    }

    private void NavigateToScreen(SidebarScreenViewModel sidebarScreenViewModel)
    {
        // If the current screen changed through external means and already matches, don't navigate again
        if (_hostScreen.Router.GetCurrentViewModel()?.GetType() == sidebarScreenViewModel.ScreenType)
            return;
        
        _hostScreen.Router.Navigate.Execute(sidebarScreenViewModel.CreateInstance(_container, _hostScreen));
        _profileEditorService.ChangeCurrentProfileConfiguration(null);
    }
}