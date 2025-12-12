using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Document;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntrySpecificationsViewModel : ValidatableViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _categoriesValid;
    private readonly ObservableAsPropertyHelper<bool> _iconValid;
    private readonly ObservableAsPropertyHelper<bool> _descriptionValid;
    private readonly IWorkshopClient _workshopClient;
    private readonly IWindowService _windowService;
    [Notify] private string _name = string.Empty;
    [Notify] private string _summary = string.Empty;
    [Notify] private string _description = string.Empty;
    [Notify] private bool _isDefault;
    [Notify] private bool _isEssential;
    [Notify] private bool _isDeviceProvider;
    [Notify] private bool _fit;
    [Notify] private Bitmap? _iconBitmap;
    [Notify(Setter.Private)] private bool _iconChanged;

    private string? _lastIconPath;

    public EntrySpecificationsViewModel(IWorkshopClient workshopClient, IWindowService windowService, IAuthenticationService authenticationService)
    {
        _workshopClient = workshopClient;
        _windowService = windowService;
        SelectIcon = ReactiveCommand.CreateFromTask(ExecuteSelectIcon);

        Categories.ToObservableChangeSet()
            .AutoRefresh(c => c.IsSelected)
            .Filter(c => c.IsSelected)
            .Transform(c => c.Id)
            .Bind(out ReadOnlyObservableCollection<long> selectedCategories)
            .Subscribe();
        SelectedCategories = selectedCategories;

        this.ValidationRule<EntrySpecificationsViewModel, string>(vm => vm.Name, s => !string.IsNullOrWhiteSpace(s), "Name is required");
        this.ValidationRule<EntrySpecificationsViewModel, string>(vm => vm.Summary, s => !string.IsNullOrWhiteSpace(s), "Summary is required");
        ValidationHelper descriptionRule = this.ValidationRule<EntrySpecificationsViewModel, string>(vm => vm.Description, s => !string.IsNullOrWhiteSpace(s), "Description is required");

        // These don't use inputs that support validation messages, do so manually
        ValidationHelper iconRule = this.ValidationRule<EntrySpecificationsViewModel, Bitmap>(vm => vm.IconBitmap, s => s != null, "Icon required");
        ValidationHelper categoriesRule = this.ValidationRule(vm => vm.Categories, Categories.ToObservableChangeSet().AutoRefresh(c => c.IsSelected).Filter(c => c.IsSelected).IsNotEmpty(),
            "At least one category must be selected"
        );
        _iconValid = iconRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.IconValid);
        _categoriesValid = categoriesRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.CategoriesValid);
        _descriptionValid = descriptionRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.DescriptionValid);

        IsAdministrator = authenticationService.GetRoles().Contains("Administrator");
        this.WhenActivatedAsync(async _ => await PopulateCategories());
        this.WhenAnyValue(vm => vm.Fit).Subscribe(_ => UpdateIcon());
    }

    public ReactiveCommand<Unit, Unit> SelectIcon { get; }

    public ObservableCollection<CategoryViewModel> Categories { get; } = new();
    public ObservableCollection<string> Tags { get; } = new();
    public ReadOnlyObservableCollection<long> SelectedCategories { get; }

    public bool CategoriesValid => _categoriesValid.Value;
    public bool IconValid => _iconValid.Value;
    public bool DescriptionValid => _descriptionValid.Value;
    public bool IsAdministrator { get; }

    public List<long> PreselectedCategories { get; set; } = new();
    public EntryType EntryType { get; set; }

    private async Task ExecuteSelectIcon()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("png").WithExtension("jpg").WithExtension("bmp").WithName("Bitmap image"))
            .ShowAsync();

        if (result == null)
            return;

        _lastIconPath = result[0];
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (_lastIconPath == null)
            return;

        IconBitmap?.Dispose();
        IconBitmap = BitmapExtensions.LoadAndResize(_lastIconPath, 128, Fit);
        IconChanged = true;
    }

    private async Task PopulateCategories()
    {
        IOperationResult<IGetCategoriesResult> categories = await _workshopClient.GetCategories.ExecuteAsync(EntryType);
        Categories.Clear();
        if (categories.Data != null)
            Categories.AddRange(categories.Data.Categories.Select(c => new CategoryViewModel(c) {IsSelected = PreselectedCategories.Contains(c.Id)}));
    }
}