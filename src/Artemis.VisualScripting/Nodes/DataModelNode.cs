using System;
using System.Linq;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Data Model-Value", "Outputs a selectable data model value.")]
    public class DataModelNode : Node<DataModelNodeCustomViewModel>
    {
        public DataModelNode() : base("Data Model", "Outputs a selectable data model value")
        {
        }

        public OutputPin Output { get; private set; }
        public DataModelPath DataModelPath { get; set; }

        public override void Initialize()
        {
            if (Storage is not DataModelPathEntity pathEntity) 
                return;

            DataModelPath = new DataModelPath(null, pathEntity);
            CustomViewModel.SelectionViewModel.ChangeDataModelPath(DataModelPath);
            UpdateOutputPin();
        }
        
        public override void Evaluate()
        {
            if (DataModelPath.IsValid && Output != null)
                Output.Value = DataModelPath.GetValue()!;
        }

        public void UpdateOutputPin()
        {
            if (Pins.Contains(Output))
            {
                RemovePin(Output);
                Output = null;
            }

            Type type = DataModelPath?.GetPropertyType();
            if (type != null)
                Output = CreateOutputPin(type);
        }
    }
}