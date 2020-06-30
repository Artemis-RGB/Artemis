using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.DataModelVisualization;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Services
{
    public class DataModelVisualizationService : IDataModelVisualizationService
    {
        private readonly IDataModelService _dataModelService;

        public DataModelVisualizationService(IDataModelService dataModelService)
        {
            _dataModelService = dataModelService;
        }

        public DataModelViewModel GetMainDataModelVisualization()
        {
            var viewModel = new DataModelViewModel();
            foreach (var dataModelExpansion in _dataModelService.DataModelExpansions)
                viewModel.Children.Add(new DataModelViewModel(null, dataModelExpansion, dataModelExpansion.DataModelDescription, viewModel));

            return viewModel;
        }

        public DataModelViewModel GetPluginDataModelVisualization(Plugin plugin)
        {
            var dataModel = _dataModelService.GetPluginDataModel(plugin);
            if (dataModel == null)
                return null;

            var viewModel = new DataModelViewModel();
            viewModel.Children.Add(new DataModelViewModel(null, dataModel, dataModel.DataModelDescription, viewModel));
            return viewModel;
        }
    }

    public interface IDataModelVisualizationService : IArtemisUIService
    {
        DataModelViewModel GetMainDataModelVisualization();
        DataModelViewModel GetPluginDataModelVisualization(Plugin plugin);
    }
}