using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.DataModel
{
    [Node("Data Model-Event", "Responds to a data model event trigger", "Data Model", OutputType = typeof(bool))]
    public class DataModelEventNode : Node<DataModelPathEntity, DataModelEventNodeCustomViewModel>, IDisposable
    {
        private DataModelPath _dataModelPath;
        private DateTime _lastTrigger;
        private int _currentIndex;
        private Type _currentType;
        private bool _updating;

        public DataModelEventNode() : base("Data Model-Event", "Responds to a data model event trigger")
        {
            _currentType = typeof(object);
            CreateCycleValues(typeof(object), 1);
            Output = CreateOutputPin(typeof(object));
        }

        private void CreateCycleValues(Type type, int initialCount)
        {
            if (CycleValues != null)
            {
                CycleValues.PinAdded -= CycleValuesOnPinAdded;
                CycleValues.PinRemoved -= CycleValuesOnPinRemoved;
                foreach (IPin pin in CycleValues)
                {
                    pin.PinConnected -= OnPinConnected;
                    pin.PinDisconnected -= OnPinDisconnected;
                }

                RemovePinCollection(CycleValues);
            }

            CycleValues = CreateInputPinCollection(type, "", initialCount);
            CycleValues.PinAdded += CycleValuesOnPinAdded;
            CycleValues.PinRemoved += CycleValuesOnPinRemoved;
            foreach (IPin pin in CycleValues)
            {
                pin.PinConnected += OnPinConnected;
                pin.PinDisconnected += OnPinDisconnected;
            }
        }

        private void CycleValuesOnPinAdded(object sender, SingleValueEventArgs<IPin> e)
        {
            e.Value.PinConnected += OnPinConnected;
            e.Value.PinDisconnected += OnPinDisconnected;
        }

        private void CycleValuesOnPinRemoved(object sender, SingleValueEventArgs<IPin> e)
        {
            e.Value.PinConnected -= OnPinConnected;
            e.Value.PinDisconnected -= OnPinDisconnected;
        }

        private void OnPinDisconnected(object sender, SingleValueEventArgs<IPin> e)
        {
            ProcessPinDisconnected();
        }

        private void OnPinConnected(object sender, SingleValueEventArgs<IPin> e)
        {
            IPin source = e.Value;
            IPin target = (IPin) sender;
            ProcessPinConnected(source, target);
        }

        public InputPinCollection CycleValues { get; set; }
        public OutputPin Output { get; set; }
        public INodeScript Script { get; set; }

        public DataModelPath DataModelPath
        {
            get => _dataModelPath;
            set => SetAndNotify(ref _dataModelPath, value);
        }

        public override void Initialize(INodeScript script)
        {
            Script = script;

            if (Storage == null)
                return;

            DataModelPath = new DataModelPath(Storage);
        }

        public override void Evaluate()
        {
            object outputValue = null;
            if (DataModelPath?.GetValue() is IDataModelEvent dataModelEvent)
            {
                if (dataModelEvent.LastTrigger > _lastTrigger)
                {
                    _lastTrigger = dataModelEvent.LastTrigger;
                    _currentIndex++;

                    if (_currentIndex >= CycleValues.Count())
                        _currentIndex = 0;
                }

                outputValue = CycleValues.ElementAt(_currentIndex).PinValue;
            }

            if (Output.Type.IsInstanceOfType(outputValue))
                Output.Value = outputValue;
            else if (Output.Type.IsValueType)
                Output.Value = Output.Type.GetDefault()!;
        }

        private void ProcessPinConnected(IPin source, IPin target)
        {
            if (_updating)
                return;

            try
            {
                _updating = true;

                // No need to change anything if the types haven't changed
                if (_currentType == source.Type)
                    return;

                int reconnectIndex = CycleValues.ToList().IndexOf(target);
                ChangeCurrentType(source.Type);

                if (reconnectIndex != -1)
                {
                    CycleValues.ToList()[reconnectIndex].ConnectTo(source);
                    source.ConnectTo(CycleValues.ToList()[reconnectIndex]);
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private void ChangeCurrentType(Type type)
        {
            CreateCycleValues(type, CycleValues.Count());

            List<IPin> oldOutputConnections = Output.ConnectedTo.ToList();
            RemovePin(Output);
            Output = CreateOutputPin(type);
            foreach (IPin oldOutputConnection in oldOutputConnections.Where(o => o.Type.IsAssignableFrom(Output.Type)))
            {
                oldOutputConnection.DisconnectAll();
                oldOutputConnection.ConnectTo(Output);
                Output.ConnectTo(oldOutputConnection);
            }

            _currentType = type;
        }

        private void ProcessPinDisconnected()
        {
            if (_updating)
                return;
            try
            {
                // If there's still a connected pin, stick to the current type
                if (CycleValues.Any(v => v.ConnectedTo.Any()))
                    return;

                ChangeCurrentType(typeof(object));
            }
            finally
            {
                _updating = false;
            }
        }

        private void UpdatePinsType(IPin source)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}