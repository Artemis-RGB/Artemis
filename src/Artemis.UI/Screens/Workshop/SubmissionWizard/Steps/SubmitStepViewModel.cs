using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using IdentityModel;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class SubmitStepViewModel : SubmissionViewModel
{
    [Notify] private ReadOnlyObservableCollection<CategoryViewModel>? _categories;
    [Notify] private Bitmap? _iconBitmap;

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

            Observable.FromAsync(() => workshopClient.GetCategories.ExecuteAsync(State.EntryType)).Subscribe(PopulateCategories).DisposeWith(d);
        });
    }
    
    public string? CurrentUser { get; }
    
    private void PopulateCategories(IOperationResult<IGetCategoriesResult> result)
    {
        if (result.Data == null)
            Categories = null;
        else
            Categories = new ReadOnlyObservableCollection<CategoryViewModel>(
                new ObservableCollection<CategoryViewModel>(result.Data.Categories.Where(c => State.Categories.Contains(c.Id)).Select(c => new CategoryViewModel(c)))
            );
    }
}