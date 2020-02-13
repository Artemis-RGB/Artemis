using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Profile
{
    public abstract class ProfileElement : PropertyChangedBase
    {
        protected List<ProfileElement> _children;

        protected ProfileElement()
        {
            _children = new List<ProfileElement>();
        }

        public Guid EntityId { get; internal set; }
        public Profile Profile { get; internal set; }
        public ProfileElement Parent { get; internal set; }

        /// <summary>
        ///     The element's children
        /// </summary>
        public ReadOnlyCollection<ProfileElement> Children => _children.AsReadOnly();

        /// <summary>
        ///     The order in which this element appears in the update loop and editor
        /// </summary>
        public int Order { get; internal set; }

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
        public abstract void Render(double deltaTime, SKCanvas canvas);

        public List<Folder> GetAllFolders()
        {
            var folders = new List<Folder>();
            foreach (var childFolder in Children.Where(c => c is Folder).Cast<Folder>())
            {
                // Add all folders in this element
                folders.Add(childFolder);
                // Add all folders in folders inside this element
                folders.AddRange(childFolder.GetAllFolders());
            }

            return folders;
        }

        public List<Layer> GetAllLayers()
        {
            var layers = new List<Layer>();

            // Add all layers in this element
            layers.AddRange(Children.Where(c => c is Layer).Cast<Layer>());

            // Add all layers in folders inside this element
            foreach (var childFolder in Children.Where(c => c is Folder).Cast<Folder>())
                layers.AddRange(childFolder.GetAllLayers());

            return layers;
        }

        /// <summary>
        ///     Adds a profile element to the <see cref="Children" /> collection, optionally at the given position (1-based)
        /// </summary>
        /// <param name="child">The profile element to add</param>
        /// <param name="order">The order where to place the child (1-based), defaults to the end of the collection</param>
        public void AddChild(ProfileElement child, int? order = null)
        {
            lock (_children)
            {
                // Add to the end of the list
                if (order == null)
                {
                    _children.Add(child);
                    child.Order = _children.Count;
                    return;
                }

                // Shift everything after the given order
                foreach (var profileElement in _children.Where(c => c.Order >= order).ToList())
                    profileElement.Order++;

                int targetIndex;
                if (order == 0)
                    targetIndex = 0;
                else if (order > _children.Count)
                    targetIndex = _children.Count;
                else
                    targetIndex = _children.FindIndex(c => c.Order == order + 1);

                _children.Insert(targetIndex, child);
                child.Order = order.Value;
                child.Parent = this;
            }
        }

        /// <summary>
        ///     Removes a profile element from the <see cref="Children" /> collection
        /// </summary>
        /// <param name="child">The profile element to remove</param>
        public void RemoveChild(ProfileElement child)
        {
            lock (_children)
            {
                _children.Remove(child);

                // Shift everything after the given order
                foreach (var profileElement in _children.Where(c => c.Order > child.Order).ToList())
                    profileElement.Order--;

                child.Parent = null;
            }
        }

        /// <summary>
        ///     Applies the profile element's properties to the underlying storage entity
        /// </summary>
        internal abstract void ApplyToEntity();
    }
}