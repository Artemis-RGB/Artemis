using System;
using Artemis.UI.Shared.Controls.GradientPicker;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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