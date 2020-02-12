using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarViewModel : PropertyChangedBase
    {
        private readonly IModuleVmFactory _moduleVmFactory;
        private readonly IPluginService _pluginService;

        public SidebarViewModel(List<MainScreenViewModel> defaultSidebarItems, IModuleVmFactory moduleVmFactory, IPluginService pluginService)
        {
            _moduleVmFactory = moduleVmFactory;
            _pluginService = pluginService;

            DefaultSidebarItems = defaultSidebarItems;
            SidebarItemObjects = new Dictionary<INavigationItem, object>();
            SidebarItems = new BindableCollection<INavigationItem>();

            SetupSidebar();
            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;
        }

        public List<MainScreenViewModel> DefaultSidebarItems { get; set; }
        public BindableCollection<INavigationItem> SidebarItems { get; set; }
        public Dictionary<INavigationItem, object> SidebarItemObjects { get; set; }
        public IScreen SelectedItem { get; set; }

        public void SetupSidebar()
        {
            SidebarItems.Clear();
            SidebarItemObjects.Clear();

            // Add all default sidebar items
            SidebarItems.Add(new DividerNavigationItem());
            foreach (var screen in DefaultSidebarItems.OrderBy(d => d.DisplayOrder))
            {
                var sidebarItem = new FirstLevelNavigationItem {Icon = screen.DisplayIcon, Label = screen.DisplayName};
                SidebarItems.Add(sidebarItem);
                SidebarItemObjects.Add(sidebarItem, screen);
            }

            // Add all activated modules
            SidebarItems.Add(new DividerNavigationItem());
            SidebarItems.Add(new SubheaderNavigationItem {Subheader = "Modules"});
            var modules = _pluginService.GetPluginsOfType<Core.Plugins.Abstract.Module>().ToList();
            foreach (var module in modules)
                AddModule(module);

            // Select the top item, which will be one of the defaults
            SidebarItems[1].IsSelected = true;
            SelectedItem = (IScreen) SidebarItemObjects[SidebarItems[1]];
        }

        // ReSharper disable once UnusedMember.Global - Called by view
        public void SelectItem(WillSelectNavigationItemEventArgs args)
        {
            if (args.NavigationItemToSelect == null)
            {
                SelectedItem = null;
                return;
            }

            var sidebarItemObject = SidebarItemObjects[args.NavigationItemToSelect];
            // The default items are singleton screens, simply set it as the selected item
            if (sidebarItemObject is IScreen screen)
                SelectedItem = screen;
            // Modules have a VM that must be created, use a factory and set the result as the selected item
            else if (sidebarItemObject is Core.Plugins.Abstract.Module module)
                SelectedItem = _moduleVmFactory.Create(module);
        }

        public void AddModule(Core.Plugins.Abstract.Module module)
        {
            // Ensure the module is not already in the list
            if (SidebarItemObjects.Any(io => io.Value == module))
                return;

            // Icon is provided as string to avoid having to reference MaterialDesignThemes
            var parsedIcon = Enum.TryParse<PackIconKind>(module.DisplayIcon, true, out var iconEnum);
            if (parsedIcon == false)
                iconEnum = PackIconKind.QuestionMarkCircle;
            var sidebarItem = new FirstLevelNavigationItem {Icon = iconEnum, Label = module.DisplayName};
            SidebarItems.Add(sidebarItem);
            SidebarItemObjects.Add(sidebarItem, module);
        }

        public void RemoveModule(Core.Plugins.Abstract.Module module)
        {
            // If not in the list there's nothing to do
            if (SidebarItemObjects.All(io => io.Value != module))
                return;

            var existing = SidebarItemObjects.First(io => io.Value == module);
            SidebarItems.Remove(existing.Key);
            SidebarItemObjects.Remove(existing.Key);
        }

        #region Event handlers

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Core.Plugins.Abstract.Module module)
                AddModule(module);
        }

        private void PluginServiceOnPluginDisabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Core.Plugins.Abstract.Module module)
                RemoveModule(module);
        }

        #endregion
    }
}