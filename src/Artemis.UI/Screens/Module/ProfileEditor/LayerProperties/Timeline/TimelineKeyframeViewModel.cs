using System;
using System.Linq;
using System.Windows;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Utilities;
using Artemis.UI.Shared.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineKeyframeViewModel<T> : TimelineKeyframeViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelineKeyframeViewModel(IProfileEditorService profileEditorService, LayerPropertyKeyframe<T> layerPropertyKeyframe)
            : base(profileEditorService, layerPropertyKeyframe)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyKeyframe = layerPropertyKeyframe;
        }

        public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }

        #region Context menu actions

        public void Copy()
        {
            var newKeyframe = new LayerPropertyKeyframe<T>(
                LayerPropertyKeyframe.Value,
                LayerPropertyKeyframe.Position,
                LayerPropertyKeyframe.EasingFunction,
                LayerPropertyKeyframe.LayerProperty
            );
            LayerPropertyKeyframe.LayerProperty.AddKeyframe(newKeyframe);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Delete()
        {
            LayerPropertyKeyframe.LayerProperty.RemoveKeyframe(LayerPropertyKeyframe);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion
    }

    public abstract class TimelineKeyframeViewModel : PropertyChangedBase
    {
        private readonly IProfileEditorService _profileEditorService;
        private int _pixelsPerSecond;

        protected TimelineKeyframeViewModel(IProfileEditorService profileEditorService, BaseLayerPropertyKeyframe baseLayerPropertyKeyframe)
        {
            _profileEditorService = profileEditorService;
            BaseLayerPropertyKeyframe = baseLayerPropertyKeyframe;
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
        }

        public BaseLayerPropertyKeyframe BaseLayerPropertyKeyframe { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; set; }

        public bool IsSelected { get; set; }
        public double X { get; set; }
        public string Timestamp { get; set; }

        public UIElement ParentView { get; set; }

        public void Update(int pixelsPerSecond)
        {
            _pixelsPerSecond = pixelsPerSecond;

            X = pixelsPerSecond * BaseLayerPropertyKeyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(BaseLayerPropertyKeyframe.Position.TotalSeconds):00}.{BaseLayerPropertyKeyframe.Position.Milliseconds:000}";
        }

        #region Keyframe movement

        #endregion

        #region Easing

        public void ContextMenuOpening()
        {
            CreateEasingViewModels();
        }

        public void ContextMenuClosing()
        {
            EasingViewModels.Clear();
        }

        private void CreateEasingViewModels()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(this, v)));
        }

        public void SelectEasingMode(TimelineEasingViewModel easingViewModel)
        {
            BaseLayerPropertyKeyframe.EasingFunction = easingViewModel.EasingFunction;
            // Set every selection to false except on the VM that made the change
            foreach (var propertyTrackEasingViewModel in EasingViewModels.Where(vm => vm != easingViewModel))
                propertyTrackEasingViewModel.IsEasingModeSelected = false;


            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        #region Movement

        private bool _movementReleased = true;
        private TimeSpan _startOffset;

        public void ApplyMovement(TimeSpan cursorTime)
        {
            if (_movementReleased)
            {
                _movementReleased = false;
                _startOffset = cursorTime - BaseLayerPropertyKeyframe.Position;
            }
            else
            {
                BaseLayerPropertyKeyframe.Position = cursorTime - _startOffset;
                if (BaseLayerPropertyKeyframe.Position < TimeSpan.Zero)
                    BaseLayerPropertyKeyframe.Position = TimeSpan.Zero;

                Update(_pixelsPerSecond);
            }
        }

        public void ReleaseMovement()
        {
            _movementReleased = true;
        }

        #endregion
    }
}