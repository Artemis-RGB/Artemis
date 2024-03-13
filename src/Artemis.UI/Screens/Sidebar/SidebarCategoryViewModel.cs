using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarCategoryViewModel : ActivatableViewModelBase
{
    private readonly IProfileService _profileService;
    private readonly IRouter _router;
    private readonly ISidebarVmFactory _vmFactory;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _isCollapsed;
    private ObservableAsPropertyHelper<bool>? _isSuspended;
    [Notify] private SidebarProfileConfigurationViewModel? _selectedProfileConfiguration;

    public SidebarCategoryViewModel(ProfileCategory profileCategory, IProfileService profileService, IWindowService windowService, ISidebarVmFactory vmFactory, IRouter router)
    {
        _profileService = profileService;
        _windowService = windowService;
        _vmFactory = vmFactory;
        _router = router;

        ProfileCategory = profileCategory;
        SourceList<ProfileConfiguration> profileConfigurations = new();

        // Only show items when not collapsed
        IObservable<Func<ProfileConfiguration, bool>> profileConfigurationsFilter = this.WhenAnyValue(vm => vm.IsCollapsed).Select(b => new Func<object, bool>(_ => !b));
        profileConfigurations.Connect()
            .Filter(profileConfigurationsFilter)
            .Sort(SortExpressionComparer<ProfileConfiguration>.Ascending(c => c.Order))
            .Transform(c => _vmFactory.SidebarProfileConfigurationViewModel(c))
            .Bind(out ReadOnlyObservableCollection<SidebarProfileConfigurationViewModel> profileConfigurationViewModels)
            .Subscribe();
        ProfileConfigurations = profileConfigurationViewModels;

        ToggleCollapsed = ReactiveCommand.Create(ExecuteToggleCollapsed);
        ToggleSuspended = ReactiveCommand.Create(ExecuteToggleSuspended);
        AddProfile = ReactiveCommand.CreateFromTask(ExecuteAddProfile);
        ImportProfile = ReactiveCommand.CreateFromTask(ExecuteImportProfile);
        MoveUp = ReactiveCommand.Create(ExecuteMoveUp);
        MoveDown = ReactiveCommand.Create(ExecuteMoveDown);
        RenameCategory = ReactiveCommand.CreateFromTask(ExecuteRenameCategory);
        DeleteCategory = ReactiveCommand.CreateFromTask(ExecuteDeleteCategory);

        this.WhenActivated(d =>
        {
            // Navigate on selection change
            this.WhenAnyValue(vm => vm.SelectedProfileConfiguration)
                .WhereNotNull()
                .Subscribe(s => _router.Navigate($"profile-editor/{s.ProfileConfiguration.ProfileId}", new RouterNavigationOptions {IgnoreOnPartialMatch = true, RecycleScreens = false}))
                .DisposeWith(d);

            _router.CurrentPath.WhereNotNull().Subscribe(r => SelectedProfileConfiguration = ProfileConfigurations.FirstOrDefault(c => c.Matches(r))).DisposeWith(d);

            // Update the list of profiles whenever the category fires events
            Observable.FromEventPattern<ProfileConfigurationEventArgs>(x => profileCategory.ProfileConfigurationAdded += x, x => profileCategory.ProfileConfigurationAdded -= x)
                .Subscribe(e => profileConfigurations.Add(e.EventArgs.ProfileConfiguration))
                .DisposeWith(d);
            Observable.FromEventPattern<ProfileConfigurationEventArgs>(x => profileCategory.ProfileConfigurationRemoved += x, x => profileCategory.ProfileConfigurationRemoved -= x)
                .Subscribe(e => profileConfigurations.RemoveMany(profileConfigurations.Items.Where(c => c == e.EventArgs.ProfileConfiguration)))
                .DisposeWith(d);

            profileConfigurations.Edit(updater =>
            {
                updater.Clear();
                updater.AddRange(profileCategory.ProfileConfigurations);
            });

            _isCollapsed = ProfileCategory.WhenAnyValue(vm => vm.IsCollapsed).ToProperty(this, vm => vm.IsCollapsed).DisposeWith(d);
            _isSuspended = ProfileCategory.WhenAnyValue(vm => vm.IsSuspended).ToProperty(this, vm => vm.IsSuspended).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> ImportProfile { get; }
    public ReactiveCommand<Unit, Unit> ToggleCollapsed { get; }
    public ReactiveCommand<Unit, Unit> ToggleSuspended { get; }
    public ReactiveCommand<Unit, Unit> AddProfile { get; }
    public ReactiveCommand<Unit, Unit> MoveUp { get; }
    public ReactiveCommand<Unit, Unit> MoveDown { get; }
    public ReactiveCommand<Unit, Unit> RenameCategory { get; }
    public ReactiveCommand<Unit, Unit> DeleteCategory { get; }

    public ProfileCategory ProfileCategory { get; }
    public ReadOnlyObservableCollection<SidebarProfileConfigurationViewModel> ProfileConfigurations { get; }
    public bool IsCollapsed => _isCollapsed?.Value ?? false;
    public bool IsSuspended => _isSuspended?.Value ?? false;
    
    public void AddProfileConfiguration(ProfileConfiguration profileConfiguration, ProfileConfiguration? target)
    {
        ProfileCategory oldCategory = profileConfiguration.Category;
        ProfileCategory.AddProfileConfiguration(profileConfiguration, target);

        _profileService.SaveProfileCategory(ProfileCategory);
        // If the profile moved to a new category, also save the old category
        if (oldCategory != ProfileCategory)
            _profileService.SaveProfileCategory(oldCategory);
    }

    private async Task ExecuteRenameCategory()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Edit category")
            .WithViewModel(out SidebarCategoryEditViewModel vm, ProfileCategory)
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();
    }

    private async Task ExecuteDeleteCategory()
    {
        if (await _windowService.ShowConfirmContentDialog($"Delete {ProfileCategory.Name}", "Do you want to delete this category and all its profiles?"))
        {
            if (ProfileCategory.ProfileConfigurations.Any(c => _profileService.FocusProfile == c))
                await _router.Navigate("home");
            _profileService.DeleteProfileCategory(ProfileCategory);
        }
    }

    private async Task ExecuteAddProfile()
    {
        ProfileConfiguration? result = await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(ProfileCategory, ProfileConfiguration.Empty);
        if (result != null)
        {
            SidebarProfileConfigurationViewModel viewModel = _vmFactory.SidebarProfileConfigurationViewModel(result);
            SelectedProfileConfiguration = viewModel;
        }
    }

    private async Task ExecuteImportProfile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("zip").WithName("Artemis profile"))
            .ShowAsync();

        if (result == null)
            return;

        try
        {
            await using FileStream fileStream = File.OpenRead(result[0]);
            await _profileService.ImportProfile(fileStream, ProfileCategory, true, true);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Import profile failed", e);
        }
    }

    private void ExecuteToggleCollapsed()
    {
        ProfileCategory.IsCollapsed = !ProfileCategory.IsCollapsed;
        _profileService.SaveProfileCategory(ProfileCategory);
    }

    private void ExecuteToggleSuspended()
    {
        ProfileCategory.IsSuspended = !ProfileCategory.IsSuspended;
        _profileService.SaveProfileCategory(ProfileCategory);
    }

    private void ExecuteMoveUp()
    {
        List<ProfileCategory> categories = _profileService.ProfileCategories.OrderBy(p => p.Order).ToList();
        int index = categories.IndexOf(ProfileCategory);
        if (index <= 0)
            return;

        categories.Remove(ProfileCategory);
        categories.Insert(index - 1, ProfileCategory);
        ApplyCategoryOrder(categories);
    }

    private void ExecuteMoveDown()
    {
        List<ProfileCategory> categories = _profileService.ProfileCategories.OrderBy(p => p.Order).ToList();
        int index = categories.IndexOf(ProfileCategory);
        if (index >= categories.Count - 1)
            return;

        categories.Remove(ProfileCategory);
        categories.Insert(index + 1, ProfileCategory);
        ApplyCategoryOrder(categories);
    }

    private void ApplyCategoryOrder(List<ProfileCategory> categories)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].Order == i + 1)
                continue;
            categories[i].Order = i + 1;
            _profileService.SaveProfileCategory(categories[i]);
        }
    }
}