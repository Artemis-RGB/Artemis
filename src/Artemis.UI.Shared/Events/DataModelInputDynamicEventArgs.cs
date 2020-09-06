using System;

namespace Artemis.UI.Shared
{
    public class DataModelInputDynamicEventArgs : EventArgs
    {
        public DataModelVisualizationViewModel DataModelVisualizationViewModel { get; }

        public DataModelInputDynamicEventArgs(DataModelVisualizationViewModel dataModelVisualizationViewModel)
        {
            DataModelVisualizationViewModel = dataModelVisualizationViewModel;
        }
    }
}