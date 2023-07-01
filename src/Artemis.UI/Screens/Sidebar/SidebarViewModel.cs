﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private SidebarScreenViewModel? _selectedSidebarScreen;
    private ReadOnlyObservableCollection<SidebarCategoryViewModel> _sidebarCategories = new(new ObservableCollection<SidebarCategoryViewModel>());

    public SidebarViewModel(IRouter router, IProfileService profileService, IWindowService windowService, ISidebarVmFactory sidebarVmFactory)
    {
        _router = router;
        _windowService = windowService;

        SidebarScreens = new ObservableCollection<SidebarScreenViewModel>
        {
            new(MaterialIconKind.Home, "Home", "home"),
#if DEBUG
            new(MaterialIconKind.TestTube, "Workshop", "workshop"),
#endif
            new(MaterialIconKind.Devices, "Surface Editor", "surface-editor"),
            new(MaterialIconKind.Cog, "Settings", "settings")
        };

        AddCategory = ReactiveCommand.CreateFromTask(ExecuteAddCategory);

        SourceList<ProfileCategory> profileCategories = new();

        this.WhenAnyValue(vm => vm.SelectedSidebarScreen).WhereNotNull().Subscribe(NavigateToScreen);
        this.WhenActivated(d =>
        {
            _router.CurrentPath.WhereNotNull().Subscribe(r => SelectedSidebarScreen = SidebarScreens.FirstOrDefault(s => s.Matches(r))).DisposeWith(d);

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

    private void NavigateToScreen(SidebarScreenViewModel sidebarScreenViewModel)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            try
            {
                await _router.Navigate(sidebarScreenViewModel.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true});
            }
            catch (Exception e)
            {
                _windowService.ShowExceptionDialog("Navigation failed", e);
            }
        });
    }
}