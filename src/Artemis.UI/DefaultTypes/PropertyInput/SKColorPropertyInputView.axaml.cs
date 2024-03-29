using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class SKColorPropertyInputView : ReactiveUserControl<SKColorPropertyInputViewModel>
{
    public SKColorPropertyInputView()
    {
        InitializeComponent();
    }


    private void ColorPickerButton_OnFlyoutOpened(ColorPickerButton sender, EventArgs args)
    {
        ViewModel?.StartPreview();
    }

    private void ColorPickerButton_OnFlyoutClosed(ColorPickerButton sender, EventArgs args)
    {
        ViewModel?.ApplyPreview();
    }
}