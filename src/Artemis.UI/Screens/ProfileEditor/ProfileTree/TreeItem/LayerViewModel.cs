using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        public LayerViewModel(ProfileElement layer,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory) :
            base(layer, profileEditorService, dialogService, profileTreeVmFactory)
        {
        }

        public Layer Layer => ProfileElement as Layer;
        public bool ShowIcons => Layer?.LayerBrush != null;
        public override bool SupportsChildren => false;
    }
}