using System;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.General.DataModels.Windows;

namespace Artemis.Plugins.Modules.General.DataModels
{
    public class GeneralDataModel : DataModel
    {
        public GeneralDataModel()
        {
            TimeDataModel = new TimeDataModel();
        }

        public WindowDataModel ActiveWindow { get; set; }
        public TimeDataModel TimeDataModel { get; set; }
    }

    public class TimeDataModel : DataModel
    {
        public DateTimeOffset CurrentTime { get; set; }
        public TimeSpan TimeSinceMidnight { get; set; }
    }
}