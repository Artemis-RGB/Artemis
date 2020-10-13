using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core
{
    /// <summary>
    ///     An abstract class for display condition parts
    /// </summary>
    public abstract class DataModelConditionPart : IDisposable
    {
        private readonly List<DataModelConditionPart> _children = new List<DataModelConditionPart>();

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
            }
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
        internal abstract bool EvaluateObject(object target);

        internal abstract void Save();
        internal abstract DataModelConditionPartEntity GetEntity();

        #region IDisposable

        /// <summary>
        /// Disposed the condition part
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
    }
}