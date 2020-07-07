using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

        /// <inheritdoc />
        public sealed override void Submit()
        {
            foreach (var sourceUpdatingBinding in BindingOperations.GetSourceUpdatingBindings(View))
                sourceUpdatingBinding.UpdateSource();

            OnSubmit();
            UpdateCallback(InputValue, true);
        }

        /// <inheritdoc />
        public sealed  override void Cancel()
        {
            OnCancel();
            UpdateCallback(InputValue, false);
        }
    }

    /// <summary>
    ///     For internal use only, implement <see cref="DataModelInputViewModel{T}" /> instead.
    /// </summary>
    public abstract class DataModelInputViewModel : PropertyChangedBase, IViewAware
    {
        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="DataModelInputViewModel{T}" /> instead.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        internal abstract object InternalGuard { get; }

        internal Action<object, bool> UpdateCallback { get; set; }

        public void AttachView(UIElement view)
        {
            if (View != null)
                throw new InvalidOperationException(string.Format("Tried to attach View {0} to ViewModel {1}, but it already has a view attached", view.GetType().Name, GetType().Name));

            View = view;

            // After the animation finishes attempt to focus the input field
            Task.Run(async () =>
            {
                await Task.Delay(400);
                await Execute.OnUIThreadAsync(() => View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)));
            });
        }

        public UIElement View { get; set; }

        /// <summary>
        ///     Submits the input value and removes this view model
        /// </summary>
        public abstract void Submit();

        /// <summary>
        ///     Discards changes to the input value and removes this view model
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        ///     Called before the current value is submitted
        /// </summary>
        protected virtual void OnSubmit()
        {
        }

        /// <summary>
        ///     Called before the current value is discarded
        /// </summary>
        protected virtual void OnCancel()
        {
        }
    }
}