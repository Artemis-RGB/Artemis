using System;
using System.Reactive.Linq;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
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

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not IDataContextProvider {DataContext: NodeData nodeData} || ViewModel == null)
            return;

        ViewModel.CreateNode(nodeData);
        ViewModel.IsVisible = false;
    }
}