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

        protected virtual void OnDisplayValueUpdated()
        {
        }

        public override void UpdateValue(object model)
        {
            DisplayValue = model is T value ? value : default;
        }

        internal override object InternalGuard => null;
    }

    /// <summary>
    ///     For internal use only, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
    /// </summary>
    public abstract class DataModelDisplayViewModel : PropertyChangedBase
    {
        public abstract void UpdateValue(object model);

        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
        /// </summary>
        internal abstract object InternalGuard { get; }
    }
}