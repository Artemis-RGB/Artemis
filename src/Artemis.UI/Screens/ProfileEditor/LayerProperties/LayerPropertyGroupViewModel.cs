using System;
using System.Collections.Generic;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public sealed class LayerPropertyGroupViewModel : Conductor<Screen>.Collection.AllActive
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private bool _isVisible;
        private TreeGroupViewModel _treeGroupViewModel;
        private TimelineGroupViewModel _timelineGroupViewModel;

        public LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory)
        {
            _profileEditorService = profileEditorService;
            _layerPropertyVmFactory = layerPropertyVmFactory;

            LayerPropertyGroup = layerPropertyGroup;
            IsVisible = !LayerPropertyGroup.IsHidden;

            TreeGroupViewModel = _layerPropertyVmFactory.TreeGroupViewModel(this);
            TreeGroupViewModel.ConductWith(this);
            TimelineGroupViewModel = _layerPropertyVmFactory.TimelineGroupViewModel(this);
            TimelineGroupViewModel.ConductWith(this);
        }

        public LayerPropertyGroup LayerPropertyGroup { get; }

        public TreeGroupViewModel TreeGroupViewModel
        {
            get => _treeGroupViewModel;
            set => SetAndNotify(ref _treeGroupViewModel, value);
        }

        public TimelineGroupViewModel TimelineGroupViewModel
        {
            get => _timelineGroupViewModel;
            set => SetAndNotify(ref _timelineGroupViewModel, value);
        }

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


        protected override void OnInitialActivate()
        {
            LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;

            PopulateChildren();

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            LayerPropertyGroup.VisibilityChanged -= LayerPropertyGroupOnVisibilityChanged;
            base.OnClose();
        }

        public void UpdateOrder(int order)
        {
            if (LayerPropertyGroup.LayerEffect != null)
                LayerPropertyGroup.LayerEffect.Order = order;
            NotifyOfPropertyChange(nameof(IsExpanded));
        }

        public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels(bool expandedOnly)
        {
            List<ITimelineKeyframeViewModel> result = new();
            if (expandedOnly && !IsExpanded)
                return result;

            foreach (Screen child in Items)
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
        public void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            foreach (Screen item in Items)
            {
                if (item is LayerPropertyViewModel layerPropertyViewModel)
                    layerPropertyViewModel.TimelinePropertyViewModel.WipeKeyframes(start, end);
                else if (item is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
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
            foreach (Screen item in Items)
            {
                if (item is LayerPropertyViewModel layerPropertyViewModel)
                    layerPropertyViewModel.TimelinePropertyViewModel.ShiftKeyframes(start, end, amount);
                else if (item is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
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
            foreach (PropertyInfo propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                if (Attribute.IsDefined(propertyInfo, typeof(LayerPropertyIgnoreAttribute)))
                    continue;

                if (typeof(ILayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    ILayerProperty value = (ILayerProperty) propertyInfo.GetValue(LayerPropertyGroup);
                    // Ensure a supported input VM was found, otherwise don't add it
                    if (value != null && _profileEditorService.CanCreatePropertyInputViewModel(value))
                        Items.Add(_layerPropertyVmFactory.LayerPropertyViewModel(value));
                }
                else if (typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    LayerPropertyGroup value = (LayerPropertyGroup) propertyInfo.GetValue(LayerPropertyGroup);
                    if (value != null)
                        Items.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(value));
                }
            }
        }
    }
}