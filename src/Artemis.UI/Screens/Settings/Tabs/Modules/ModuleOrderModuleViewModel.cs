using System;
using Artemis.Core.Modules;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Modules
{
    public class ModuleOrderModuleViewModel : PropertyChangedBase
    {
        private string _priority;

        public ModuleOrderModuleViewModel(Module module)
        {
            Module = module;
            Update();
        }

        public Module Module { get; }

        public string Priority
        {
            get => _priority;
            set => SetAndNotify(ref _priority, value);
        }

        public void Update()
        {
            switch (Module.PriorityCategory)
            {
                case ModulePriorityCategory.Normal:
                    Priority = "3." + (Module.Priority + 1);
                    break;
                case ModulePriorityCategory.Application:
                    Priority = "2." + (Module.Priority + 1);
                    break;
                case ModulePriorityCategory.Overlay:
                    Priority = "1." + (Module.Priority + 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}