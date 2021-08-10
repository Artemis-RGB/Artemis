using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class DataModelNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly DataModelNode _node;

        public DataModelNodeCustomViewModel(DataModelNode node, IDataModelUIService dataModelUIService) : base(node)
        {
            _node = node;

            Execute.OnUIThreadSync(() =>
            {
                SelectionViewModel = dataModelUIService.GetDynamicSelectionViewModel(module: null);
                SelectionViewModel.PropertySelected += SelectionViewModelOnPropertySelected;
            });
        }

        public DataModelDynamicViewModel SelectionViewModel { get; set; }

        private void SelectionViewModelOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            _node.DataModelPath = SelectionViewModel.DataModelPath;
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