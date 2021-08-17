using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticIntegerValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticIntegerValueNode _node;

        public StaticIntegerValueNodeCustomViewModel(StaticIntegerValueNode node) : base(node)
        {
            _node = node;
        }

        public int Input
        {
            get => (int) _node.Storage;
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }
}