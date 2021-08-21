using System.ComponentModel;

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
            get => (double) (_node.Storage ?? 0.0);
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

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
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
            get => (float) (_node.Storage ?? 0f);
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

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
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
            get => _node.Storage is long longInput ? (int) longInput : (int) (_node.Storage ?? 0);
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

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
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
            get => (string) _node.Storage;
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

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
        }
    }
}