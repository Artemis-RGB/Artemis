using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;
using Stylet;

namespace Artemis.Core
{
    public abstract class ProfileElement : PropertyChangedBase, IDisposable
    {
        private bool _enabled;
        private Guid _entityId;
        private string _name;
        private int _order;
        private ProfileElement _parent;
        private Profile _profile;
        protected List<ProfileElement> ChildrenList;
        protected bool Disposed;

        protected ProfileElement()
        {
            ChildrenList = new List<ProfileElement>();
        }

        /// <summary>
        ///     Gets the unique ID of this profile element
        /// </summary>
        public Guid EntityId
        {
            get => _entityId;
            internal set => SetAndNotify(ref _entityId, value);
        }

        /// <summary>
        ///     Gets the profile this element belongs to
        /// </summary>
        public Profile Profile
        {
            get => _profile;
            internal set => SetAndNotify(ref _profile, value);
        }

        /// <summary>
        ///     Gets the parent of this element
        /// </summary>
        public ProfileElement Parent
        {
            get => _parent;
            internal set => SetAndNotify(ref _parent, value);
        }

        /// <summary>
        ///     The element's children
        /// </summary>
        public ReadOnlyCollection<ProfileElement> Children => ChildrenList.AsReadOnly();

        /// <summary>
        ///     The order in which this element appears in the update loop and editor
        /// </summary>
        public int Order
        {
            get => _order;
            internal set => SetAndNotify(ref _order, value);
        }

        /// <summary>
        ///     The name which appears in the editor
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     Gets or sets the enabled state, if not enabled the element is skipped in render and update
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetAndNotify(ref _enabled, value);
        }

        /// <summary>
        ///     Updates the element
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Renders the element
        /// </summary>
        public abstract void Render(SKCanvas canvas);

        /// <summary>
        ///     Resets the internal state of the element
        /// </summary>
        public abstract void Reset();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(EntityId)}: {EntityId}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }

        #region Hierarchy

        /// <summary>
        ///     Adds a profile element to the <see cref="Children" /> collection, optionally at the given position (1-based)
        /// </summary>
        /// <param name="child">The profile element to add</param>
        /// <param name="order">The order where to place the child (1-based), defaults to the end of the collection</param>
        public virtual void AddChild(ProfileElement child, int? order = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            lock (ChildrenList)
            {
                if (ChildrenList.Contains(child))
                    return;

                // Add to the end of the list
                if (order == null)
                {
                    ChildrenList.Add(child);
                    child.Order = ChildrenList.Count;
                }
                // Shift everything after the given order
                else
                {
                    foreach (ProfileElement profileElement in ChildrenList.Where(c => c.Order >= order).ToList())
                        profileElement.Order++;

                    int targetIndex;
                    if (order == 0)
                        targetIndex = 0;
                    else if (order > ChildrenList.Count)
                        targetIndex = ChildrenList.Count;
                    else
                        targetIndex = ChildrenList.FindIndex(c => c.Order == order + 1);

                    ChildrenList.Insert(targetIndex, child);
                    child.Order = order.Value;
                }

                child.Parent = this;
            }

            OnChildAdded();
        }

        /// <summary>
        ///     Removes a profile element from the <see cref="Children" /> collection
        /// </summary>
        /// <param name="child">The profile element to remove</param>
        public virtual void RemoveChild(ProfileElement child)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            lock (ChildrenList)
            {
                ChildrenList.Remove(child);

                // Shift everything after the given order
                foreach (ProfileElement profileElement in ChildrenList.Where(c => c.Order > child.Order).ToList())
                    profileElement.Order--;

                child.Parent = null;
            }

            OnChildRemoved();
        }

        /// <summary>
        ///     Returns a flattened list of all child folders
        /// </summary>
        /// <returns></returns>
        public List<Folder> GetAllFolders()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            List<Folder> folders = new List<Folder>();
            foreach (Folder childFolder in Children.Where(c => c is Folder).Cast<Folder>())
            {
                // Add all folders in this element
                folders.Add(childFolder);
                // Add all folders in folders inside this element
                folders.AddRange(childFolder.GetAllFolders());
            }

            return folders;
        }

        /// <summary>
        ///     Returns a flattened list of all child layers
        /// </summary>
        /// <returns></returns>
        public List<Layer> GetAllLayers()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            List<Layer> layers = new List<Layer>();

            // Add all layers in this element
            layers.AddRange(Children.Where(c => c is Layer).Cast<Layer>());

            // Add all layers in folders inside this element
            foreach (Folder childFolder in Children.Where(c => c is Folder).Cast<Folder>())
                layers.AddRange(childFolder.GetAllLayers());

            return layers;
        }

        #endregion

        #region Storage

        internal abstract void Load();
        internal abstract void Save();

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        #endregion

        #region Events

        public event EventHandler ChildAdded;
        public event EventHandler ChildRemoved;

        protected virtual void OnChildAdded()
        {
            ChildAdded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnChildRemoved()
        {
            ChildRemoved?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}