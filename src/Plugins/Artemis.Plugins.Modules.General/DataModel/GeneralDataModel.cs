using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Plugins.Modules.General.DataModel.Windows;

namespace Artemis.Plugins.Modules.General.DataModel
{
    public class GeneralDataModel : Core.DataModelExpansions.DataModel
    {
        public GeneralDataModel()
        {
            TimeDataModel = new TimeDataModel();
            TestTimeList = new List<TimeDataModel>();

            var testExpression = new Func<object, object, bool>((leftItem, rightDataModel) =>
                ((TimeDataModel) leftItem).CurrentTime.Month == ((GeneralDataModel) rightDataModel).TimeDataModel.CurrentTime.Day);


            var test = TestTimeList.Any(model => testExpression(model, this));
        }

        public WindowDataModel ActiveWindow { get; set; }
        public TimeDataModel TimeDataModel { get; set; }

        public List<TimeDataModel> TestTimeList { get; set; }
    }

    public class TimeDataModel : Core.DataModelExpansions.DataModel
    {
        public DateTime CurrentTime { get; set; }
        public DateTime CurrentTimeUTC { get; set; }
    }
}