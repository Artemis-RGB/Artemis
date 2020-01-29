using System;
using System.Windows.Input;

namespace Artemis.UI.Events
{
    public class ShapeControlEventArgs : EventArgs
    {
        public ShapeControlEventArgs(MouseEventArgs mouseEventArgs, ShapeControlPoint shapeControlPoint)
        {
            MouseEventArgs = mouseEventArgs;
            ShapeControlPoint = shapeControlPoint;
        }

        public MouseEventArgs MouseEventArgs { get; set; }
        public ShapeControlPoint ShapeControlPoint { get; }
    }

    public enum ShapeControlPoint
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
        TopCenter,
        RightCenter,
        BottomCenter,
        LeftCenter,
        LayerShape,
        Anchor
    }
}