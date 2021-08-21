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

    public class StaticFloatValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticFloatValueNode _node;

        public StaticFloatValueNodeCustomViewModel(StaticFloatValueNode node) : base(node)
        {
            _node = node;
        }

        public float Input
        {
            get => (float)_node.Storage;
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }

    public class StaticIntegerValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticIntegerValueNode _node;

        public StaticIntegerValueNodeCustomViewModel(StaticIntegerValueNode node) : base(node)
        {
            _node = node;
        }

        public int Input
        {
            get
            {
                if (_node.Storage is long longInput)
                    return (int)longInput;
                return (int)_node.Storage;
            }
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }

    public class StaticStringValueNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly StaticStringValueNode _node;

        public StaticStringValueNodeCustomViewModel(StaticStringValueNode node) : base(node)
        {
            _node = node;
        }

        public string Input
        {
            get => (string)_node.Storage;
            set
            {
                _node.Storage = value;
                OnPropertyChanged(nameof(Input));
            }
        }
    }
}