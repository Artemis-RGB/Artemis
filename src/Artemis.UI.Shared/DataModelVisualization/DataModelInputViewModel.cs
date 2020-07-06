using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization
{
    public abstract class DataModelInputViewModel<T> : DataModelInputViewModel
    {
        private T _inputValue;

        protected DataModelInputViewModel(DataModelPropertyAttribute description, T initialValue)
        {
            Description = description;
            InputValue = initialValue;
        }

        public T InputValue
        {
            get => _inputValue;
            set => SetAndNotify(ref _inputValue, value);
        }

        public DataModelPropertyAttribute Description { get; }
        internal override object InternalGuard { get; } = null;

        protected void Submit()
        {
        }
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