using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;

namespace Artemis.Core
{
    /// <summary>
    ///    Represents a data binding that binds a certain <see cref="LayerProperty{T}" /> to a value inside a <see cref="DataModel" />
    /// </summary>
    public interface IDataBinding : IStorageModel, IUpdateModel
    {
        /// <summary>
        ///     (Re)initializes the data binding
        /// </summary>
        /// <param name="dataModelService"></param>
        /// <param name="dataBindingService"></param>
        void Initialize(IDataModelService dataModelService, IDataBindingService dataBindingService);

        /// <summary>
        ///     Applies the data binding to the layer property
        /// </summary>
        void Apply();
    }
}