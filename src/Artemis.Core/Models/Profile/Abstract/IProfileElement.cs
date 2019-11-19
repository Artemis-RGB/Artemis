using System.Collections.Generic;
using System.Drawing;

namespace Artemis.Core.Models.Profile.Abstract
{
    public abstract class ProfileElement
    {
        /// <summary>
        ///     The element's children
        /// </summary>
        public List<ProfileElement> Children { get; set; }

        /// <summary>
        ///     The order in which this element appears in the update loop and editor
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     The name which appears in the editor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Updates the element
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Renders the element
        /// </summary>
        public abstract void Render(double deltaTime, Surface.Surface surface, Graphics graphics);

        /// <summary>
        ///     Applies the profile element's properties to the underlying storage entity
        /// </summary>
        internal abstract void ApplyToEntity();
    }
}