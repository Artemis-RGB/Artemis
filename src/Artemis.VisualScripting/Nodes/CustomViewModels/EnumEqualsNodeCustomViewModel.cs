using System;
using System.ComponentModel;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared;
using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class EnumEqualsNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly EnumEqualsNode _node;

        public EnumEqualsNodeCustomViewModel(EnumEqualsNode node) : base(node)
        {
            _node = node;
        }

        public Enum Input
        {
            get => _node.Storage as Enum;
            set => _node.Storage = value;
        }

        public BindableCollection<ValueDescription> EnumValues { get; } = new();

        public override void OnActivate()
        {
            _node.InputPin.PinConnected += InputPinOnPinConnected;
            _node.InputPin.PinDisconnected += InputPinOnPinDisconnected;
            _node.PropertyChanged += NodeOnPropertyChanged;

            if (_node.InputPin.Value != null && _node.InputPin.Value.GetType().IsEnum)
                EnumValues.AddRange(EnumUtilities.GetAllValuesAndDescriptions(_node.InputPin.Value.GetType()));
            base.OnActivate();
        }

        public override void OnDeactivate()
        {
            _node.InputPin.PinConnected -= InputPinOnPinConnected;
            _node.InputPin.PinDisconnected -= InputPinOnPinDisconnected;
            _node.PropertyChanged -= NodeOnPropertyChanged;

            base.OnDeactivate();
        }

        private void InputPinOnPinDisconnected(object sender, SingleValueEventArgs<IPin> e)
        {
            EnumValues.Clear();
        }

        private void InputPinOnPinConnected(object sender, SingleValueEventArgs<IPin> e)
        {
            EnumValues.Clear();
            if (_node.InputPin.Value != null && _node.InputPin.Value.GetType().IsEnum)
                EnumValues.AddRange(EnumUtilities.GetAllValuesAndDescriptions(_node.InputPin.Value.GetType()));
        }

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Node.Storage))
                OnPropertyChanged(nameof(Input));
        }
    }
}