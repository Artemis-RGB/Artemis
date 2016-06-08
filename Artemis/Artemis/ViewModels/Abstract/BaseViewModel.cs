using Caliburn.Micro;

namespace Artemis.ViewModels.Abstract
{
    public abstract class BaseViewModel : Conductor<IScreen>.Collection.OneActive
    {
    }
}