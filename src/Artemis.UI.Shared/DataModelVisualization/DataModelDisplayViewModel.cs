using Artemis.Core.DataModelExpansions;
using Stylet;

namespace Artemis.UI.Shared
{
    public abstract class DataModelDisplayViewModel<T> : DataModelDisplayViewModel
    {
        private T _displayValue;

        public T DisplayValue
        {
            get => _displayValue;
            set
            {
                if (!SetAndNotify(ref _displayValue, value)) return;
                OnDisplayValueUpdated();
            }
        }

        internal override object InternalGuard => null;

        public override void UpdateValue(object model)
        {
            DisplayValue = model is T value ? value : default;
        }

        protected virtual void OnDisplayValueUpdated()
        {
        }
    }

    /// <summary>
    ///     For internal use only, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
    /// </summary>
    public abstract class DataModelDisplayViewModel : PropertyChangedBase
    {
        private DataModelPropertyAttribute _propertyDescription;

        /// <summary>
        ///     Gets the property description of this value
        /// </summary>
        public DataModelPropertyAttribute PropertyDescription
        {
            get => _propertyDescription;
            internal set => SetAndNotify(ref _propertyDescription, value);
        }

        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
        /// </summary>
        internal abstract object InternalGuard { get; }

        public abstract void UpdateValue(object model);
    }
}