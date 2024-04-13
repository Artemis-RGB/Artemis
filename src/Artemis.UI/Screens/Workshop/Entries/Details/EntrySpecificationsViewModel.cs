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
    [Notify] private Bitmap? _iconBitmap;
    [Notify(Setter.Private)] private bool _iconChanged;

    public EntrySpecificationsViewModel(IWorkshopClient workshopClient, IWindowService windowService)
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

        this.WhenActivatedAsync(async d =>
        {
            // Load categories
            await PopulateCategories();
            Disposable.Create(ClearIcon).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> SelectIcon { get; }

    public ObservableCollection<CategoryViewModel> Categories { get; } = new();
    public ObservableCollection<string> Tags { get; } = new();
    public ReadOnlyObservableCollection<long> SelectedCategories { get; }
    
    public bool CategoriesValid => _categoriesValid.Value ;
    public bool IconValid => _iconValid.Value;
    public bool DescriptionValid => _descriptionValid.Value;

    public List<long> PreselectedCategories { get; set; } = new();
    
    private async Task ExecuteSelectIcon()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("png").WithExtension("jpg").WithExtension("bmp").WithName("Bitmap image"))
            .ShowAsync();

        if (result == null)
            return;

        IconBitmap?.Dispose();
        IconBitmap = BitmapExtensions.LoadAndResize(result[0], 128);
        IconChanged = true;
    }

    private void ClearIcon()
    {
        IconBitmap?.Dispose();
        IconBitmap = null;
    }

    private async Task PopulateCategories()
    {
        IOperationResult<IGetCategoriesResult> categories = await _workshopClient.GetCategories.ExecuteAsync();
        Categories.Clear();
        if (categories.Data != null)
            Categories.AddRange(categories.Data.Categories.Select(c => new CategoryViewModel(c) {IsSelected = PreselectedCategories.Contains(c.Id)}));
    }
}