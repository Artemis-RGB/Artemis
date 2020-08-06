using Artemis.Core.Plugins.Abstract.ViewModels;

namespace Artemis.Plugins.Modules.General.ViewModels
{
    public class GeneralViewModel : ModuleViewModel
    {
        public GeneralModule GeneralModule { get; }

        public GeneralViewModel(GeneralModule module) : base(module, "General")
        {
            GeneralModule = module;
        }

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