using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;
using SkiaSharp;

namespace Artemis.Plugins.DataModelExpansions.TestData.DataModels
{
    public class PluginDataModel : DataModel
    {
        public PluginDataModel()
        {
            PluginSubDataModel = new PluginSubDataModel();
        }

        // Your datamodel can have regular properties and you can annotate them if you'd like
        [DataModelProperty(Name = "A test string", Description = "It doesn't do much, but it's there.")]
        public string TemplateDataModelString { get; set; }

        public SKColor TestColorA { get; set; }
        public SKColor TestColorB { get; set; }

        // You can even have classes in your datamodel, just don't forget to instantiate them ;)
        [DataModelProperty(Name = "A class within the datamodel")]
        public PluginSubDataModel PluginSubDataModel { get; set; }
    }

    public class PluginSubDataModel
    {
        public PluginSubDataModel()
        {
            ListOfInts = new List<int> { 1, 2, 3, 4, 5 };
        }

        // You don't need to annotate properties, they will still show up 
        public float FloatyFloat { get; set; }

        // You can even have a list!
        public List<int> ListOfInts { get; set; }

        // If you don't want a property to show up in the datamodel, annotate it with DataModelIgnore
        [DataModelIgnore]
        public string MyDarkestSecret { get; set; }
    }
}