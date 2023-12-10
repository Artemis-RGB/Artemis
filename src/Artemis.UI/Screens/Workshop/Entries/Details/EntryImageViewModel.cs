using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public class EntryImageViewModel : ViewModelBase
{
    public EntryImageViewModel(IImage image)
    {
        Image = image;
        Url = $"{WorkshopConstants.WORKSHOP_URL}/images/{image.Id}.png";
        ThumbnailUrl = $"{WorkshopConstants.WORKSHOP_URL}/images/{image.Id}-thumb.png";
    }

    public IImage Image { get; }
    public string Url { get; }
    public string ThumbnailUrl { get; }
}