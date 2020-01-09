using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeViewModel : PropertyChangedBase
    {
        private readonly IProfileEditorService _profileEditorService;

        public PropertyTreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertiesViewModel = layerPropertiesViewModel;
            PropertyTreeItemViewModels = new BindableCollection<PropertyTreeItemViewModel>();

            _profileEditorService.CurrentTimeChanged += (sender, args) => Update(false);
            _profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update(true);
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public BindableCollection<PropertyTreeItemViewModel> PropertyTreeItemViewModels { get; set; }

        public void PopulateProperties(List<LayerPropertyViewModel> properties)
        {
            PropertyTreeItemViewModels.Clear();

            // Only put parents on the top-level, let parents populate their own children recursively
            foreach (var property in properties)
            {
                if (property.Children.Any())
                    PropertyTreeItemViewModels.Add(new PropertyTreeParentViewModel(property));
            }
        }

        public void ClearProperties()
        {
            PropertyTreeItemViewModels.Clear();
        }

        /// <summary>
        ///     Updates the tree item's input if it is visible and has keyframes enabled
        /// </summary>
        /// <param name="forceUpdate">Force update regardless of visibility and keyframes</param>
        public void Update(bool forceUpdate)
        {
            foreach (var viewModel in PropertyTreeItemViewModels)
                viewModel.Update(forceUpdate);
        }
    }
}