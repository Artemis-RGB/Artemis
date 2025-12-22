using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

public partial class TreePropertyView : ReactiveUserControl<ITreePropertyViewModel>
{
    public TreePropertyView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ITreePropertyViewModel? viewModel = ViewModel;
            if (viewModel != null)
            {
                Observable.FromEventPattern<LayerPropertyEventArgs>(e => viewModel.BaseLayerProperty.CurrentValueSet += e, e => viewModel.BaseLayerProperty.CurrentValueSet -= e)
                    .Subscribe(_ => this.BringIntoView())
                    .DisposeWith(d);
            }
        });
    }


    private void DataBindingToggleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ToggleCurrentLayerProperty();
        DataBindingToggleButton.IsChecked = !DataBindingToggleButton.IsChecked;
    }
}