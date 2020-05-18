using System;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : TreePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel, PropertyInputViewModel<T> propertyInputViewModel,
            IProfileEditorService profileEditorService) : base(layerPropertyBaseViewModel)
        {
            _profileEditorService = profileEditorService;
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
    }
}