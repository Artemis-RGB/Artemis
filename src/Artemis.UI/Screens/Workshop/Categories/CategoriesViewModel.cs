using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Categories;

public class CategoriesViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopClient _client;

    public CategoriesViewModel(IWorkshopClient client)
    {
        _client = client;
    }
}