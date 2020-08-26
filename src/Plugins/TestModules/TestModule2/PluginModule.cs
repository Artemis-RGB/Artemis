using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Modules;
using SkiaSharp;




namespace TestModule2
{
    // The core of your module. Hover over the method names to see a description.
    public class PluginModule : ProfileModule
    {
        // This is the beginning of your plugin life cycle. Use this instead of a constructor.
        public override void EnablePlugin()
        {
            DisplayName = "TestModule2";
            DisplayIcon = "ToyBrickPlus";
            DefaultPriorityCategory = ModulePriorityCategory.Application;

        }

        // This is the end of your plugin life cycle.
        public override void DisablePlugin()
        {
            // Make sure to clean up resources where needed (dispose IDisposables etc.)
        }

        public override void ModuleActivated(bool isOverride)
        {
            // When this gets called your activation requirements have been met and the module will start displaying
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            // When this gets called your activation requirements are no longer met and your module will stop displaying
        }
    }
}