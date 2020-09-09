using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding that binds a certain <see cref="LayerProperty{T}" /> to a value inside a
    ///     <see cref="DataModel" />
    /// </summary>
    public interface IDataBinding : IStorageModel, IUpdateModel, IDisposable
    {
        /// <summary>
        ///     Applies the data binding to the layer property
        /// </summary>
        void Apply();
    }
}