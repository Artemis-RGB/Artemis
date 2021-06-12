using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using MaterialDesignColors.ColorManipulation;
using Stylet;

namespace Artemis.UI.Shared.Input
{
    /// <summary>
    ///     Represents a view model that allows inputting a static value used by boolean operations on a certain data model
    ///     property
    /// </summary>
    public class DataModelStaticViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly Window? _rootView;
        private SolidColorBrush _buttonBrush = new(Color.FromRgb(171, 71, 188));
        private bool _displaySwitchButton;
        private DataModelDisplayViewModel? _displayViewModel;
        private DataModelInputViewModel? _inputViewModel;
        private bool _isEnabled;
        private string _placeholder = "Enter a value";
        private DataModelPropertyAttribute _targetDescription;
        private Type _targetType;
        private object? _value;

        internal DataModelStaticViewModel(Type targetType, DataModelPropertyAttribute targetDescription, IDataModelUIService dataModelUIService)
        {
            _dataModelUIService = dataModelUIService;
            _rootView = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            _targetType = targetType;
            _targetDescription = targetDescription;
            _isEnabled = TargetType != null;
            _displayViewModel = _dataModelUIService.GetDataModelDisplayViewModel(TargetType ?? typeof(object), TargetDescription, true);

            if (_rootView != null)
            {
                _rootView.MouseUp += RootViewOnMouseUp;
                _rootView.KeyUp += RootViewOnKeyUp;
            }
        }

        /// <summary>
        ///     Gets or sets the brush to use for the input button
        /// </summary>
        public SolidColorBrush ButtonBrush
        {
            get => _buttonBrush;
            set
            {
                if (!SetAndNotify(ref _buttonBrush, value)) return;
                NotifyOfPropertyChange(nameof(SwitchButtonBrush));
            }
        }

        /// <summary>
        ///     Gets the brush to use for the switch button
        /// </summary>
        public SolidColorBrush SwitchButtonBrush => new(ButtonBrush.Color.Darken());

        /// <summary>
        ///     Gets the view model used to display the value
        /// </summary>
        public DataModelDisplayViewModel? DisplayViewModel
        {
            get => _displayViewModel;
            private set => SetAndNotify(ref _displayViewModel, value);
        }

        /// <summary>
        ///     Gets the view model used to edit the value
        /// </summary>
        public DataModelInputViewModel? InputViewModel
        {
            get => _inputViewModel;
            private set => SetAndNotify(ref _inputViewModel, value);
        }

        /// <summary>
        ///     Gets the type of the target property
        /// </summary>
        public Type TargetType
        {
            get => _targetType;
            private set => SetAndNotify(ref _targetType, value);
        }

        /// <summary>
        ///     Gets the description of the target property
        /// </summary>
        public DataModelPropertyAttribute TargetDescription
        {
            get => _targetDescription;
            set => SetAndNotify(ref _targetDescription, value);
        }

        /// <summary>
        ///     Gets or sets the value of the target
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                if (!SetAndNotify(ref _value, value)) return;
                DisplayViewModel?.UpdateValue(_value);
            }
        }

        /// <summary>
        ///     Gets or sets the placeholder text when no value is entered
        /// </summary>
        public string Placeholder
        {
            get => _placeholder;
            set => SetAndNotify(ref _placeholder, value);
        }

        /// <summary>
        ///     Gets or sets the enabled state of the input
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            private set => SetAndNotify(ref _isEnabled, value);
        }

        /// <summary>
        ///     Gets or sets whether the switch button should be displayed
        /// </summary>
        public bool DisplaySwitchButton
        {
            get => _displaySwitchButton;
            set => SetAndNotify(ref _displaySwitchButton, value);
        }

        /// <summary>
        ///     Activates the input view model
        /// </summary>
        public void ActivateInputViewModel()
        {
            InputViewModel = _dataModelUIService.GetDataModelInputViewModel(
                TargetType,
                TargetDescription,
                Value,
                ApplyFreeInput
            );
        }

        /// <summary>
        ///     Updates the target type
        /// </summary>
        /// <param name="target">The new target type</param>
        public void UpdateTargetType(Type target)
        {
            TargetType = target;
            DisplayViewModel = _dataModelUIService.GetDataModelDisplayViewModel(TargetType ?? typeof(object), TargetDescription, true);
            IsEnabled = TargetType != null;

            // If null, clear the input
            if (TargetType == null)
            {
                ApplyFreeInput(null, true);
                return;
            }

            // If the type changed, reset to the default type
            if (Value == null || !TargetType.IsCastableFrom(Value.GetType()))
                // Force the VM to close if it was open and apply the new value
                ApplyFreeInput(TargetType.GetDefault(), true);
        }

        /// <summary>
        ///     Requests switching the input type to dynamic using a <see cref="DataModelDynamicViewModel"/>
        /// </summary>
        public void SwitchToDynamic()
        {
            InputViewModel?.Cancel();
            ApplyFreeInput(TargetType.GetDefault(), true);

            OnSwitchToDynamicRequested();
        }

        private void ApplyFreeInput(object? value, bool submitted)
        {
            if (submitted)
                OnValueUpdated(new DataModelInputStaticEventArgs(value));

            InputViewModel = null;
            Value = value;
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_rootView != null)
                {
                    _rootView.MouseUp -= RootViewOnMouseUp;
                    _rootView.KeyUp -= RootViewOnKeyUp;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


        #region Event handlers

        private void RootViewOnKeyUp(object sender, KeyEventArgs e)
        {
            if (InputViewModel == null)
                return;

            if (e.Key == Key.Escape)
                InputViewModel.Cancel();
            else if (e.Key == Key.Enter)
                InputViewModel.Submit();
        }

        private void RootViewOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (InputViewModel == null)
                return;

            if (sender is FrameworkElement frameworkElement && !frameworkElement.IsDescendantOf(InputViewModel.View))
                InputViewModel.Submit();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the value of the property has been updated
        /// </summary>
        public event EventHandler<DataModelInputStaticEventArgs>? ValueUpdated;

        /// <summary>
        ///     Occurs when a switch to dynamic input has been requested
        /// </summary>
        public event EventHandler? SwitchToDynamicRequested;

        /// <summary>
        ///     Invokes the <see cref="ValueUpdated" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueUpdated(DataModelInputStaticEventArgs e)
        {
            ValueUpdated?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="SwitchToDynamicRequested" /> event
        /// </summary>
        protected virtual void OnSwitchToDynamicRequested()
        {
            SwitchToDynamicRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}