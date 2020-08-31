using System;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    public class RenderProfileElementEventArgs : EventArgs
    {
        public RenderProfileElementEventArgs(RenderProfileElement renderProfileElement)
        {
            RenderProfileElement = renderProfileElement;
        }

        public RenderProfileElementEventArgs(RenderProfileElement renderProfileElement, RenderProfileElement previousRenderProfileElement)
        {
            RenderProfileElement = renderProfileElement;
            PreviousRenderProfileElement = previousRenderProfileElement;
        }

        public RenderProfileElement RenderProfileElement { get; }
        public RenderProfileElement PreviousRenderProfileElement { get; }
    }
}