using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class DataModelNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly DataModelNode _node;

        public DataModelNodeCustomViewModel(DataModelNode node) : base(node)
        {
            _node = node;
        }

        public DataModelPath DataModelPath
        {
            get => _node.DataModelPath;
            set
            {
                _node.DataModelPath = value;
                OnPropertyChanged(nameof(DataModelPath));

                if (_node.DataModelPath != null)
                {
                    _node.DataModelPath.Save();
                    _node.Storage = _node.DataModelPath.Entity;
                }
                else
                {
                    _node.Storage = null;
                }

                _node.UpdateOutputPin();
            }
        }
    }
}