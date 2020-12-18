using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides extensions for special data model wrappers used by events and list conditions
    /// </summary>
    public static class DataModelWrapperExtensions
    {
        /// <summary>
        ///     Creates a  view model for a <see cref="EventPredicateWrapperDataModel" />
        /// </summary>
        /// <param name="wrapper">The wrapper to create the view model for</param>
        /// <param name="dataModelUIService">The data model UI service to be used by the view model</param>
        /// <param name="configuration">The update configuration to be used by the view model</param>
        /// <returns>The created view model</returns>
        public static DataModelPropertiesViewModel CreateViewModel(this EventPredicateWrapperDataModel wrapper, IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            DataModelPropertiesViewModel viewModel = new(wrapper, null, new DataModelPath(wrapper));
            viewModel.Update(dataModelUIService, configuration);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(dataModelUIService, configuration);
            viewModel.Children.First().IsVisualizationExpanded = true;

            return viewModel;
        }

        /// <summary>
        ///     Creates a  view model for a <see cref="ListPredicateWrapperDataModel" />
        /// </summary>
        /// <param name="wrapper">The wrapper to create the view model for</param>
        /// <param name="dataModelUIService">The data model UI service to be used by the view model</param>
        /// <param name="configuration">The update configuration to be used by the view model</param>
        /// <returns>The created view model</returns>
        public static DataModelPropertiesViewModel CreateViewModel(this ListPredicateWrapperDataModel wrapper, IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            DataModelPropertiesViewModel viewModel = new(wrapper, null, new DataModelPath(wrapper));
            viewModel.Update(dataModelUIService, configuration);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(dataModelUIService, configuration);
            viewModel.Children.First().IsVisualizationExpanded = true;

            return viewModel;
        }
    }
}