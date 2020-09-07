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

        public void ShowTimeInDataModel()
        {
            GeneralModule.ShowProperty(model => model.TimeDataModel.CurrentTime);
        }

        public void HideTimeInDataModel()
        {
            GeneralModule.HideProperty(model => model.TimeDataModel.CurrentTime);
        }
    }
}