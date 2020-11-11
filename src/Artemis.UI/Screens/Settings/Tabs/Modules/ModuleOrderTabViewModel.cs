using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using GongSolutions.Wpf.DragDrop;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Modules
{
    public class ModuleOrderTabViewModel : Screen, IDropTarget
    {
        private readonly IModuleService _moduleService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly DefaultDropHandler _defaultDropHandler;
        private readonly List<ModuleOrderModuleViewModel> _modules;

        public ModuleOrderTabViewModel(IPluginManagementService pluginManagementService, IModuleService moduleService)
        {
            DisplayName = "MODULE PRIORITY";

            _pluginManagementService = pluginManagementService;
            _moduleService = moduleService;
            _modules = new List<ModuleOrderModuleViewModel>(pluginManagementService.GetFeaturesOfType<Module>().Select(m => new ModuleOrderModuleViewModel(m)));
            _defaultDropHandler = new DefaultDropHandler();

            NormalModules = new BindableCollection<ModuleOrderModuleViewModel>();
            ApplicationModules = new BindableCollection<ModuleOrderModuleViewModel>();
            OverlayModules = new BindableCollection<ModuleOrderModuleViewModel>();

            Update();
        }

        public BindableCollection<ModuleOrderModuleViewModel> NormalModules { get; set; }
        public BindableCollection<ModuleOrderModuleViewModel> ApplicationModules { get; set; }
        public BindableCollection<ModuleOrderModuleViewModel> OverlayModules { get; set; }

        public void DragOver(IDropInfo dropInfo)
        {
            _defaultDropHandler.DragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem == dropInfo.Data)
                return;

            ModuleOrderModuleViewModel viewModel = (ModuleOrderModuleViewModel) dropInfo.Data;
            BindableCollection<ModuleOrderModuleViewModel> targetCollection = (BindableCollection<ModuleOrderModuleViewModel>) dropInfo.TargetCollection;
            int insertIndex = dropInfo.InsertIndex;

            ModulePriorityCategory category;
            if (targetCollection == NormalModules)
                category = ModulePriorityCategory.Normal;
            else if (targetCollection == ApplicationModules)
                category = ModulePriorityCategory.Application;
            else
                category = ModulePriorityCategory.Overlay;

            // If moving down, take the removal of ourselves into consideration with the insert index
            if (targetCollection.Contains(viewModel) && targetCollection.IndexOf(viewModel) < insertIndex)
                insertIndex--;

            _moduleService.UpdateModulePriority(viewModel.Module, category, insertIndex);

            Update();
        }

        public void Update()
        {
            NormalModules.Clear();
            NormalModules.AddRange(_modules.Where(m => m.Module.PriorityCategory == ModulePriorityCategory.Normal).OrderBy(m => m.Module.Priority));

            ApplicationModules.Clear();
            ApplicationModules.AddRange(_modules.Where(m => m.Module.PriorityCategory == ModulePriorityCategory.Application).OrderBy(m => m.Module.Priority));

            OverlayModules.Clear();
            OverlayModules.AddRange(_modules.Where(m => m.Module.PriorityCategory == ModulePriorityCategory.Overlay).OrderBy(m => m.Module.Priority));

            foreach (ModuleOrderModuleViewModel moduleOrderModuleViewModel in _modules)
                moduleOrderModuleViewModel.Update();
        }
    }
}