using System;
using System.Windows;
using System.Windows.Input;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public abstract class VisualizationToolViewModel : CanvasViewModel
    {
        private Cursor _cursor;
        private bool _isMouseDown;
        private Point _mouseDownStartPosition;

        protected VisualizationToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService)
        {
            // Not relevant for visualization tools as they overlay the entire canvas
            X = 0;
            Y = 0;

            ProfileViewModel = profileViewModel;
            ProfileEditorService = profileEditorService;
            Cursor = Cursors.Arrow;
        }

        public ProfileViewModel ProfileViewModel { get; }
        public IProfileEditorService ProfileEditorService { get; }

        public Cursor Cursor
        {
            get => _cursor;
            protected set => SetAndNotify(ref _cursor, value);
        }

        public bool IsMouseDown
        {
            get => _isMouseDown;
            protected set => SetAndNotify(ref _isMouseDown, value);
        }

        public Point MouseDownStartPosition
        {
            get => _mouseDownStartPosition;
            protected set => SetAndNotify(ref _mouseDownStartPosition, value);
        }

        public virtual void MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            MouseDownStartPosition = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
        }

        public virtual void MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
        }

        public virtual void MouseMove(object sender, MouseEventArgs e)
        {
        }

        public virtual void MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        public virtual void KeyUp(KeyEventArgs e)
        {
        }

        public virtual void KeyDown(KeyEventArgs e)
        {
        }

        protected Rect GetSquareRectBetweenPoints(Point start, Point end)
        {
            // Find the shortest side
            var size = Math.Min(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));

            // There's probably a very elegant way to do this, and this is not it
            if (end.X < start.X && end.Y < start.Y)
                return new Rect(start.X - size, start.Y - size, size, size);
            if (end.X < start.X)
                return new Rect(start.X - size, Math.Min(start.Y, end.Y), size, size);
            if (end.Y < start.Y)
                return new Rect(Math.Min(start.X, end.X), start.Y - size, size, size);
            return new Rect(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), size, size);
        }
    }
}