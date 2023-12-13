using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public class EntryImagesViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    public ObservableCollection<EntryImageViewModel> Images { get; }

    public EntryImagesViewModel(IEntryDetails entryDetails, IWindowService windowService)
    {
        _windowService = windowService;
        Images = new ObservableCollection<EntryImageViewModel>(entryDetails.Images.Select(i => new EntryImageViewModel(i)));
    }

    public async Task ShowImages(EntryImageViewModel image)
    {
        await _windowService.CreateContentDialog()
            .WithViewModel(out EntryImagesDialogViewModel vm, Images, image)
            .HavingPrimaryButton(b => b.WithText("Previous").WithCommand(vm.Previous))
            .HavingSecondaryButton(b => b.WithText("Next").WithCommand(vm.Next))
            .WithFullScreen()
            .ShowAsync();
    }
}