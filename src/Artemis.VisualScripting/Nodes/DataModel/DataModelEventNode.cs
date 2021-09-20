using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.DataModel
{
    [Node("Data Model-Event", "Responds to a data model event trigger", "External", OutputType = typeof(bool))]
    public class DataModelEventNode : Node<DataModelEventNodeCustomViewModel>, IDisposable
    {
        private DataModelPath _dataModelPath;
        private DateTime _lastTrigger;
        private int _currentIndex;

        public DataModelEventNode() : base("Data Model-Event", "Responds to a data model event trigger")
        {
            Input = CreateInputPin<object>();
            Input.PinConnected += InputOnPinConnected;
        }

        public InputPin<object> Input { get; }

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
            CreateCycleValues();
            CreateOutput();

            if (Storage is not DataModelPathEntity pathEntity)
                return;

            DataModelPath = new DataModelPath(pathEntity);
        }

        public override void Evaluate()
        {
            object outputValue = null;
            if (DataModelPath?.GetValue() is DataModelEvent dataModelEvent)
            {
                if (dataModelEvent.LastTrigger > _lastTrigger)
                {
                    _lastTrigger = dataModelEvent.LastTrigger;
                    _currentIndex++;

                    if (_currentIndex > CycleValues.Count())
                        _currentIndex = 0;
                }

                outputValue = _currentIndex == 0
                    ? Input.Value
                    : CycleValues.ElementAt(_currentIndex - 1).PinValue;
            }

            Output.Value = outputValue ?? Output.Type.GetDefault();
        }

        private void InputOnPinConnected(object sender, SingleValueEventArgs<IPin> e)
        {
            CreateCycleValues();
            CreateOutput();
        }

        private void CreateCycleValues()
        {
            int pinCount = CycleValues?.Count() ?? 1;
            Type inputType = Input.ConnectedTo.FirstOrDefault()?.Type ?? typeof(object);

            if (CycleValues != null)
            {
                if (inputType == CycleValues.Type)
                    return;
                RemovePinCollection(CycleValues);
            }

            CycleValues = CreateInputPinCollection(inputType, "", pinCount);
        }

        private void CreateOutput()
        {
            Type inputType = Input.ConnectedTo.FirstOrDefault()?.Type ?? typeof(object);
            
            if (Output != null)
            {
                if (inputType == Output.Type)
                    return;
                RemovePin(Output);
            }

            Output = CreateOutputPin(inputType);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}