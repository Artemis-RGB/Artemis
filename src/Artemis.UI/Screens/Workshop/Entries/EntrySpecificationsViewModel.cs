using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Entries.Windows;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using StrawberryShake;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Artemis.UI.Screens.Workshop.Entries;

public class EntrySpecificationsViewModel : ValidatableViewModelBase
{
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _categoriesValid;
    private ObservableAsPropertyHelper<bool>? _iconValid;
    private string _description = string.Empty;
    private string _name = string.Empty;
    private string _summary = string.Empty;
    private Bitmap? _iconBitmap;
    private Window? _previewWindow;
    private TextDocument? _markdownDocument;

    public EntrySpecificationsViewModel(IWorkshopClient workshopClient, IWindowService windowService)
    {
        _windowService = windowService;
        SelectIcon = ReactiveCommand.CreateFromTask(ExecuteSelectIcon);
        OpenMarkdownPreview = ReactiveCommand.Create(ExecuteOpenMarkdownPreview);

        // this.WhenAnyValue(vm => vm.Description).Subscribe(d => MarkdownDocument.Text = d);
        this.WhenActivated(d =>
        {
            // Load categories
            Observable.FromAsync(workshopClient.GetCategories.ExecuteAsync).Subscribe(PopulateCategories).DisposeWith(d);

            this.ClearValidationRules();

            MarkdownDocument = new TextDocument(new StringTextSource(Description));
            MarkdownDocument.TextChanged += MarkdownDocumentOnTextChanged;
            Disposable.Create(() =>
            {
                _previewWindow?.Close();
                MarkdownDocument.TextChanged -= MarkdownDocumentOnTextChanged;
                MarkdownDocument = null;
                ClearIcon();
            }).DisposeWith(d);
        });
    }

    private void MarkdownDocumentOnTextChanged(object? sender, EventArgs e)
    {
        Description = MarkdownDocument.Text;
    }

    public ReactiveCommand<Unit, Unit> SelectIcon { get; }
    public ReactiveCommand<Unit, Unit> OpenMarkdownPreview { get; }

    public ObservableCollection<CategoryViewModel> Categories { get; } = new();
    public ObservableCollection<string> Tags { get; } = new();
    public bool CategoriesValid => _categoriesValid?.Value ?? true;
    public bool IconValid => _iconValid?.Value ?? true;

    public string Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Summary
    {
        get => _summary;
        set => RaiseAndSetIfChanged(ref _summary, value);
    }

    public string Description
    {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }

    public Bitmap? IconBitmap
    {
        get => _iconBitmap;
        set => RaiseAndSetIfChanged(ref _iconBitmap, value);
    }

    public TextDocument? MarkdownDocument
    {
        get => _markdownDocument;
        set => RaiseAndSetIfChanged(ref _markdownDocument, value);
    }

    public List<int> PreselectedCategories { get; set; } = new List<int>();

    public void SetupDataValidation()
    {
        // Hopefully this can be avoided in the future
        // https://github.com/reactiveui/ReactiveUI.Validation/discussions/558
        this.ValidationRule(vm => vm.Name, s => !string.IsNullOrWhiteSpace(s), "Name is required");
        this.ValidationRule(vm => vm.Summary, s => !string.IsNullOrWhiteSpace(s), "Summary is required");
        this.ValidationRule(vm => vm.Description, s => !string.IsNullOrWhiteSpace(s), "Description is required");

        // These don't use inputs that support validation messages, do so manually
        ValidationHelper iconRule = this.ValidationRule(vm => vm.IconBitmap, s => s != null, "Icon required");
        ValidationHelper categoriesRule = this.ValidationRule(vm => vm.Categories, Categories.ToObservableChangeSet().AutoRefresh(c => c.IsSelected).Filter(c => c.IsSelected).IsNotEmpty(),
            "At least one category must be selected"
        );
        _iconValid = iconRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.IconValid);
        _categoriesValid = categoriesRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.CategoriesValid);
    }

    private async Task ExecuteSelectIcon()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("png").WithExtension("jpg").WithExtension("bmp").WithName("Bitmap image"))
            .ShowAsync();

        if (result == null)
            return;

        IconBitmap?.Dispose();
        IconBitmap = BitmapExtensions.LoadAndResize(result[0], 128);
    }

    private void ExecuteOpenMarkdownPreview()
    {
        if (_previewWindow != null)
        {
            _previewWindow.Activate();
            return;
        }

        _previewWindow = _windowService.ShowWindow(out MarkdownPreviewViewModel _, this.WhenAnyValue(vm => vm.Description));
        _previewWindow.Closed += PreviewWindowOnClosed;
    }

    private void PreviewWindowOnClosed(object? sender, EventArgs e)
    {
        if (_previewWindow != null)
            _previewWindow.Closed -= PreviewWindowOnClosed;
        _previewWindow = null;
    }

    private void ClearIcon()
    {
        IconBitmap?.Dispose();
        IconBitmap = null;
    }

    private void PopulateCategories(IOperationResult<IGetCategoriesResult> result)
    {
        Categories.Clear();
        if (result.Data != null)
            Categories.AddRange(result.Data.Categories.Select(c => new CategoryViewModel(c) {IsSelected = PreselectedCategories.Contains(c.Id)}));
    }
}