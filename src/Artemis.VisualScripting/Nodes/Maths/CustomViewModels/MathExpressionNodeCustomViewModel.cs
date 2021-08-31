using System.ComponentModel;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.Maths.CustomViewModels
{
    public class MathExpressionNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly MathExpressionNode _node;

        public MathExpressionNodeCustomViewModel(MathExpressionNode node) : base(node)
        {
            _node = node;
        }

        public string Input
        {
            get => (string)_node.Storage;
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