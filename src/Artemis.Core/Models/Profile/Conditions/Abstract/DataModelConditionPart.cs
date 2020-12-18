using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core
{
    /// <summary>
    ///     An abstract class for display condition parts
    /// </summary>
    public abstract class DataModelConditionPart : IDisposable
    {
        private readonly List<DataModelConditionPart> _children = new();

        /// <summary>
        ///     Gets the parent of this part
        /// </summary>
        public DataModelConditionPart? Parent { get; internal set; }

        /// <summary>
        ///     Gets the children of this part
        /// </summary>
        public ReadOnlyCollection<DataModelConditionPart> Children => _children.AsReadOnly();

        /// <summary>
        ///     Adds a child to the display condition part's <see cref="Children" /> collection
        /// </summary>
        /// <param name="dataModelConditionPart"></param>
        /// <param name="index">An optional index at which to insert the condition</param>
        public void AddChild(DataModelConditionPart dataModelConditionPart, int? index = null)
        {
            if (!_children.Contains(dataModelConditionPart))
            {
                dataModelConditionPart.Parent = this;
                if (index != null)
                    _children.Insert(index.Value, dataModelConditionPart);
                else
                    _children.Add(dataModelConditionPart);

                OnChildAdded();
            }
        }

        /// <summary>
        ///     Removes a child from the display condition part's <see cref="Children" /> collection
        /// </summary>
        /// <param name="dataModelConditionPart">The child to remove</param>
        public void RemoveChild(DataModelConditionPart dataModelConditionPart)
        {
            if (_children.Contains(dataModelConditionPart))
            {
                dataModelConditionPart.Parent = null;
                _children.Remove(dataModelConditionPart);
                OnChildRemoved();
            }
        }

        /// <summary>
        ///     Removes all children. You monster.
        /// </summary>
        public void ClearChildren()
        {
            while (Children.Any())
                RemoveChild(Children[0]);
        }

        /// <summary>
        ///     Evaluates the condition part on the data model
        /// </summary>
        /// <returns></returns>
        public abstract bool Evaluate();

        /// <summary>
        ///     Evaluates the condition part on the given target (currently only for lists)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal abstract bool EvaluateObject(object? target);

        internal abstract void Save();
        internal abstract DataModelConditionPartEntity GetEntity();

        #region IDisposable

        /// <summary>
        ///     Disposed the condition part
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

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a child-condition was added
        /// </summary>
        public event EventHandler? ChildAdded;

        /// <summary>
        ///     Occurs when a child-condition was removed
        /// </summary>
        public event EventHandler? ChildRemoved;

        /// <summary>
        ///     Invokers the <see cref="ChildAdded" /> event
        /// </summary>
        protected virtual void OnChildAdded()
        {
            ChildAdded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Invokers the <see cref="ChildRemoved" /> event
        /// </summary>
        protected virtual void OnChildRemoved()
        {
            ChildRemoved?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}