using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public class SpecificationsStepViewModel : SubmissionViewModel
{
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _categoriesValid;
    private ObservableAsPropertyHelper<bool>? _iconValid;
    private string _description = string.Empty;
    private string _name = string.Empty;
    private string _summary = string.Empty;
    private Bitmap? _iconBitmap;

    public SpecificationsStepViewModel(IWorkshopClient workshopClient, IWindowService windowService)
    {
        _windowService = windowService;
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        Continue = ReactiveCommand.Create(ExecuteContinue, ValidationContext.Valid);
        SelectIcon = ReactiveCommand.CreateFromTask(ExecuteSelectIcon);
        ClearIcon = ReactiveCommand.Create(ExecuteClearIcon);

        this.WhenActivated(d =>
        {
            DisplayName = $"{State.EntryType} Information";

            // Load categories
            Observable.FromAsync(workshopClient.GetCategories.ExecuteAsync).Subscribe(PopulateCategories).DisposeWith(d);

            // Apply the state
            ApplyFromState();

            this.ClearValidationRules();
            Disposable.Create(ExecuteClearIcon).DisposeWith(d);
        });
    }

    public override ReactiveCommand<Unit, Unit> Continue { get; }
    public override ReactiveCommand<Unit, Unit> GoBack { get; }
    public ReactiveCommand<Unit, Unit> SelectIcon { get; }
    public ReactiveCommand<Unit, Unit> ClearIcon { get; }

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

    private void ExecuteGoBack()
    {
        // Apply what's there so far
        ApplyToState();

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
        if (!ValidationContext.Validations.Any())
        {
            // The ValidationContext seems to update asynchronously, so stop and schedule a retry
            SetupDataValidation();
            Dispatcher.UIThread.Post(ExecuteContinue);
            return;
        }

        ApplyToState();
        
        if (!ValidationContext.GetIsValid())
            return;
        
        State.ChangeScreen<SubmitStepViewModel>();
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

    private void PopulateCategories(IOperationResult<IGetCategoriesResult> result)
    {
        Categories.Clear();
        if (result.Data != null)
            Categories.AddRange(result.Data.Categories.Select(c => new CategoryViewModel(c) {IsSelected = State.Categories.Contains(c.Id)}));
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
        ValidationHelper categoriesRule = this.ValidationRule(vm => vm.Categories, Categories.ToObservableChangeSet().AutoRefresh(c => c.IsSelected).Filter(c => c.IsSelected).IsNotEmpty(),
            "At least one category must be selected"
        );
        _iconValid = iconRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.IconValid);
        _categoriesValid = categoriesRule.ValidationChanged.Select(c => c.IsValid).ToProperty(this, vm => vm.CategoriesValid);
    }

    private void ApplyFromState()
    {
        // Basic fields
        Name = State.Name;
        Summary = State.Summary;
        Description = State.Description;

        // Tags
        Tags.Clear();
        Tags.AddRange(State.Tags);

        // Icon
        if (State.Icon != null)
        {
            State.Icon.Seek(0, SeekOrigin.Begin);
            IconBitmap = BitmapExtensions.LoadAndResize(State.Icon, 128);
        }
    }

    private void ApplyToState()
    {
        // Basic fields
        State.Name = Name;
        State.Summary = Summary;
        State.Description = Description;

        // Categories and tasks
        State.Categories = Categories.Where(c => c.IsSelected).Select(c => c.Id).ToList();
        State.Tags = new List<string>(Tags);

        // Icon
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
}