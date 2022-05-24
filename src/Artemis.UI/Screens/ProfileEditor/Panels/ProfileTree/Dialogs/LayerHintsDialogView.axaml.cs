using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;

public partial class LayerHintsDialogView : ReactiveCoreWindow<LayerHintsDialogViewModel>
{
    public LayerHintsDialogView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}