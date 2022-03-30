using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticSKColorValueNodeCustomView : ReactiveUserControl<StaticSKColorValueNodeCustomViewModel>
{
    public StaticSKColorValueNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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