using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticStringValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticStringValueNode _node;

        public StaticStringValueNodeCustomViewModel(StaticStringValueNode node) : base(node)
        {
            _node = node;
        }

        public string Input
        {
            get => (string) _node.Storage;
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }
}