using System.Collections.ObjectModel;
using System.Linq;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public class EntryImagesViewModel : ViewModelBase
{
    public ObservableCollection<EntryImageViewModel> Images { get; }

    public EntryImagesViewModel(IEntryDetails entryDetails)
    {
        Images = new ObservableCollection<EntryImageViewModel>(entryDetails.Images.Select(i => new EntryImageViewModel(i)));
    }
}