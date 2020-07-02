using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization
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

        internal override void UpdateValue(object model)
        {
            DisplayValue = model is T value ? value : default;
        }
    }

    /// <summary>
    ///     For internal use only, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
    /// </summary>
    public abstract class DataModelDisplayViewModel : PropertyChangedBase
    {
        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="DataModelDisplayViewModel{T}" /> instead.
        /// </summary>
        internal abstract void UpdateValue(object model);
    }
}