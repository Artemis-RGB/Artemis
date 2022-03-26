using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
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
            ViewModel?.WhenAnyValue(vm => vm.IsVisible).Where(visible => !visible).Subscribe(_ => this.FindLogicalAncestorOfType<Grid>()?.ContextFlyout?.Hide()).DisposeWith(d);
            this.Get<TextBox>("SearchBox").SelectAll();
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}