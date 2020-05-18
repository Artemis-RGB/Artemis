using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineViewModel
    {
        private readonly LayerPropertiesViewModel _layerPropertiesViewModel;

        public TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups)
        {
            _layerPropertiesViewModel = layerPropertiesViewModel;
            LayerPropertyGroups = layerPropertyGroups;
            SelectionRectangle = new RectangleGeometry();

            UpdateKeyframes();
        }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }

        public double Width { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }

        public void UpdateKeyframes()
        {
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
            {
                foreach (var layerPropertyBaseViewModel in layerPropertyGroupViewModel.GetAllChildren())
                {
                    if (layerPropertyBaseViewModel is LayerPropertyViewModel layerPropertyViewModel)
                        layerPropertyViewModel.TimelinePropertyBaseViewModel.UpdateKeyframes(this);
                }
            }
        }

        #region Keyframe movement

        public void MoveSelectedKeyframes(TimeSpan cursorTime)
        {
            // Ensure the selection rectangle doesn't show, the view isn't aware of different types of dragging
            SelectionRectangle.Rect = new Rect();

            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ApplyMovement(cursorTime);

            _layerPropertiesViewModel.ProfileEditorService.UpdateProfilePreview();
        }


        public void ReleaseSelectedKeyframes()
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ReleaseMovement();
        }

        #endregion

        #region Keyframe selection

        private Point _mouseDragStartPoint;
        private bool _mouseDragging;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            ((IInputElement) sender).CaptureMouse();

            SelectionRectangle.Rect = new Rect();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
            _mouseDragging = true;
            e.Handled = true;
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (!_mouseDragging)
                return;

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            var keyframeViewModels = GetAllKeyframeViewModels();
            var selectedKeyframes = HitTestUtilities.GetHitViewModels<TimelineKeyframeViewModel>((Visual) sender, SelectionRectangle);
            foreach (var keyframeViewModel in keyframeViewModels)
                keyframeViewModel.IsSelected = selectedKeyframes.Contains(keyframeViewModel);

            _mouseDragging = false;
            e.Handled = true;
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition((IInputElement) sender);
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;
                e.Handled = true;
            }
        }

        public void SelectKeyframe(TimelineKeyframeViewModel clicked, bool selectBetween, bool toggle)
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            if (selectBetween)
            {
                var selectedIndex = keyframeViewModels.FindIndex(k => k.IsSelected);
                // If nothing is selected, select only the clicked
                if (selectedIndex == -1)
                {
                    clicked.IsSelected = true;
                    return;
                }

                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;

                var clickedIndex = keyframeViewModels.IndexOf(clicked);
                if (clickedIndex < selectedIndex)
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(clickedIndex).Take(selectedIndex - clickedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
                else
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(selectedIndex).Take(clickedIndex - selectedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
            }
            else if (toggle)
            {
                // Toggle only the clicked keyframe, leave others alone
                clicked.IsSelected = !clicked.IsSelected;
            }
            else
            {
                // Only select the clicked keyframe
                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;
                clicked.IsSelected = true;
            }
        }

        private List<TimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            var viewModels = new List<LayerPropertyBaseViewModel>();
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                viewModels.AddRange(layerPropertyGroupViewModel.GetAllChildren());

            var keyframes = viewModels.Where(vm => vm is LayerPropertyViewModel)
                .SelectMany(vm => ((LayerPropertyViewModel) vm).TimelinePropertyBaseViewModel.TimelineKeyframeViewModels)
                .ToList();

            return keyframes;
        }

        #endregion
    }
}