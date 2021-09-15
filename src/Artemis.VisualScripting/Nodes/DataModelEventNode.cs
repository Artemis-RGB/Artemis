using System;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Data Model-Event", "Responds to a data model event trigger", "External", OutputType = typeof(bool))]
    public class DataModelEventNode : Node<DataModelEventNodeCustomViewModel>, IDisposable
    {
        private DataModelPath _dataModelPath;

        public DataModelEventNode() : base("Data Model-Event", "Responds to a data model event trigger")
        {
            Output = CreateOutputPin<bool>();
        }

        public OutputPin<bool> Output { get; }
        public INodeScript Script { get; set; }

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

            DataModelPath = new DataModelPath(pathEntity);
            DataModelPath.PathValidated += DataModelPathOnPathValidated;
        }

        public override void Evaluate()
        {
        }

        private void DataModelPathOnPathValidated(object sender, EventArgs e)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}