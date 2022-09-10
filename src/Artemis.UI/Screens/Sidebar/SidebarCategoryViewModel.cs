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
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarCategoryViewModel : ActivatableViewModelBase
{
    private readonly IProfileService _profileService;
    private readonly SidebarViewModel _sidebarViewModel;
    private readonly ISidebarVmFactory _vmFactory;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _isCollapsed;
    private ObservableAsPropertyHelper<bool>? _isSuspended;
    private SidebarProfileConfigurationViewModel? _selectedProfileConfiguration;

    public SidebarCategoryViewModel(SidebarViewModel sidebarViewModel, ProfileCategory profileCategory, IProfileService profileService, IWindowService windowService,
        IProfileEditorService profileEditorService, ISidebarVmFactory vmFactory)
    {
        _sidebarViewModel = sidebarViewModel;
        _profileService = profileService;
        _windowService = windowService;
        _vmFactory = vmFactory;

        ProfileCategory = profileCategory;
        SourceList<ProfileConfiguration> profileConfigurations = new();

        // Only show items when not collapsed
        IObservable<Func<ProfileConfiguration, bool>> profileConfigurationsFilter = this.WhenAnyValue(vm => vm.IsCollapsed).Select(b => new Func<object, bool>(_ => !b));
        profileConfigurations.Connect()
            .Filter(profileConfigurationsFilter)
            .Sort(SortExpressionComparer<ProfileConfiguration>.Ascending(c => c.Order))
            .Transform(c => _vmFactory.SidebarProfileConfigurationViewModel(_sidebarViewModel, c))
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
            // Update the list of profiles whenever the category fires events
            Observable.FromEventPattern<ProfileConfigurationEventArgs>(x => profileCategory.ProfileConfigurationAdded += x, x => profileCategory.ProfileConfigurationAdded -= x)
                .Subscribe(e => profileConfigurations.Add(e.EventArgs.ProfileConfiguration))
                .DisposeWith(d);
            Observable.FromEventPattern<ProfileConfigurationEventArgs>(x => profileCategory.ProfileConfigurationRemoved += x, x => profileCategory.ProfileConfigurationRemoved -= x)
                .Subscribe(e => profileConfigurations.RemoveMany(profileConfigurations.Items.Where(c => c == e.EventArgs.ProfileConfiguration)))
                .DisposeWith(d);

            profileEditorService.ProfileConfiguration.Subscribe(p => SelectedProfileConfiguration = ProfileConfigurations.FirstOrDefault(c => ReferenceEquals(c.ProfileConfiguration, p)))
                .DisposeWith(d);

            _isCollapsed = ProfileCategory.WhenAnyValue(vm => vm.IsCollapsed).ToProperty(this, vm => vm.IsCollapsed).DisposeWith(d);
            _isSuspended = ProfileCategory.WhenAnyValue(vm => vm.IsSuspended).ToProperty(this, vm => vm.IsSuspended).DisposeWith(d);

            // Change the current profile configuration when a new one is selected
            this.WhenAnyValue(vm => vm.SelectedProfileConfiguration).WhereNotNull().Subscribe(s => profileEditorService.ChangeCurrentProfileConfiguration(s.ProfileConfiguration));
        });

        profileConfigurations.Edit(updater =>
        {
            foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                updater.Add(profileConfiguration);
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

    public SidebarProfileConfigurationViewModel? SelectedProfileConfiguration
    {
        get => _selectedProfileConfiguration;
        set => RaiseAndSetIfChanged(ref _selectedProfileConfiguration, value);
    }

    public void AddProfileConfiguration(ProfileConfiguration profileConfiguration, int? index)
    {
        ProfileCategory oldCategory = profileConfiguration.Category;
        ProfileCategory.AddProfileConfiguration(profileConfiguration, index);

        _profileService.SaveProfileCategory(ProfileCategory);
        // If the profile moved to a new category, also save the old category
        if (oldCategory != ProfileCategory)
            _profileService.SaveProfileCategory(oldCategory);
    }

    private async Task ExecuteRenameCategory()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Edit category")
            .WithViewModel(out SidebarCategoryEditViewModel vm, ("category", ProfileCategory))
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();

        _sidebarViewModel.UpdateProfileCategories();
    }

    private async Task ExecuteDeleteCategory()
    {
        if (await _windowService.ShowConfirmContentDialog($"Delete {ProfileCategory.Name}", "Do you want to delete this category and all its profiles?"))
            _profileService.DeleteProfileCategory(ProfileCategory);
    }

    private async Task ExecuteAddProfile()
    {
        ProfileConfiguration? result = await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(
            ("profileCategory", ProfileCategory),
            ("profileConfiguration", null)
        );
        if (result != null)
        {
            SidebarProfileConfigurationViewModel viewModel = _vmFactory.SidebarProfileConfigurationViewModel(_sidebarViewModel, result);
            SelectedProfileConfiguration = viewModel;
        }
    }
    
    private async Task ExecuteImportProfile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("Artemis profile"))
            .ShowAsync();

        if (result == null)
            return;

        string json = await File.ReadAllTextAsync(result[0]);
        ProfileConfigurationExportModel? profileConfigurationExportModel = null;
        try
        {
            profileConfigurationExportModel = JsonConvert.DeserializeObject<ProfileConfigurationExportModel>(json, IProfileService.ExportSettings);
        }
        catch (JsonException e)
        {
            _windowService.ShowExceptionDialog("Import profile failed", e);
        }

        if (profileConfigurationExportModel == null)
        {
            await _windowService.ShowConfirmContentDialog("Import profile", "Failed to import this profile, make sure it is a valid Artemis profile.", "Confirm", null);
            return;
        }

        try
        {
            _profileService.ImportProfile(ProfileCategory, profileConfigurationExportModel);
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

        categories[index - 1].Order++;
        ProfileCategory.Order--;
        _profileService.SaveProfileCategory(categories[index - 1]);
        _profileService.SaveProfileCategory(ProfileCategory);

        _sidebarViewModel.UpdateProfileCategories();
    }

    private void ExecuteMoveDown()
    {
        List<ProfileCategory> categories = _profileService.ProfileCategories.OrderBy(p => p.Order).ToList();
        int index = categories.IndexOf(ProfileCategory);
        if (index >= categories.Count - 1)
            return;

        categories[index + 1].Order--;
        ProfileCategory.Order++;
        _profileService.SaveProfileCategory(categories[index + 1]);
        _profileService.SaveProfileCategory(ProfileCategory);

        _sidebarViewModel.UpdateProfileCategories();
    }
}