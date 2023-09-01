using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarViewModel : ActivatableViewModelBase
{
    public const string ROOT_SCREEN = "root";

    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private ReadOnlyObservableCollection<SidebarCategoryViewModel> _sidebarCategories = new(new ObservableCollection<SidebarCategoryViewModel>());
    private SidebarScreenViewModel? _selectedScreen;
    private bool _updating;

    public SidebarViewModel(IRouter router, IProfileService profileService, IWindowService windowService, ISidebarVmFactory sidebarVmFactory)
    {
        _router = router;
        _windowService = windowService;

        SidebarScreen = new SidebarScreenViewModel(MaterialIconKind.Abacus, ROOT_SCREEN, "", null, new ObservableCollection<SidebarScreenViewModel>()
        {
            new(MaterialIconKind.HomeOutline, "Home", "home"),
            #if DEBUG
            new(MaterialIconKind.TestTube, "Workshop", "workshop", null, new ObservableCollection<SidebarScreenViewModel>
            {
                new(MaterialIconKind.FolderVideo, "Profiles", "workshop/profiles/1", "workshop/profiles"),
                new(MaterialIconKind.KeyboardVariant, "Layouts", "workshop/layouts/1", "workshop/layouts"),
                new(MaterialIconKind.Bookshelf, "Library", "workshop/library"),
            }),
            #endif
            new(MaterialIconKind.Devices, "Surface Editor", "surface-editor"),
            new(MaterialIconKind.SettingsOutline, "Settings", "settings")
        });

        AddCategory = ReactiveCommand.CreateFromTask(ExecuteAddCategory);
        this.WhenAnyValue(vm => vm.SelectedScreen).WhereNotNull().Subscribe(s => SidebarScreen.ExpandIfRequired(s));

        SourceList<ProfileCategory> profileCategories = new();
        this.WhenActivated(d =>
        {
            _router.CurrentPath.WhereNotNull().Subscribe(r =>
            {
                _updating = true;
                SelectedScreen = SidebarScreen.GetMatch(r);
                _updating = false;
            }).DisposeWith(d);

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
        });
    }

    public SidebarScreenViewModel SidebarScreen { get; }

    public SidebarScreenViewModel? SelectedScreen
    {
        get => _selectedScreen;
        set => RaiseAndSetIfChanged(ref _selectedScreen, value);
    }

    public ReadOnlyObservableCollection<SidebarCategoryViewModel> SidebarCategories
    {
        get => _sidebarCategories;
        set => RaiseAndSetIfChanged(ref _sidebarCategories, value);
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

    public void NavigateToScreen(SidebarScreenViewModel sidebarScreenViewModel)
    {
        if (_updating)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>
        {
            try
            {
                await _router.Navigate(sidebarScreenViewModel.Path);
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Navigation failed", e);
            }
        });
    }
}