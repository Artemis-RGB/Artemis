using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImagePropertiesDialogView : ReactiveUserControl<ImagePropertiesDialogViewModel>
{
    public ImagePropertiesDialogView()
    {
        InitializeComponent();
    }
}