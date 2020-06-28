using System.Timers;
using Artemis.UI.DataModelVisualization;
using Artemis.UI.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class DataModelDebugViewModel : Screen
    {
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly Timer _updateTimer;

        public DataModelDebugViewModel(IDataModelVisualizationService dataModelVisualizationService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += (sender, args) => MainDataModel.Update();

            DisplayName = "Data model";
        }

        public DataModelViewModel MainDataModel { get; set; }

        protected override void OnActivate()
        {
            MainDataModel = _dataModelVisualizationService.GetMainDataModelVisualization();
            _updateTimer.Start();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
        }
    }
}