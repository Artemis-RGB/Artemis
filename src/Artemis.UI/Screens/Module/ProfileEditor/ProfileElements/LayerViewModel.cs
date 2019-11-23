using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements
{
    public class LayerViewModel : ProfileElementViewModel
    {
        public LayerViewModel(Layer layer, FolderViewModel parent)
        {
            Layer = layer;
            Parent = parent;
            ProfileElement = layer;
        }

        public Layer Layer { get; }
    }
}