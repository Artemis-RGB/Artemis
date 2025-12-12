using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Image;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;
using EntrySpecificationsViewModel = Artemis.UI.Screens.Workshop.Entries.Details.EntrySpecificationsViewModel;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionDetailsViewModel : RoutableScreen
{
    private readonly IWorkshopClient _client;
    private readonly IWindowService _windowService;
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    private readonly Func<EntrySpecificationsViewModel> _getGetSpecificationsViewModel;
    private readonly Func<IImage, ImageSubmissionViewModel> _getExistingImageSubmissionViewModel;
    private readonly Func<ImageUploadRequest, ImageSubmissionViewModel> _getImageSubmissionViewModel;
    private readonly List<ImageSubmissionViewModel> _removedImages = new();

    [Notify] private IGetSubmittedEntryById_Entry? _entry;
    [Notify] private EntrySpecificationsViewModel? _entrySpecificationsViewModel;
    [Notify(Setter.Private)] private bool _hasChanges;

    public SubmissionDetailsViewModel(IWorkshopClient client,
        IWindowService windowService,
        IWorkshopService workshopService,
        IRouter router,
        Func<EntrySpecificationsViewModel> getSpecificationsViewModel,
        Func<IImage, ImageSubmissionViewModel> getExistingImageSubmissionViewModel,
        Func<ImageUploadRequest, ImageSubmissionViewModel> getImageSubmissionViewModel)
    {
        _client = client;
        _windowService = windowService;
        _workshopService = workshopService;
        _router = router;
        _getGetSpecificationsViewModel = getSpecificationsViewModel;
        _getExistingImageSubmissionViewModel = getExistingImageSubmissionViewModel;
        _getImageSubmissionViewModel = getImageSubmissionViewModel;

        AddImage = ReactiveCommand.CreateFromTask(ExecuteAddImage);
        DiscardChanges = ReactiveCommand.CreateFromTask(ExecuteDiscardChanges, this.WhenAnyValue(vm => vm.HasChanges));
        SaveChanges = ReactiveCommand.CreateFromTask(ExecuteSaveChanges, this.WhenAnyValue(vm => vm.HasChanges));
    }

    public ObservableCollection<ImageSubmissionViewModel> Images { get; } = new();
    public ReactiveCommand<Unit, Unit> AddImage { get; }
    public ReactiveCommand<Unit, Unit> SaveChanges { get; }
    public ReactiveCommand<Unit, Unit> DiscardChanges { get; }

    public async Task SetEntry(IGetSubmittedEntryById_Entry? entry, CancellationToken cancellationToken)
    {
        Entry = entry;
        await ApplyDetailsFromEntry(cancellationToken);
    }

    public async Task OnClosing(NavigationArguments args)
    {
        if (!HasChanges)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog("You have unsaved changes", "Do you want to discard your unsaved changes?");
        if (!confirmed)
            args.Cancel();
        else
            await ExecuteDiscardChanges();
    }

    private async Task ApplyDetailsFromEntry(CancellationToken cancellationToken)
    {
        // Clean up event handlers
        if (EntrySpecificationsViewModel != null)
        {
            EntrySpecificationsViewModel.PropertyChanged -= InputChanged;
            ((INotifyCollectionChanged) EntrySpecificationsViewModel.SelectedCategories).CollectionChanged -= InputChanged;
            EntrySpecificationsViewModel.Tags.CollectionChanged -= InputChanged;
        }

        if (Entry == null)
        {
            EntrySpecificationsViewModel = null;
            ApplyImagesFromEntry();
            return;
        }

        EntrySpecificationsViewModel specificationsViewModel = _getGetSpecificationsViewModel();
        specificationsViewModel.IconBitmap = await GetEntryIcon(cancellationToken);
        specificationsViewModel.Name = Entry.Name;
        specificationsViewModel.Summary = Entry.Summary;
        specificationsViewModel.Description = Entry.Description;
        specificationsViewModel.IsDefault = Entry.DefaultEntryInfo != null;
        specificationsViewModel.IsEssential = Entry.DefaultEntryInfo?.IsEssential ?? false;
        specificationsViewModel.IsDeviceProvider = Entry.DefaultEntryInfo?.IsDeviceProvider ?? false;
        specificationsViewModel.PreselectedCategories = Entry.Categories.Select(c => c.Id).ToList();
        specificationsViewModel.EntryType = Entry.EntryType;

        specificationsViewModel.Tags.Clear();
        foreach (string tag in Entry.Tags.Select(c => c.Name))
            specificationsViewModel.Tags.Add(tag);

        EntrySpecificationsViewModel = specificationsViewModel;
        EntrySpecificationsViewModel.PropertyChanged += InputChanged;
        ((INotifyCollectionChanged) EntrySpecificationsViewModel.SelectedCategories).CollectionChanged += InputChanged;
        EntrySpecificationsViewModel.Tags.CollectionChanged += InputChanged;

        ApplyImagesFromEntry();
    }

    private void ApplyImagesFromEntry()
    {
        foreach (ImageSubmissionViewModel imageSubmissionViewModel in Images)
            imageSubmissionViewModel.PropertyChanged -= InputChanged;

        Images.Clear();
        _removedImages.Clear();

        if (Entry == null)
            return;

        foreach (IImage image in Entry.Images)
            AddImageViewModel(_getExistingImageSubmissionViewModel(image));
    }

    private void AddImageViewModel(ImageSubmissionViewModel viewModel)
    {
        viewModel.PropertyChanged += InputChanged;
        viewModel.Remove = ReactiveCommand.Create(() =>
        {
            // _removedImages is a list of images that are to be deleted, images without an ID never existed in the first place so only add those with an ID
            if (viewModel.Id != null)
                _removedImages.Add(viewModel);

            Images.Remove(viewModel);
            UpdateHasChanges();
        });
        Images.Add(viewModel);
    }

    private async Task<Bitmap?> GetEntryIcon(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return null;

        Stream? stream = await _workshopService.GetEntryIcon(Entry.Id, cancellationToken);
        return stream != null ? new Bitmap(stream) : null;
    }

    private void UpdateHasChanges()
    {
        if (EntrySpecificationsViewModel == null || Entry == null)
            return;

        List<long> categories = EntrySpecificationsViewModel.Categories.Where(c => c.IsSelected).Select(c => c.Id).OrderBy(c => c).ToList();
        List<string> tags = EntrySpecificationsViewModel.Tags.OrderBy(t => t).ToList();

        HasChanges = EntrySpecificationsViewModel.Name != Entry.Name ||
                     EntrySpecificationsViewModel.Description != Entry.Description ||
                     EntrySpecificationsViewModel.Summary != Entry.Summary ||
                     EntrySpecificationsViewModel.IconChanged ||
                     HasAdminChanges() || 
                     !tags.SequenceEqual(Entry.Tags.Select(t => t.Name).OrderBy(t => t)) ||
                     !categories.SequenceEqual(Entry.Categories.Select(c => c.Id).OrderBy(c => c)) ||
                     Images.Any(i => i.HasChanges) ||
                     _removedImages.Any();
    }

    private bool HasAdminChanges()
    {
        if (EntrySpecificationsViewModel == null || Entry == null)
            return false;

        bool isDefault = Entry.DefaultEntryInfo != null;
        bool isEssential = Entry.DefaultEntryInfo?.IsEssential ?? false;
        bool isDeviceProvider = Entry.DefaultEntryInfo?.IsDeviceProvider ?? false;

        return EntrySpecificationsViewModel.IsDefault != isDefault ||
               (EntrySpecificationsViewModel.IsDefault && (
                   EntrySpecificationsViewModel.IsEssential != isEssential ||
                   EntrySpecificationsViewModel.IsDeviceProvider != isDeviceProvider));
    }

    private async Task ExecuteDiscardChanges()
    {
        await ApplyDetailsFromEntry(CancellationToken.None);
    }

    private async Task ExecuteSaveChanges(CancellationToken cancellationToken)
    {
        if (Entry == null || EntrySpecificationsViewModel == null || !EntrySpecificationsViewModel.ValidationContext.GetIsValid())
            return;

        UpdateEntryInput input = new()
        {
            Id = Entry.Id,
            Name = EntrySpecificationsViewModel.Name,
            Summary = EntrySpecificationsViewModel.Summary,
            Description = EntrySpecificationsViewModel.Description,
            Categories = EntrySpecificationsViewModel.SelectedCategories,
            Tags = EntrySpecificationsViewModel.Tags,
            DefaultEntryInfo = EntrySpecificationsViewModel.IsDefault
                ? new DefaultEntryInfoInput
                {
                    IsEssential = EntrySpecificationsViewModel.IsEssential,
                    IsDeviceProvider = EntrySpecificationsViewModel.IsDeviceProvider
                }
                : null
        };

        IOperationResult<IUpdateEntryResult> result = await _client.UpdateEntry.ExecuteAsync(input, cancellationToken);
        result.EnsureNoErrors();

        if (EntrySpecificationsViewModel.IconChanged && EntrySpecificationsViewModel.IconBitmap != null)
        {
            using MemoryStream stream = new();
            EntrySpecificationsViewModel.IconBitmap.Save(stream);
            ApiResult imageResult = await _workshopService.SetEntryIcon(Entry.Id, stream, cancellationToken);
            if (!imageResult.IsSuccess)
                throw new ArtemisWorkshopException("Failed to upload image. " + imageResult.Message);
        }

        foreach (ImageSubmissionViewModel imageViewModel in Images)
        {
            // Upload new images
            if (imageViewModel.ImageUploadRequest != null)
            {
                await _workshopService.UploadEntryImage(Entry.Id, imageViewModel.ImageUploadRequest, cancellationToken);
            }
            // Update existing images
            else if (imageViewModel.HasChanges && imageViewModel.Id != null)
            {
                if (imageViewModel.Name != null)
                    await _client.UpdateEntryImage.ExecuteAsync(imageViewModel.Id.Value, imageViewModel.Name, imageViewModel.Description, cancellationToken);
            }
        }

        // Delete old images
        foreach (ImageSubmissionViewModel imageViewModel in _removedImages)
        {
            if (imageViewModel.Id != null)
                await _workshopService.DeleteEntryImage(imageViewModel.Id.Value, cancellationToken);
        }

        HasChanges = false;
        await _router.Reload();
    }

    private async Task ExecuteAddImage(CancellationToken arg)
    {
        string[]? result = await _windowService.CreateOpenFileDialog().WithAllowMultiple().HavingFilter(f => f.WithBitmaps()).ShowAsync();
        if (result == null)
            return;

        foreach (string path in result)
        {
            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            if (stream.Length > ImageUploadRequest.MAX_FILE_SIZE)
            {
                await _windowService.ShowConfirmContentDialog("File too big", $"File {path} exceeds maximum file size of 10 MB", "Skip file", null);
                await stream.DisposeAsync();
                continue;
            }

            ImageUploadRequest request = new(stream, Path.GetFileName(path), string.Empty);
            ImageSubmissionViewModel viewModel = _getImageSubmissionViewModel(request);

            // Show the dialog to give the image a name and description
            if (await viewModel.Edit() != ContentDialogResult.Primary)
            {
                await stream.DisposeAsync();
                continue;
            }

            AddImageViewModel(viewModel);
        }
    }

    private void InputChanged(object? sender, EventArgs e)
    {
        UpdateHasChanges();
    }
}