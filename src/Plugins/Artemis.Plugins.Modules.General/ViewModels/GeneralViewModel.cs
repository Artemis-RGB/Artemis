using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.General.ViewModels
{
    public class GeneralViewModel : ModuleViewModel
    {
        public GeneralViewModel(GeneralModule module) : base(module, "General")
        {
            GeneralModule = module;
        }

        public GeneralModule GeneralModule { get; }

        public void ShowUTCTimeInDataModel()
        {
            GeneralModule.ShowProperty(model => model.TimeDataModel.CurrentTimeUTC);
        }

        public void HideUTCTimeInDataModel()
        {
            GeneralModule.HideProperty(model => model.TimeDataModel.CurrentTimeUTC);
        }
    }
}