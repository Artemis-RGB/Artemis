using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization
{
    public abstract class DataModelInputViewModel<T> : DataModelInputViewModel
    {
        protected DataModelInputViewModel(DataModelPropertyAttribute description)
        {
            Description = description;
        }

        public DataModelPropertyAttribute Description { get; }
        internal override object InternalGuard { get; } = null;
    }

    /// <summary>
    ///     For internal use only, implement <see cref="DataModelInputViewModel{T}" /> instead.
    /// </summary>
    public abstract class DataModelInputViewModel : PropertyChangedBase
    {
        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="DataModelInputViewModel{T}" /> instead.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        internal abstract object InternalGuard { get; }
    }
}