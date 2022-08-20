using System;
using System.Reactive.Linq;
using Artemis.Core;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

public class TreePropertyView : ReactiveUserControl<ITreePropertyViewModel>
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
        this.Find<ToggleButton>("DataBindingToggleButton").IsChecked = !this.Find<ToggleButton>("DataBindingToggleButton").IsChecked;
    }
}