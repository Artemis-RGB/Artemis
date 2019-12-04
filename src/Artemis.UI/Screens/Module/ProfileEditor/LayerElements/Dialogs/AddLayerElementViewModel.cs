using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Dialogs;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerElements.Dialogs
{
    public class AddLayerElementViewModel : DialogViewModelBase
    {
        private readonly ILayerService _layerService;


        public AddLayerElementViewModel(IPluginService pluginService, ILayerService layerService, Layer layer)
        {
            _layerService = layerService;
            Layer = layer;

            LayerElementDescriptors = new BindableCollection<LayerElementDescriptor>();
            var layerElementProviders = pluginService.GetPluginsOfType<LayerElementProvider>();
            foreach (var layerElementProvider in layerElementProviders)
                LayerElementDescriptors.AddRange(layerElementProvider.LayerElementDescriptors);
        }

        public Layer Layer { get; }

        public LayerElementDescriptor SelectedLayerElementDescriptor { get; set; }
        public BindableCollection<LayerElementDescriptor> LayerElementDescriptors { get; set; }
        public bool CanAccept => SelectedLayerElementDescriptor != null;

        public void Accept()
        {
            if (Session.IsEnded)
                return;
            var layerElement = _layerService.InstantiateLayerElement(Layer, SelectedLayerElementDescriptor);
            Session.Close(layerElement);
        }

        public void Cancel()
        {
            if (Session.IsEnded)
                return;
            Session.Close();
        }

        #region View event handlers

        public void ListBoxItemMouseClick(object sender, MouseButtonEventArgs args)
        {
            if (args.ClickCount > 1 && SelectedLayerElementDescriptor != null)
                Accept();
        }

        #endregion
    }
}