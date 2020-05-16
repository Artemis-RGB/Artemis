using System;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : TreePropertyViewModel
    {
        public TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel, PropertyInputViewModel<T> propertyInputViewModel) : base(layerPropertyBaseViewModel)
        {
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
            PropertyInputViewModel = propertyInputViewModel;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }
        public PropertyInputViewModel<T> PropertyInputViewModel { get; set; }

        public override void Dispose()
        {
            PropertyInputViewModel.Dispose();
        }
    }

    public abstract class TreePropertyViewModel : IDisposable
    {
        protected TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public abstract void Dispose();

        public static void RegisterPropertyInputViewModel()
        {
        }
    }
}