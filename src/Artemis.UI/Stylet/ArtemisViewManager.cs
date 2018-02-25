using System.IO;
using System.Windows;
using System.Windows.Markup;
using Artemis.Plugins.Interfaces;
using Stylet;

namespace Artemis.UI.Stylet
{
    public class ArtemisViewManager : ViewManager
    {
        public ArtemisViewManager(ViewManagerConfig config) : base(config)
        {
        }

        public override UIElement CreateViewForModel(object model)
        {
            if (model is IModuleViewModel)
                return CreateViewForPlugin(model);

            return base.CreateViewForModel(model);
        }

        private UIElement CreateViewForPlugin(object model)
        {
            var pluginInfo = ((IModuleViewModel) model).PluginInfo;
            var viewPath = pluginInfo.Folder + pluginInfo.ViewModel.Replace("ViewModel", "View").Replace(".cs", ".xaml");
            // There doesn't have to be a view so make sure one exists
            if (!File.Exists(viewPath))
                return null;

            // Compile the view if found, must be done on UI thread sadly
            object view = null;
            Execute.OnUIThread(() => view = XamlReader.Parse(File.ReadAllText(viewPath)));

            return (UIElement) view;
        }
    }
}