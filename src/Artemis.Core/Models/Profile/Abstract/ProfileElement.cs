using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public List<Folder> GetAllFolders()
        {
            var folders = new List<Folder>();
            foreach (var childFolder in Children.Where(c => c is Folder).Cast<Folder>())
            {
                folders.Add(childFolder);
                folders.AddRange(childFolder.GetAllFolders());
            }

            return folders;
        }

        public List<Layer> GetAllLayers()
        {
            var folders = new List<Layer>();
            foreach (var childLayer in Children.Where(c => c is Layer).Cast<Layer>())
            {
                folders.Add(childLayer);
                folders.AddRange(childLayer.GetAllLayers());
            }

            return folders;
        }
    }
}