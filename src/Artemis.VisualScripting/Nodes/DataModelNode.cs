using System;
using System.Linq;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.CustomViewModels;
using Stylet;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Data Model-Value", "Outputs a selectable data model value.")]
    public class DataModelNode : Node<DataModelNodeCustomViewModel>, IDisposable
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

            if (Storage is not DataModelPathEntity pathEntity)
                return;

            DataModelPath = new DataModelPath(null, pathEntity);
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
                if (pathValue != null)
                    Output.Value = pathValue;
            }
        }

        public void UpdateOutputPin(bool loadConnections)
        {
            Execute.OnUIThread(() =>
            {
                if (Output != null && Output.Type == DataModelPath?.GetPropertyType())
                    return;

                if (Output != null)
                {
                    RemovePin(Output);
                    Output = null;
                }

                Type type = DataModelPath?.GetPropertyType();
                if (type != null)
                    Output = CreateOutputPin(type);

                if (loadConnections && Script is NodeScript nodeScript) 
                    nodeScript.LoadConnections();
            });
        }

        private void DataModelPathOnPathValidated(object sender, EventArgs e)
        {
            UpdateOutputPin(true);
           
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            DataModelPath.Dispose();
        }

        #endregion
    }
}