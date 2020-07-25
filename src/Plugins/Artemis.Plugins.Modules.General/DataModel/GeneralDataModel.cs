using Artemis.Plugins.Modules.General.DataModel.Windows;

namespace Artemis.Plugins.Modules.General.DataModel
{
    public class GeneralDataModel : Core.Plugins.Abstract.DataModels.DataModel
    {
        public WindowDataModel ActiveWindow { get; set; }
    }
}