using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Modules;
using SkiaSharp;




namespace TestModule1
{
    // The core of your module. Hover over the method names to see a description.
    public class PluginModule : ProfileModule
    {
        // This is the beginning of your plugin life cycle. Use this instead of a constructor.
        public override void EnablePlugin()
        {
            DisplayName = "TestModule1";
            DisplayIcon = "ToyBrickPlus";
            DefaultPriorityCategory = ModulePriorityCategory.Normal;

        }

        // This is the end of your plugin life cycle.
        public override void DisablePlugin()
        {
            // Make sure to clean up resources where needed (dispose IDisposables etc.)
        }


        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
        }
    }
}