using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public partial class StaticSKColorValueNodeCustomView : ReactiveUserControl<StaticSKColorValueNodeCustomViewModel>
{
    public StaticSKColorValueNodeCustomView()
    {
        InitializeComponent();
    }


    private void ColorPickerButton_OnFlyoutOpened(ColorPickerButton sender, EventArgs args)
    {
        ViewModel?.PauseUpdating();
    }

    private void ColorPickerButton_OnFlyoutClosed(ColorPickerButton sender, EventArgs args)
    {
        ViewModel?.ResumeUpdating();
    }
}