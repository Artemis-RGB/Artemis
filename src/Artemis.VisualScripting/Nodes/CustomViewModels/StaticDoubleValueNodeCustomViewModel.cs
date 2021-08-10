using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticDoubleValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticDoubleValueNode _node;

        public StaticDoubleValueNodeCustomViewModel(StaticDoubleValueNode node) : base(node)
        {
            _node = node;
        }

        public double Input
        {
            get => (double) _node.Storage;
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }
}