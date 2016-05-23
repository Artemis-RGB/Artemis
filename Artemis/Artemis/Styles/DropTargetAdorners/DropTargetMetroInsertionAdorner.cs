// Direct copy-paste from GongSolutions.WPF.DragDrop - https://github.com/punker76/gong-wpf-dragdrop
// All credit goes to their awesome library, I merely copied this in order to change the highlight color.

// BSD 3-Clause License
// 
// Copyright (c) 2015, Jan Karger (Steven Kirk)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// * Neither the name of gong-wpf-dragdrop nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro;

namespace Artemis.Styles.DropTargetAdorners
{
    public class DropTargetMetroInsertionAdorner : DropTargetAdorner
    {
        private static readonly Pen m_Pen;
        private static readonly PathGeometry m_Triangle;

        static DropTargetMetroInsertionAdorner()
        {
            // Create the pen and triangle in a static constructor and freeze them to improve performance.
            const int triangleSize = 5;

            var color = (Color)ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"];
            m_Pen = new Pen(new SolidColorBrush(color), 2);
            m_Pen.Freeze();

            var firstLine = new LineSegment(new Point(0, -triangleSize), false);
            firstLine.Freeze();
            var secondLine = new LineSegment(new Point(0, triangleSize), false);
            secondLine.Freeze();

            var figure = new PathFigure { StartPoint = new Point(triangleSize, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            m_Triangle = new PathGeometry();
            m_Triangle.Figures.Add(figure);
            m_Triangle.Freeze();
        }

        public DropTargetMetroInsertionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var itemsControl = DropInfo.VisualTarget as ItemsControl;

            if (itemsControl != null)
            {
                // Get the position of the item at the insertion index. If the insertion point is
                // to be after the last item, then get the position of the last item and add an 
                // offset later to draw it at the end of the list.
                ItemsControl itemParent;

                if (DropInfo.VisualTargetItem != null)
                {
                    itemParent = ItemsControl.ItemsControlFromItemContainer(DropInfo.VisualTargetItem);
                }
                else
                {
                    itemParent = itemsControl;
                }

                var index = Math.Min(DropInfo.InsertIndex, itemParent.Items.Count - 1);

                var lastItemInGroup = false;
                var targetGroup = DropInfo.TargetGroup;
                if (targetGroup != null && targetGroup.IsBottomLevel &&
                    DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                {
                    var indexOf = targetGroup.Items.IndexOf(DropInfo.TargetItem);
                    lastItemInGroup = indexOf == targetGroup.ItemCount - 1;
                    if (lastItemInGroup && DropInfo.InsertIndex != itemParent.Items.Count)
                    {
                        index--;
                    }
                }

                var itemContainer = (UIElement)itemParent.ItemContainerGenerator.ContainerFromIndex(index);

                if (itemContainer != null)
                {
                    var itemRect = new Rect(itemContainer.TranslatePoint(new Point(), AdornedElement),
                        itemContainer.RenderSize);
                    Point point1, point2;
                    double rotation = 0;

                    if (DropInfo.VisualTargetOrientation == Orientation.Vertical)
                    {
                        if (DropInfo.InsertIndex == itemParent.Items.Count || lastItemInGroup)
                        {
                            itemRect.Y += itemContainer.RenderSize.Height;
                        }

                        point1 = new Point(itemRect.X, itemRect.Y);
                        point2 = new Point(itemRect.Right, itemRect.Y);
                    }
                    else
                    {
                        var itemRectX = itemRect.X;

                        if (DropInfo.VisualTargetFlowDirection == FlowDirection.LeftToRight &&
                            DropInfo.InsertIndex == itemParent.Items.Count)
                        {
                            itemRectX += itemContainer.RenderSize.Width;
                        }
                        else if (DropInfo.VisualTargetFlowDirection == FlowDirection.RightToLeft &&
                                 DropInfo.InsertIndex != itemParent.Items.Count)
                        {
                            itemRectX += itemContainer.RenderSize.Width;
                        }

                        point1 = new Point(itemRectX, itemRect.Y);
                        point2 = new Point(itemRectX, itemRect.Bottom);
                        rotation = 90;
                    }

                    drawingContext.DrawLine(m_Pen, point1, point2);
                    DrawTriangle(drawingContext, point1, rotation);
                    DrawTriangle(drawingContext, point2, 180 + rotation);
                }
            }
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(rotation));

            drawingContext.DrawGeometry(m_Pen.Brush, null, m_Triangle);

            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}