using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.PanAndZoom;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodePickerView : ReactiveUserControl<NodePickerViewModel>
{
    public NodePickerView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel?.WhenAnyValue(vm => vm.IsVisible).Where(visible => visible == false).Subscribe(_ => this.FindLogicalAncestorOfType<ZoomBorder>()?.ContextFlyout?.Hide()).DisposeWith(d);
            this.Get<TextBox>("SearchBox").SelectAll();
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}