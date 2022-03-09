using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an element of a <see cref="Profile" />
    /// </summary>
    public abstract class ProfileElement : BreakableModel, IDisposable
    {
        internal readonly List<ProfileElement> ChildrenList;
        private Guid _entityId;
        private string? _name;
        private int _order;
        private ProfileElement? _parent;
        private Profile _profile;
        private bool _suspended;

        internal ProfileElement(Profile profile)
        {
            _profile = profile;
            ChildrenList = new List<ProfileElement>();
            Children = new ReadOnlyCollection<ProfileElement>(ChildrenList);
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
        public ProfileElement? Parent
        {
            get => _parent;
            internal set => SetAndNotify(ref _parent, value);
        }

        /// <summary>
        ///     The element's children
        /// </summary>
        public ReadOnlyCollection<ProfileElement> Children { get; }

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
        public string? Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     Gets or sets the suspended state, if suspended the element is skipped in render and update
        /// </summary>
        public bool Suspended
        {
            get => _suspended;
            set => SetAndNotify(ref _suspended, value);
        }

        /// <summary>
        ///     Gets a boolean indicating whether the profile element is disposed
        /// </summary>
        public bool Disposed { get; protected set; }

        #region Overrides of BreakableModel

        /// <inheritdoc />
        public override string BrokenDisplayName => Name ?? GetType().Name;

        #endregion

        /// <summary>
        ///     Updates the element
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Renders the element
        /// </summary>
        public abstract void Render(SKCanvas canvas, SKPointI basePosition);

        /// <summary>
        ///     Resets the internal state of the element
        /// </summary>
        public abstract void Reset();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(EntityId)}: {EntityId}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }

        /// <summary>
        ///     Occurs when a child was added to the <see cref="Children" /> list
        /// </summary>
        public event EventHandler<ProfileElementEventArgs>? ChildAdded;

        /// <summary>
        ///     Occurs when a child was removed from the <see cref="Children" /> list
        /// </summary>
        public event EventHandler<ProfileElementEventArgs>? ChildRemoved;

        /// <summary>
        ///     Occurs when a child was added to the <see cref="Children" /> list of this element or any of it's descendents.
        /// </summary>
        public event EventHandler<ProfileElementEventArgs>? DescendentAdded;

        /// <summary>
        ///     Occurs when a child was removed from the <see cref="Children" /> list of this element or any of it's descendents.
        /// </summary>
        public event EventHandler<ProfileElementEventArgs>? DescendentRemoved;

        /// <summary>
        ///     Invokes the <see cref="ChildAdded" /> event
        /// </summary>
        protected virtual void OnChildAdded(ProfileElement child)
        {
            ChildAdded?.Invoke(this, new ProfileElementEventArgs(child));
        }

        /// <summary>
        ///     Invokes the <see cref="ChildRemoved" /> event
        /// </summary>
        protected virtual void OnChildRemoved(ProfileElement child)
        {
            ChildRemoved?.Invoke(this, new ProfileElementEventArgs(child));
        }

        /// <summary>
        ///     Invokes the <see cref="DescendentAdded" /> event
        /// </summary>
        protected virtual void OnDescendentAdded(ProfileElement child)
        {
            DescendentAdded?.Invoke(this, new ProfileElementEventArgs(child));
            Parent?.OnDescendentAdded(child);
        }

        /// <summary>
        ///     Invokes the <see cref="DescendentRemoved" /> event
        /// </summary>
        protected virtual void OnDescendentRemoved(ProfileElement child)
        {
            DescendentRemoved?.Invoke(this, new ProfileElementEventArgs(child));
            Parent?.OnDescendentRemoved(child);
        }

        /// <summary>
        ///     Disposes the profile element
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Hierarchy

        /// <summary>
        ///     Adds a profile element to the <see cref="Children" /> collection, optionally at the given position (0-based)
        /// </summary>
        /// <param name="child">The profile element to add</param>
        /// <param name="order">The order where to place the child (0-based), defaults to the end of the collection</param>
        public virtual void AddChild(ProfileElement child, int? order = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            ProfileElement? current = this;
            while (current != null)
            {
                if (ReferenceEquals(child, this))
                    throw new ArtemisCoreException("Cannot make an element a child of itself");
                current = current.Parent;
            }

            lock (ChildrenList)
            {
                if (ChildrenList.Contains(child))
                    return;

                // Add to the end of the list
                if (order == null)
                {
                    ChildrenList.Add(child);
                }
                // Insert at the given index
                else
                {
                    if (order < 0)
                        order = 0;
                    if (order > ChildrenList.Count)
                        order = ChildrenList.Count;
                    ChildrenList.Insert(order.Value, child);
                }

                child.Parent = this;
                StreamlineOrder();
            }

            OnChildAdded(child);
            OnDescendentAdded(child);
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
                StreamlineOrder();

                child.Parent = null;
            }

            OnChildRemoved(child);
            OnDescendentRemoved(child);
        }

        private void StreamlineOrder()
        {
            for (int index = 0; index < ChildrenList.Count; index++)
                ChildrenList[index].Order = index + 1;
        }

        /// <summary>
        ///     Returns a flattened list of all child render elements
        /// </summary>
        /// <returns></returns>
        public List<RenderProfileElement> GetAllRenderElements()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            List<RenderProfileElement> elements = new();
            foreach (RenderProfileElement childElement in Children.Where(c => c is RenderProfileElement).Cast<RenderProfileElement>())
            {
                // Add all folders in this element
                elements.Add(childElement);
                // Add all folders in folders inside this element
                elements.AddRange(childElement.GetAllRenderElements());
            }

            return elements;
        }

        /// <summary>
        ///     Returns a flattened list of all child folders
        /// </summary>
        /// <returns></returns>
        public List<Folder> GetAllFolders()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            List<Folder> folders = new();
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

            List<Layer> layers = new();

            // Add all layers in this element
            layers.AddRange(Children.Where(c => c is Layer).Cast<Layer>());

            // Add all layers in folders inside this element
            foreach (Folder childFolder in Children.Where(c => c is Folder).Cast<Folder>())
                layers.AddRange(childFolder.GetAllLayers());

            return layers;
        }

        /// <summary>
        ///     Returns a name for a new layer according to any other layers with a default name similar to creating new folders in
        ///     Explorer
        /// </summary>
        /// <returns>The resulting name i.e. <c>New layer</c> or <c>New layer (2)</c></returns>
        public string GetNewLayerName()
        {
            if (!Children.Any(c => c is Layer))
                return "New layer";

            // Grab existing unnamed layers and get the first available number 
            // Looks slow but it's not https://stackoverflow.com/a/8865806/5015269
            Regex regex = new(@"New layer \((\d+)\)");
            int firstAvailable = Enumerable.Range(1, int.MaxValue)
                .Except(Children.Where(c => c is Layer && c.Name != null && regex.IsMatch(c.Name))
                    .Select(c => int.Parse(regex.Match(c.Name!).Groups[1].Value))
                    .OrderBy(i => i))
                .First();
            return $"New layer ({firstAvailable})";
        }

        /// <summary>
        ///     Returns a name for a new folder according to any other folders with a default name similar to creating new folders
        ///     in Explorer
        /// </summary>
        /// <returns>The resulting name i.e. <c>New folder</c> or <c>New folder (2)</c></returns>
        public string GetNewFolderName()
        {
            if (!Children.Any(c => c is Folder))
                return "New folder";

            // Grab existing unnamed layers and get the first available number 
            // Looks slow but it's not https://stackoverflow.com/a/8865806/5015269
            Regex regex = new(@"New folder \((\d+)\)");
            int firstAvailable = Enumerable.Range(1, int.MaxValue)
                .Except(Children.Where(c => c is Folder && c.Name != null && regex.IsMatch(c.Name))
                    .Select(c => int.Parse(regex.Match(c.Name!).Groups[1].Value))
                    .OrderBy(i => i))
                .First();
            return $"New folder ({firstAvailable})";
        }

        #endregion

        #region Storage

        internal abstract void Load();
        internal abstract void Save();

        #endregion
    }
}