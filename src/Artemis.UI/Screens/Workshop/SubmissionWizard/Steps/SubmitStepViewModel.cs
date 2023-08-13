using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using IdentityModel;
using ReactiveUI;
using StrawberryShake;
using System;
using System.IO;
using Avalonia.Media.Imaging;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class SubmitStepViewModel : SubmissionViewModel
{
    private ReadOnlyObservableCollection<CategoryViewModel>? _categories;
    private Bitmap? _iconBitmap;

    /// <inheritdoc />
    public SubmitStepViewModel(IAuthenticationService authenticationService, IWorkshopClient workshopClient)
    {
        CurrentUser = authenticationService.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value;
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<SpecificationsStepViewModel>());
        Continue = ReactiveCommand.Create(() => State.ChangeScreen<UploadStepViewModel>());
        
        ContinueText = "Submit";
        
        this.WhenActivated(d =>
        {
            if (State.Icon != null)
            {
                State.Icon.Seek(0, SeekOrigin.Begin);
                IconBitmap = new Bitmap(State.Icon);
                IconBitmap.DisposeWith(d);
            }
            Observable.FromAsync(workshopClient.GetCategories.ExecuteAsync).Subscribe(PopulateCategories).DisposeWith(d);
        });
    }

    public Bitmap? IconBitmap
    {
        get => _iconBitmap;
        set => RaiseAndSetIfChanged(ref _iconBitmap, value);
    }

    public string? CurrentUser { get; }

    public ReadOnlyObservableCollection<CategoryViewModel>? Categories
    {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }

    public override ReactiveCommand<Unit, Unit> Continue { get; }

    public override ReactiveCommand<Unit, Unit> GoBack { get; }

    private void PopulateCategories(IOperationResult<IGetCategoriesResult> result)
    {
        if (result.Data == null)
            Categories = null;
        else
        {
            Categories = new ReadOnlyObservableCollection<CategoryViewModel>(
                new ObservableCollection<CategoryViewModel>(result.Data.Categories.Where(c => State.Categories.Contains(c.Id)).Select(c => new CategoryViewModel(c)))
            );
        }
    }
}