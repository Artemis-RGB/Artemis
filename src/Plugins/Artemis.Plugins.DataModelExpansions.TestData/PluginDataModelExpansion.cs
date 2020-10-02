using System;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.TestData.DataModels;
using SkiaSharp;

namespace Artemis.Plugins.DataModelExpansions.TestData
{
    public class PluginDataModelExpansion : DataModelExpansion<PluginDataModel>
    {
        private Random _rand;

        public override void EnablePlugin()
        {
            _rand = new Random();
            AddTimedUpdate(TimeSpan.FromSeconds(1), TimedUpdate);

            DataModel.AddDynamicChild(new DynamicDataModel(), "Dynamic1", "Dynamic data model 1");

            // var testPath1 = new DataModelPath(DataModel, "TemplateDataModelString");
            var testPath2 = new DataModelPath(DataModel, "PluginSubDataModel.Number");
            // var testPath3 = new DataModelPath(DataModel, "Dynamic1.DynamicString");
            // var testPath4 = new DataModelPath(DataModel, "TemplateDataModelString");
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            // You can access your data model here and update it however you like
            DataModel.TemplateDataModelString = $"The last delta time was {deltaTime} seconds";

            var dynamic = DataModel.DynamicChild<DynamicDataModel>("Dynamic1")?.DynamicString;
            var dynamic2 = DataModel.DynamicChild<DynamicDataModel>("Dynamic2")?.DynamicString;
        }

        private void TimedUpdate(double deltaTime)
        {
            DataModel.TestColorA = SKColor.FromHsv(_rand.Next(0, 360), 100, 100);
            DataModel.TestColorB = SKColor.FromHsv(_rand.Next(0, 360), 100, 100);
        }
    }
}