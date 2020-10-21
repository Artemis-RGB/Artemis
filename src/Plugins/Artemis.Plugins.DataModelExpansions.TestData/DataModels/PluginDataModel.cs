using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using SkiaSharp;

namespace Artemis.Plugins.DataModelExpansions.TestData.DataModels
{
    public class PluginDataModel : DataModel
    {
        public PluginDataModel()
        {
            PluginSubDataModel = new PluginSubDataModel();
            ListItems = new List<SomeListItem>();
            for (int i = 0; i < 20; i++)
                ListItems.Add(new SomeListItem {ItemName = $"Item {i + 1}", Number = i});

            GenericTest = new Test<string> {Value = "Generic string value"};
        }

        // Your datamodel can have regular properties and you can annotate them if you'd like
        [DataModelProperty(Name = "A test string", Description = "It doesn't do much, but it's there.")]
        public string TemplateDataModelString { get; set; }

        public int? NullableInt { get; set; }
        public Test<string> GenericTest { get; set; }

        public SKColor TestColorA { get; set; }
        public SKColor TestColorB { get; set; }

        // You can even have classes in your datamodel, just don't forget to instantiate them ;)
        [DataModelProperty(Name = "A class within the datamodel")]
        public PluginSubDataModel PluginSubDataModel { get; set; }

        public Team Team { get; set; }
        public bool IsWinning { get; set; }

        public List<SomeListItem> ListItems { get; set; }

        [DataModelProperty(Description = "Event without arguments")]
        public DataModelEvent Event1 { get; set; } = new DataModelEvent();

        [DataModelProperty(Description = "Event with arguments")]
        public DataModelEvent<TestEventArgs> Event2 { get; set; } = new DataModelEvent<TestEventArgs>();
    }

    public class TestEventArgs : DataModelEventArgs
    {
        public TestEventArgs(string someValue)
        {
            SomeValue = someValue;
        }

        public string SomeValue { get; set; }
    }

    public class Test<T>
    {
        public T Value { get; set; }
    }

    public class SomeListItem
    {
        public string ItemName { get; set; }
        public int Number { get; set; }
    }

    public enum Team
    {
        Blue,
        Orange
    }

    public class PluginSubDataModel
    {
        public PluginSubDataModel()
        {
            ListOfInts = new List<int> {1, 2, 3, 4, 5};
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