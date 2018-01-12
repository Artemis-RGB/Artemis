using System.Collections.Generic;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements.Interfaces
{
    public interface IProfileElement
    {
        /// <summary>
        ///     The element's children
        /// </summary>
        List<IProfileElement> Children { get; set; }

        /// <summary>
        ///     Updates the element
        /// </summary>
        void Update();

        /// <summary>
        ///     Renders the element
        /// </summary>
        void Render(IRGBDevice rgbDevice);
    }
}