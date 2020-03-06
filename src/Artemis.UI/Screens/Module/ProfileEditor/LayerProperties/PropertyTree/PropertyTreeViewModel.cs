using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.UI.Events;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;

        public PropertyTreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertiesViewModel = layerPropertiesViewModel;
            PropertyTreeItemViewModels = new BindableCollection<PropertyTreeItemViewModel>();

            _profileEditorService.CurrentTimeChanged += OnCurrentTimeChanged;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public BindableCollection<PropertyTreeItemViewModel> PropertyTreeItemViewModels { get; set; }

        public void Dispose()
        {
            _profileEditorService.CurrentTimeChanged -= OnCurrentTimeChanged;
            _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;

            foreach (var propertyTreeItemViewModel in PropertyTreeItemViewModels)
                propertyTreeItemViewModel.Dispose();
        }

        public void AddLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            // Add as a root VM
            if (layerPropertyViewModel.Parent == null)
                PropertyTreeItemViewModels.Add(new PropertyTreeParentViewModel(layerPropertyViewModel));
            // Add recursively to one of the child VMs
            else
            {
                foreach (var propertyTreeItemViewModel in PropertyTreeItemViewModels)
                    propertyTreeItemViewModel.AddLayerProperty(layerPropertyViewModel);
            }
        }

        public void RemoveLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            // Remove a root VM
            var rootVm = PropertyTreeItemViewModels.FirstOrDefault(vm => vm.LayerPropertyViewModel == layerPropertyViewModel);
            if (rootVm != null)
                PropertyTreeItemViewModels.Remove(rootVm);
            // Remove recursively from one of the child VMs
            else
            {
                foreach (var propertyTreeItemViewModel in PropertyTreeItemViewModels)
                    propertyTreeItemViewModel.RemoveLayerProperty(layerPropertyViewModel);
            }
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

        public void PropertyTreePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled || !(sender is TreeView))
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control) sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }

        private void OnCurrentTimeChanged(object sender, EventArgs e)
        {
            Update(false);
        }

        private void OnSelectedProfileElementUpdated(object sender, ProfileElementEventArgs e)
        {
            Update(true);
        }
    }
}