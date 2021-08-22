using System.ComponentModel;
using Artemis.VisualScripting.Nodes.CustomViewModels;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color.CustomViewModels
{
    public class StaticSKColorValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticSKColorValueNode _node;

        public StaticSKColorValueNodeCustomViewModel(StaticSKColorValueNode node) : base(node)
        {
            _node = node;
        }

        public SKColor Input
        {
            get => (SKColor) (_node.Storage ?? SKColor.Empty);
            set => _node.Storage = value;
        }

        public override void OnActivate()
        {
            _node.PropertyChanged += NodeOnPropertyChanged;
        }

        public override void OnDeactivate()
        {
            _node.PropertyChanged -= NodeOnPropertyChanged;
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
        }
    }
}