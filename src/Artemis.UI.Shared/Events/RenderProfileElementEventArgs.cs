using System;
using Artemis.Core.Models.Profile;

namespace Artemis.UI.Shared.Events
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