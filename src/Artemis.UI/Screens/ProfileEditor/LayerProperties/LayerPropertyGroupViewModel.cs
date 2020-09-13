using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyGroupViewModel : PropertyChangedBase, IDisposable
    {
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private bool _isVisible;

        public LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory)
        {
            _layerPropertyVmFactory = layerPropertyVmFactory;

            LayerPropertyGroup = layerPropertyGroup;
            Children = new BindableCollection<PropertyChangedBase>();

            TreeGroupViewModel = layerPropertyVmFactory.TreeGroupViewModel(this);
            TimelineGroupViewModel = layerPropertyVmFactory.TimelineGroupViewModel(this);

            LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;
            IsVisible = !LayerPropertyGroup.IsHidden;

            PopulateChildren();
        }

        public LayerPropertyGroup LayerPropertyGroup { get; }
        public TreeGroupViewModel TreeGroupViewModel { get; }
        public TimelineGroupViewModel TimelineGroupViewModel { get; }
        public BindableCollection<PropertyChangedBase> Children { get; }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetAndNotify(ref _isVisible, value);
        }

        public bool IsExpanded
        {
            get => LayerPropertyGroup.ProfileElement.IsPropertyGroupExpanded(LayerPropertyGroup);
            set
            {
                LayerPropertyGroup.ProfileElement.SetPropertyGroupExpanded(LayerPropertyGroup, value);
                NotifyOfPropertyChange(nameof(IsExpanded));
            }
        }

        public void Dispose()
        {
            LayerPropertyGroup.VisibilityChanged -= LayerPropertyGroupOnVisibilityChanged;
            foreach (var child in Children)
            {
                if (child is IDisposable disposableChild)
                    disposableChild.Dispose();
            }
        }

        public void UpdateOrder(int order)
        {
            LayerPropertyGroup.LayerEffect.Order = order;
            NotifyOfPropertyChange(nameof(IsExpanded));
        }

        public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels(bool expandedOnly)
        {
            var result = new List<ITimelineKeyframeViewModel>();
            if (expandedOnly && !IsExpanded)
                return result;

            foreach (var child in Children)
            {
                if (child is LayerPropertyViewModel layerPropertyViewModel)
                    result.AddRange(layerPropertyViewModel.TimelinePropertyViewModel.GetAllKeyframeViewModels());
                else if (child is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
                    result.AddRange(layerPropertyGroupViewModel.GetAllKeyframeViewModels(expandedOnly));
            }

            return result;
        }

        /// <summary>
        ///     Removes the keyframes between the <paramref name="start" /> and <paramref name="end" /> position from this property
        ///     group
        /// </summary>
        /// <param name="start">The position at which to start removing keyframes, if null this will start at the first keyframe</param>
        /// <param name="end">The position at which to start removing keyframes, if null this will end at the last keyframe</param>
        public virtual void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            foreach (var child in Children)
            {
                if (child is LayerPropertyViewModel layerPropertyViewModel)
                    layerPropertyViewModel.TimelinePropertyViewModel.WipeKeyframes(start, end);
                else if (child is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
                    layerPropertyGroupViewModel.WipeKeyframes(start, end);
            }

            TimelineGroupViewModel.UpdateKeyframePositions();
        }

        /// <summary>
        ///     Shifts the keyframes between the <paramref name="start" /> and <paramref name="end" /> position by the provided
        ///     <paramref name="amount" />
        /// </summary>
        /// <param name="start">The position at which to start shifting keyframes, if null this will start at the first keyframe</param>
        /// <param name="end">The position at which to start shifting keyframes, if null this will end at the last keyframe</param>
        /// <param name="amount">The amount to shift the keyframes for</param>
        public void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
        {
            foreach (var child in Children)
            {
                if (child is LayerPropertyViewModel layerPropertyViewModel)
                    layerPropertyViewModel.TimelinePropertyViewModel.ShiftKeyframes(start, end, amount);
                else if (child is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
                    layerPropertyGroupViewModel.ShiftKeyframes(start, end, amount);
            }

            TimelineGroupViewModel.UpdateKeyframePositions();
        }

        private void LayerPropertyGroupOnVisibilityChanged(object sender, EventArgs e)
        {
            IsVisible = !LayerPropertyGroup.IsHidden;
        }

        private void PopulateChildren()
        {
            // Get all properties and property groups and create VMs for them
            // The group has methods for getting this without reflection but then we lose the order of the properties as they are defined on the group
            foreach (var propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                var propertyAttribute = (PropertyDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                var groupAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                var value = propertyInfo.GetValue(LayerPropertyGroup);

                // Create VMs for properties on the group
                if (propertyAttribute != null && value is ILayerProperty layerProperty)
                {
                    var layerPropertyViewModel = _layerPropertyVmFactory.LayerPropertyViewModel(layerProperty);
                    // After creation ensure a supported input VM was found, if not, discard the VM
                    if (!layerPropertyViewModel.TreePropertyViewModel.HasPropertyInputViewModel)
                        layerPropertyViewModel.Dispose();
                    else
                        Children.Add(layerPropertyViewModel);
                }
                // Create VMs for child groups on this group, resulting in a nested structure
                else if (groupAttribute != null && value is LayerPropertyGroup layerPropertyGroup)
                    Children.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerPropertyGroup));
            }
        }
    }
}