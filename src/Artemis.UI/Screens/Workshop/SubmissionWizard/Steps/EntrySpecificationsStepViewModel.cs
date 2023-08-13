using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Extensions;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using StrawberryShake;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class EntrySpecificationsStepViewModel : SubmissionViewModel
{
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _categoriesValid;
    private ObservableAsPropertyHelper<bool>? _iconValid;
    private string _description = string.Empty;
    private Bitmap? _iconBitmap;
    private bool _isDirty;
    private string _name = string.Empty;
    private string _summary = string.Empty;

    public EntrySpecificationsStepViewModel(IWorkshopClient workshopClient, IWindowService windowService)
    {
        _windowService = windowService;
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        Continue = ReactiveCommand.Create(ExecuteContinue, ValidationContext.Valid);
        SelectIcon = ReactiveCommand.CreateFromTask(ExecuteSelectIcon);
        ClearIcon = ReactiveCommand.Create(ExecuteClearIcon);

        workshopClient.GetCategories
            .Watch(ExecutionStrategy.CacheFirst)
            .SelectOperationResult(c => c.Categories)
            .ToObservableChangeSet(c => c.Id)
            .Transform(c => new CategoryViewModel(c))
            .Bind(out ReadOnlyObservableCollection<CategoryViewModel> categoryViewModels)
            .Subscribe();
        Categories = categoryViewModels;

        this.WhenActivated(d =>
        {
            DisplayName = $"{State.EntryType} Information";
            
            // Basic fields
            Name = State.Name;
            Summary = State.Summary;
            Description = State.Description;

            // Categories
            foreach (CategoryViewModel categoryViewModel in Categories)
                categoryViewModel.IsSelected = State.Categories.Contains(categoryViewModel.Id);

            // Tags
            Tags.Clear();
            Tags.AddRange(State.Tags);

            // Icon
            if (State.Icon != null)
            {
                State.Icon.Seek(0, SeekOrigin.Begin);
                IconBitmap = BitmapExtensions.LoadAndResize(State.Icon, 128);
            }
            
            IsDirty = false;
            this.ClearValidationRules();

            Disposable.Create(() =>
            {
                IconBitmap?.Dispose();
                IconBitmap = null;
            }).DisposeWith(d);
        });
    }

    public override ReactiveCommand<Unit, Unit> Continue { get; }
    public override ReactiveCommand<Unit, Unit> GoBack { get; }
    public ReactiveCommand<Unit, Unit> SelectIcon { get; }
    public ReactiveCommand<Unit, Unit> ClearIcon { get; }

    public ReadOnlyObservableCollection<CategoryViewModel> Categories { get; }
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

    public bool IsDirty
    {
        get => _isDirty;
        set => RaiseAndSetIfChanged(ref _isDirty, value);
    }

    public Bitmap? IconBitmap
    {
        get => _iconBitmap;
        set => RaiseAndSetIfChanged(ref _iconBitmap, value);
    }

    private void ExecuteGoBack()
    {
        switch (State.EntryType)
        {
            case EntryType.Layout:
                break;
            case EntryType.Plugin:
                break;
            case EntryType.Profile:
                State.ChangeScreen<ProfileAdaptionHintsStepViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ExecuteContinue()
    {
        if (!IsDirty)
        {
            SetupDataValidation();
            IsDirty = true;

            // The ValidationContext seems to update asynchronously, so stop and schedule a retry
            Dispatcher.UIThread.Post(ExecuteContinue);
            return;
        }

        if (!ValidationContext.GetIsValid())
            return;

        State.Name = Name;
        State.Summary = Summary;
        State.Description = Description;
        State.Categories = Categories.Where(c => c.IsSelected).Select(c => c.Id).ToList();
        State.Tags = new List<string>(Tags);

        State.Icon?.Dispose();
        if (IconBitmap != null)
        {
            State.Icon = new MemoryStream();
            IconBitmap.Save(State.Icon);
        }
        else
        {
            State.Icon = null;
        }
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

    private void ExecuteClearIcon()
    {
        IconBitmap?.Dispose();
        IconBitmap = null;
    }

    private void SetupDataValidation()
    {
        // Hopefully this can be avoided in the future
        // https://github.com/reactiveui/ReactiveUI.Validation/discussions/558
        this.ValidationRule(vm => vm.Name, s => !string.IsNullOrWhiteSpace(s), "Name is required");
        this.ValidationRule(vm => vm.Summary, s => !string.IsNullOrWhiteSpace(s), "Summary is required");
        this.ValidationRule(vm => vm.Description, s => !string.IsNullOrWhiteSpace(s), "Description is required");
        
        // These don't use inputs that support validation messages, do so manually
        ValidationHelper iconRule = this.ValidationRule(vm => vm.IconBitmap, s => s != null, "Icon required");
        ValidationHelper categoriesRule = this.ValidationRule(vm => vm.Categories, Categories.ToObservableChangeSet()
                .AutoRefresh(c => c.IsSelected)
                .Filter(c => c.IsSelected)
                .IsEmpty()
                .CombineLatest(this.WhenAnyValue(vm => vm.IsDirty), (empty, dirty) => !dirty || !empty),
            "At least one category must be selected"
        );
        _iconValid = iconRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.IconValid);
        _categoriesValid = categoriesRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.CategoriesValid);
    }
}