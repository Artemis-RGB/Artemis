using System.IO;
using System.Windows;
using System.Windows.Markup;
using Artemis.Core.Exceptions;
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
            if (model is IPluginViewModel)
                return CreateViewForPlugin(model);

            return base.CreateViewForModel(model);
        }

        private UIElement CreateViewForPlugin(object model)
        {
            var pluginInfo = ((IPluginViewModel) model).PluginInfo;
            var viewPath = pluginInfo.Folder + pluginInfo.ViewModel.Replace("ViewModel", "View").Replace(".cs", ".xaml");
            // There doesn't have to be a view so make sure one exists
            if (!File.Exists(viewPath))
                return null;

            // Compile the view if found, must be done on UI thread sadly
            object view = null;
            Execute.OnUIThread(() => view = XamlReader.Parse(File.ReadAllText(viewPath)));
            var viewType = view.GetType();
            // Make sure it's a valid UIElement
            if (viewType.IsAbstract || !typeof(UIElement).IsAssignableFrom(viewType))
            {
                var e = new ArtemisPluginException(pluginInfo, $"Found type for view: {viewType.Name}, but it wasn't a class derived from UIElement");
                throw e;
            }

            return (UIElement) view;
        }
    }
}