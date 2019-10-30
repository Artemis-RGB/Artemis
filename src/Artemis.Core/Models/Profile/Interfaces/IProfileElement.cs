using System.Collections.Generic;
using System.Drawing;
using RGB.NET.Core;

namespace Artemis.Core.Models.Profile.Interfaces
{
    public interface IProfileElement
    {
        /// <summary>
        ///     The element's children
        /// </summary>
        List<IProfileElement> Children { get; set; }

        /// <summary>
        ///     The order in which this element appears in the update loop and editor
        /// </summary>
        int Order { get; set; }

        /// <summary>
        ///     The name which appears in the editor
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Updates the element
        /// </summary>
        /// <param name="deltaTime"></param>
        void Update(double deltaTime);

        /// <summary>
        ///     Renders the element
        /// </summary>
        void Render(double deltaTime, Surface.Surface surface, Graphics graphics);
    }
}