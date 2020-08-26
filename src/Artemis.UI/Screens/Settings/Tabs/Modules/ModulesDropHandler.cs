using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services.Interfaces;
using GongSolutions.Wpf.DragDrop;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Modules
{
    public class ModulesDropHandler : IDropTarget
    {
        private readonly IModuleService _moduleService;
        private readonly BindableCollection<Core.Plugins.Modules.Module> _normalModules;
        private readonly BindableCollection<Core.Plugins.Modules.Module> _applicationModules;
        private readonly BindableCollection<Core.Plugins.Modules.Module> _overlayModules;
        private DefaultDropHandler _defaultDropHandler;

        public ModulesDropHandler(IModuleService moduleService,
            BindableCollection<Core.Plugins.Modules.Module> normalModules,
            BindableCollection<Core.Plugins.Modules.Module> applicationModules,
            BindableCollection<Core.Plugins.Modules.Module> overlayModules)
        {
            _defaultDropHandler = new DefaultDropHandler();
            _moduleService = moduleService;
            _normalModules = normalModules;
            _applicationModules = applicationModules;
            _overlayModules = overlayModules;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            _defaultDropHandler.DragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            var module = (Core.Plugins.Modules.Module) dropInfo.Data;
            var target = (BindableCollection<Core.Plugins.Modules.Module>) dropInfo.TargetCollection;
            var insertIndex = dropInfo.InsertIndex;

            ModulePriorityCategory category;
            if (target == _applicationModules)
                category = ModulePriorityCategory.Application;
            else if (target == _normalModules)
                category = ModulePriorityCategory.Normal;
            else
                category = ModulePriorityCategory.Overlay;


            if (target.Contains(module))
            {
                target.Move(target.IndexOf(module), Math.Min(target.Count - 1, insertIndex));
                _moduleService.UpdateModulePriority(module, category, insertIndex);
            }
            else
            {
                if (module.PriorityCategory == ModulePriorityCategory.Application)
                    _applicationModules.Remove(module);
                else if (module.PriorityCategory == ModulePriorityCategory.Normal)
                    _normalModules.Remove(module);
                else if (module.PriorityCategory == ModulePriorityCategory.Overlay)
                    _overlayModules.Remove(module);

                _moduleService.UpdateModulePriority(module, category, insertIndex);

                if (module.PriorityCategory == ModulePriorityCategory.Application)
                    _applicationModules.Insert(insertIndex, module);
                else if (module.PriorityCategory == ModulePriorityCategory.Normal)
                    _normalModules.Insert(insertIndex, module);
                else if (module.PriorityCategory == ModulePriorityCategory.Overlay)
                    _overlayModules.Insert(insertIndex, module);
            }
        }
    }
}