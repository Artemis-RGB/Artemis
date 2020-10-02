using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.TestData.DataModels
{
    public class DynamicDataModel : DataModel
    {
        public DynamicDataModel()
        {
            DynamicString = "Test 123";
        }

        [DataModelProperty(Description = "Descriptionnnnnn")]
        public string DynamicString { get; set; }
    }
}