using System;
using System.Reactive.Linq;
using Artemis.Core;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree
{
    public partial class TreePropertyView : ReactiveUserControl<ITreePropertyViewModel>
    {
        public TreePropertyView()
        {
            this.WhenActivated(d =>
            {
                if (ViewModel != null)
                {
                    Observable.FromEventPattern<LayerPropertyEventArgs>(e => ViewModel.BaseLayerProperty.CurrentValueSet += e, e => ViewModel.BaseLayerProperty.CurrentValueSet -= e)
                        .Subscribe(_ => this.BringIntoView())
                        .DisposeWith(d);
                }
            });
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}