using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Properties;
using Artemis.Utilities;
using Caliburn.Micro;
using Castle.Components.DictionaryAdapter;
using MahApps.Metro;

namespace Artemis.ViewModels.Profiles
{
    public class ProfileViewModel : PropertyChangedBase
    {
        private readonly DeviceManager _deviceManager;
        private double _blurProgress;
        private double _blurRadius;
        private DateTime _downTime;
        private LayerModel _draggingLayer;
        private Point? _draggingLayerOffset;
        private DrawingImage _keyboardPreview;
        private Cursor _keyboardPreviewCursor;
        private bool _resizing;
        private LayerModel _selectedLayer;
        private bool _showAll;

        public ProfileViewModel(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;

            PreviewTimer = new Timer(40);
            ShowAll = false;

            PreviewTimer.Elapsed += InvokeUpdateKeyboardPreview;
            deviceManager.OnKeyboardChangedEvent += DeviceManagerOnOnKeyboardChangedEvent;
        }

        public ProfileModel SelectedProfile { get; set; }
        public Timer PreviewTimer { get; set; }

        public LayerModel SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (Equals(value, _selectedLayer)) return;
                _selectedLayer = value;
                NotifyOfPropertyChange(() => SelectedLayer);
            }
        }

        public DrawingImage KeyboardPreview
        {
            get { return _keyboardPreview; }
            set
            {
                if (Equals(value, _keyboardPreview)) return;
                _keyboardPreview = value;
                NotifyOfPropertyChange(() => KeyboardPreview);
            }
        }

        public double BlurRadius
        {
            get { return _blurRadius; }
            set
            {
                if (value.Equals(_blurRadius)) return;
                _blurRadius = value;
                NotifyOfPropertyChange(() => BlurRadius);
            }
        }

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                if (value == _showAll) return;
                _showAll = value;
                NotifyOfPropertyChange(() => ShowAll);
            }
        }

        public ImageSource KeyboardImage => ImageUtilities
            .BitmapToBitmapImage(_deviceManager.ActiveKeyboard?.PreviewSettings.Image ?? Resources.none);

        public bool Activated { get; set; }

        private void DeviceManagerOnOnKeyboardChangedEvent(object sender, KeyboardChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => KeyboardImage);
        }

        private void InvokeUpdateKeyboardPreview(object sender, ElapsedEventArgs e)
        {
            if (_blurProgress > 2)
                _blurProgress = 0;
            _blurProgress = _blurProgress + 0.025;
            BlurRadius = (Math.Sin(_blurProgress*Math.PI) + 1)*10 + 10;

            if (SelectedProfile == null || _deviceManager.ActiveKeyboard == null || (!ShowAll && SelectedLayer == null))
            {
                var preview = new DrawingImage();
                preview.Freeze();
                KeyboardPreview = preview;
                return;
            }


            var keyboardRect = _deviceManager.ActiveKeyboard.KeyboardRectangle(4);
            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen())
            {
                // Setup the DrawingVisual's size
                drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                // Get the layers that must be drawn
                var drawLayers = GetRenderLayers();

                // Draw the layers
                foreach (var layer in drawLayers)
                {
                    layer.Update(null, true, false);
                    if (layer.LayerType.ShowInEdtor)
                        layer.Draw(null, drawingContext, true, false);
                }

                // Get the selection color
                var accentColor = ThemeManager.DetectAppStyle(Application.Current)?.Item2?.Resources["AccentColor"];
                if (accentColor == null)
                {
                    var preview = new DrawingImage();
                    preview.Freeze();
                    KeyboardPreview = preview;
                    return;
                }

                var pen = new Pen(new SolidColorBrush((Color) accentColor), 0.4);

                // Draw the selection outline and resize indicator
                if (SelectedLayer != null && SelectedLayer.MustDraw())
                {
                    var layerRect = SelectedLayer.Properties.GetRect();
                    // Deflate the rect so that the border is drawn on the inside
                    layerRect.Inflate(-0.2, -0.2);

                    // Draw an outline around the selected layer
                    drawingContext.DrawRectangle(null, pen, layerRect);
                    // Draw a resize indicator in the bottom-right
                    drawingContext.DrawLine(pen,
                        new Point(layerRect.BottomRight.X - 1, layerRect.BottomRight.Y - 0.5),
                        new Point(layerRect.BottomRight.X - 1.2, layerRect.BottomRight.Y - 0.7));
                    drawingContext.DrawLine(pen,
                        new Point(layerRect.BottomRight.X - 0.5, layerRect.BottomRight.Y - 1),
                        new Point(layerRect.BottomRight.X - 0.7, layerRect.BottomRight.Y - 1.2));
                    drawingContext.DrawLine(pen,
                        new Point(layerRect.BottomRight.X - 0.5, layerRect.BottomRight.Y - 0.5),
                        new Point(layerRect.BottomRight.X - 0.7, layerRect.BottomRight.Y - 0.7));
                }

                // Remove the clip
                drawingContext.Pop();
            }
            var drawnPreview = new DrawingImage(visual.Drawing);
            drawnPreview.Freeze();
            KeyboardPreview = drawnPreview;
        }

        public void Activate()
        {
            Activated = true;
            PreviewTimer.Start();
        }

        public void Deactivate()
        {
            Activated = false;
            PreviewTimer.Stop();
        }

        #region Processing

        /// <summary>
        ///     Handler for clicking
        /// </summary>
        /// <param name="e"></param>
        public void MouseDownKeyboardPreview(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _downTime = DateTime.Now;
        }

        /// <summary>
        ///     Second handler for clicking, selects a the layer the user clicked on
        ///     if the used clicked on an empty spot, deselects the current layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseUpKeyboardPreview(MouseButtonEventArgs e)
        {
            if (SelectedProfile == null || SelectedProfile.IsDefault)
                return;

            var timeSinceDown = DateTime.Now - _downTime;
            if (!(timeSinceDown.TotalMilliseconds < 500))
                return;
            if (_draggingLayer != null)
                return;

            var keyboard = _deviceManager.ActiveKeyboard;
            var pos = e.GetPosition((Image) e.OriginalSource);
            var x = pos.X/((double) keyboard.PreviewSettings.Width/keyboard.Width);
            var y = pos.Y/((double) keyboard.PreviewSettings.Height/keyboard.Height);

            var hoverLayer = GetLayers().Where(l => l.MustDraw())
                .FirstOrDefault(l => l.Properties.GetRect(1).Contains(x, y));

            SelectedLayer = hoverLayer;
        }

        /// <summary>
        ///     Handler for resizing and moving the currently selected layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseMoveKeyboardPreview(MouseEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var pos = e.GetPosition((Image) e.OriginalSource);
            var keyboard = _deviceManager.ActiveKeyboard;
            var x = pos.X/((double) keyboard.PreviewSettings.Width/keyboard.Width);
            var y = pos.Y/((double) keyboard.PreviewSettings.Height/keyboard.Height);
            var hoverLayer = GetLayers().Where(l => l.MustDraw())
                .FirstOrDefault(l => l.Properties.GetRect(1).Contains(x, y));

            HandleDragging(e, x, y, hoverLayer);

            if (hoverLayer == null)
            {
                KeyboardPreviewCursor = Cursors.Arrow;
                return;
            }

            // Turn the mouse pointer into a hand if hovering over an active layer
            if (hoverLayer == SelectedLayer)
            {
                var rect = hoverLayer.Properties.GetRect(1);
                KeyboardPreviewCursor =
                    Math.Sqrt(Math.Pow(x - rect.BottomRight.X, 2) + Math.Pow(y - rect.BottomRight.Y, 2)) < 0.6
                        ? Cursors.SizeNWSE
                        : Cursors.SizeAll;
            }
            else
                KeyboardPreviewCursor = Cursors.Hand;
        }

        public Cursor KeyboardPreviewCursor
        {
            get { return _keyboardPreviewCursor; }
            set
            {
                if (Equals(value, _keyboardPreviewCursor)) return;
                _keyboardPreviewCursor = value;
                NotifyOfPropertyChange(() => KeyboardPreviewCursor);
            }
        }

        /// <summary>
        ///     Handles dragging the given layer
        /// </summary>
        /// <param name="e"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hoverLayer"></param>
        private void HandleDragging(MouseEventArgs e, double x, double y, LayerModel hoverLayer)
        {
            // Reset the dragging state on mouse release
            if (e.LeftButton == MouseButtonState.Released ||
                (_draggingLayer != null && SelectedLayer != _draggingLayer))
            {
                _draggingLayerOffset = null;
                _draggingLayer = null;
                return;
            }

            if (SelectedLayer == null || (SelectedLayer.LayerType != null && !SelectedLayer.LayerType.ShowInEdtor))
                return;

            // Setup the dragging state on mouse press
            if (_draggingLayerOffset == null && hoverLayer != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var layerRect = hoverLayer.Properties.GetRect(1);

                _draggingLayerOffset = new Point(x - SelectedLayer.Properties.X, y - SelectedLayer.Properties.Y);
                _draggingLayer = hoverLayer;
                // Detect dragging if cursor is in the bottom right
                _resizing = Math.Sqrt(Math.Pow(x - layerRect.BottomRight.X, 2) +
                                      Math.Pow(y - layerRect.BottomRight.Y, 2)) < 0.6;
            }

            if (_draggingLayerOffset == null || _draggingLayer == null || (_draggingLayer != SelectedLayer))
                return;

            var draggingProps = _draggingLayer.Properties;
            // If no setup or reset was done, handle the actual dragging action
            if (_resizing)
            {
                draggingProps.Width = (int) Math.Round(x - draggingProps.X);
                draggingProps.Height = (int) Math.Round(y - draggingProps.Y);
                if (draggingProps.Width < 1)
                    draggingProps.Width = 1;
                if (draggingProps.Height < 1)
                    draggingProps.Height = 1;
            }
            else
            {
                draggingProps.X = (int) Math.Round(x - _draggingLayerOffset.Value.X);
                draggingProps.Y = (int) Math.Round(y - _draggingLayerOffset.Value.Y);
            }
        }

        public List<LayerModel> GetRenderLayers()
        {
            // Get the layers that must be drawn
            List<LayerModel> drawLayers;
            if (ShowAll)
                return SelectedProfile.GetRenderLayers(new ProfilePreviewDataModel(), false, true);

            if (SelectedLayer == null)
                return new EditableList<LayerModel>();

            if (SelectedLayer.LayerType is FolderType)
                drawLayers = SelectedLayer.GetRenderLayers(new ProfilePreviewDataModel(), false, true);
            else
                drawLayers = new List<LayerModel> {SelectedLayer};

            return drawLayers;
        }


        private List<LayerModel> GetLayers()
        {
            if (SelectedLayer == null)
                return new List<LayerModel>();

            lock (SelectedLayer)
            {
                // Get the layers that must be drawn
                List<LayerModel> drawLayers;
                if (ShowAll)
                    drawLayers = SelectedProfile.GetLayers();
                else if (SelectedLayer.LayerType is FolderType)
                    drawLayers = SelectedLayer.GetLayers().ToList();
                else
                    drawLayers = new List<LayerModel> {SelectedLayer};

                return drawLayers;
            }
        }

        #endregion
    }
}