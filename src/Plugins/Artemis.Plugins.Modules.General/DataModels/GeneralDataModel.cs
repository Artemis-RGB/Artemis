using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.General.DataModels.Windows;

namespace Artemis.Plugins.Modules.General.DataModels
{
    public class GeneralDataModel : DataModel
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

    public class TimeDataModel : DataModel
    {
        public DateTime CurrentTime { get; set; }
        public DateTime CurrentTimeUTC { get; set; }
    }
}