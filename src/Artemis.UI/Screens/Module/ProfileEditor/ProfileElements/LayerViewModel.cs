using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements
{
    public class LayerViewModel : ProfileElementViewModel
    {
        public LayerViewModel(FolderViewModel parent, Layer layer, ProfileEditorViewModel profileEditorViewModel) : base(parent, layer, profileEditorViewModel)
        {
            Layer = layer;
        }

        public Layer Layer { get; }
    }
}