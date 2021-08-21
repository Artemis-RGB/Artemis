using System;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding that binds a certain <see cref="LayerProperty{T}" /> to a value inside a
    ///     <see cref="DataModel" />
    /// </summary>
    public interface IDataBinding : IStorageModel, IUpdateModel, IDisposable
    {
        /// <summary>
        ///     Updates the smoothing progress of the data binding and recalculates the value next <see cref="Apply" /> call
        /// </summary>
        /// <param name="delta">The delta to apply during update</param>
        void UpdateWithDelta(TimeSpan delta);

        /// <summary>
        ///     Applies the data binding to the layer property
        /// </summary>
        void Apply();
        
        /// <summary>
        ///     If the data binding is enabled, loads the node script for that data binding
        /// </summary>
        void LoadNodeScript();
    }
}