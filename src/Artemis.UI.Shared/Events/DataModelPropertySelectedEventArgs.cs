using System;

namespace Artemis.UI.Shared
{
    public class DataModelPropertySelectedEventArgs : EventArgs
    {
        public DataModelVisualizationViewModel DataModelVisualizationViewModel { get; }

        public DataModelPropertySelectedEventArgs(DataModelVisualizationViewModel dataModelVisualizationViewModel)
        {
            DataModelVisualizationViewModel = dataModelVisualizationViewModel;
        }
    }
}