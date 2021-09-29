using System;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.DataModel
{
    [Node("Data Model-Value", "Outputs a selectable data model value.", "Data Model")]
    public class DataModelNode : Node<DataModelPathEntity, DataModelNodeCustomViewModel>, IDisposable
    {
        private DataModelPath _dataModelPath;

        public DataModelNode() : base("Data Model", "Outputs a selectable data model value")
        {
        }

        public INodeScript Script { get; private set; }
        public OutputPin Output { get; private set; }

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
            DataModelPath.PathValidated += DataModelPathOnPathValidated;

            UpdateOutputPin(false);
        }

        public override void Evaluate()
        {
            if (DataModelPath.IsValid)
            {
                if (Output == null)
                    UpdateOutputPin(false);

                object pathValue = DataModelPath.GetValue();

                if (pathValue == null)
                {
                    if (!Output.Type.IsValueType)
                        Output.Value = null;
                }
                else
                {
                    if (pathValue is double doublePathValue)
                        Output.Value = (float) doublePathValue;
                    else
                        Output.Value = pathValue;
                }
            }
        }

        public void UpdateOutputPin(bool loadConnections)
        {
            Type type = DataModelPath?.GetPropertyType();
            if (type == typeof(double))
                type = typeof(float);

            if (Output != null && Output.Type == type)
                return;

            if (Output != null)
            {
                RemovePin(Output);
                Output = null;
            }

            if (type != null) 
                Output = CreateOutputPin(type);

            if (loadConnections && Script is NodeScript nodeScript)
                nodeScript.LoadConnections();
        }

        private void DataModelPathOnPathValidated(object sender, EventArgs e)
        {
            UpdateOutputPin(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DataModelPath?.Dispose();
        }
    }
}