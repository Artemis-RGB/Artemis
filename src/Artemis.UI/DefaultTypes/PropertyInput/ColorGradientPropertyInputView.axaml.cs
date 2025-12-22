using System;
using Artemis.UI.Shared.Controls.GradientPicker;
using ReactiveUI.Avalonia;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class ColorGradientPropertyInputView : ReactiveUserControl<ColorGradientPropertyInputViewModel>
{
    public ColorGradientPropertyInputView()
    {
        InitializeComponent();
    }


    private void GradientPickerButton_OnFlyoutOpened(GradientPickerButton sender, EventArgs args)
    {
        ViewModel?.StartPreview();
    }

    private void GradientPickerButton_OnFlyoutClosed(GradientPickerButton sender, EventArgs args)
    {
        ViewModel?.ApplyPreview();
    }
}