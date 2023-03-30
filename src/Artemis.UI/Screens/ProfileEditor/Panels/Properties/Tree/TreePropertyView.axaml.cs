using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

public partial class TreePropertyView : ReactiveUserControl<ITreePropertyViewModel>
{
    public TreePropertyView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<LayerPropertyEventArgs>(e => ViewModel!.BaseLayerProperty.CurrentValueSet += e, e => ViewModel!.BaseLayerProperty.CurrentValueSet -= e)
                .Subscribe(_ => this.BringIntoView())
                .DisposeWith(d);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DataBindingToggleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ToggleCurrentLayerProperty();
        DataBindingToggleButton.IsChecked = !DataBindingToggleButton.IsChecked;
    }
}