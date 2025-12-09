using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Profile.ProfileEditor.ProfileTree.Dialogs;

public partial class LayerHintsDialogView : ReactiveAppWindow<LayerHintsDialogViewModel>
{
    public LayerHintsDialogView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

}