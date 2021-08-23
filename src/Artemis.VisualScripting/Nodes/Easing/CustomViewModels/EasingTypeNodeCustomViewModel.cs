using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;
using Stylet;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViewModels
{
    public class EasingTypeNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly EasingTypeNode _node;
        private NodeEasingViewModel _selectedEasingViewModel;

        public EasingTypeNodeCustomViewModel(EasingTypeNode node) : base(node)
        {
            _node = node;
            EasingViewModels = new BindableCollection<NodeEasingViewModel>(Enum.GetValues(typeof(Easings.Functions))
                .Cast<Easings.Functions>()
                .Select(e => new NodeEasingViewModel(e)));
        }

        public BindableCollection<NodeEasingViewModel> EasingViewModels { get; }

        public NodeEasingViewModel SelectedEasingViewModel
        {
            get => _selectedEasingViewModel;
            set
            {
                _selectedEasingViewModel = value;
                _node.Storage = _selectedEasingViewModel.EasingFunction;
            }
        }

        public override void OnActivate()
        {
            _node.PropertyChanged += NodeOnPropertyChanged;
            SelectedEasingViewModel = EasingViewModels.FirstOrDefault(vm => vm.EasingFunction == _node.EasingFunction);
        }

        public override void OnDeactivate()
        {
            _node.PropertyChanged -= NodeOnPropertyChanged;
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
            {
                _selectedEasingViewModel = EasingViewModels.FirstOrDefault(vm => vm.EasingFunction == _node.EasingFunction);
                NotifyOfPropertyChange(nameof(SelectedEasingViewModel));
            }
        }
    }
}