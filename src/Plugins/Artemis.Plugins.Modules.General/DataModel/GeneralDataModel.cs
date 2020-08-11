using System;
using System.Collections.Generic;
using Artemis.Plugins.Modules.General.DataModel.Windows;

namespace Artemis.Plugins.Modules.General.DataModel
{
    public class GeneralDataModel : Core.Plugins.Abstract.DataModels.DataModel
    {
        public GeneralDataModel()
        {
            TimeDataModel = new TimeDataModel();
            TestTimeList = new List<TimeDataModel>();
        }

        public WindowDataModel ActiveWindow { get; set; }
        public TimeDataModel TimeDataModel { get; set; }

        public List<TimeDataModel> TestTimeList { get; set; }
    }

    public class TimeDataModel : Core.Plugins.Abstract.DataModels.DataModel
    {
        public DateTime CurrentTime { get; set; }
        public DateTime CurrentTimeUTC { get; set; }
    }
}