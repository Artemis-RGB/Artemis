using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public static class DataModelWrapperExtensions
    {
        public static DataModelPropertiesViewModel CreateViewModel(this EventPredicateWrapperDataModel wrapper, IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            DataModelPropertiesViewModel viewModel = new DataModelPropertiesViewModel(wrapper, null, new DataModelPath(wrapper));
            viewModel.Update(dataModelUIService, configuration);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(dataModelUIService, configuration);
            viewModel.Children.First().IsVisualizationExpanded = true;

            return viewModel;
        }

        public static DataModelPropertiesViewModel CreateViewModel(this ListPredicateWrapperDataModel wrapper, IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            DataModelPropertiesViewModel viewModel = new DataModelPropertiesViewModel(wrapper, null, new DataModelPath(wrapper));
            viewModel.Update(dataModelUIService, configuration);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(dataModelUIService, configuration);
            viewModel.Children.First().IsVisualizationExpanded = true;

            return viewModel;
        }
    }
}