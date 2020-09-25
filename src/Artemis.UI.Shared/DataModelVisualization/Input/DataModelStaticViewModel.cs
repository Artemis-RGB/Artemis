using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared.Input
{
    public class DataModelStaticViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly Window _rootView;
        private Brush _buttonBrush = new SolidColorBrush(Color.FromRgb(171, 71, 188));
        private DataModelDisplayViewModel _displayViewModel;
        private DataModelInputViewModel _inputViewModel;
        private string _placeholder = "Enter a value";
        private DataModelPropertyAttribute _targetDescription;
        private Type _targetType;
        private object _value;
        private bool _isEnabled;

        internal DataModelStaticViewModel(Type targetType, IDataModelUIService dataModelUIService)
        {
            _dataModelUIService = dataModelUIService;
            _rootView = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            TargetType = targetType;
            IsEnabled = TargetType != null;
            DisplayViewModel = _dataModelUIService.GetDataModelDisplayViewModel(TargetType ?? typeof(object), true);
            
            if (_rootView != null)
            {
                _rootView.MouseUp += RootViewOnMouseUp;
                _rootView.KeyUp += RootViewOnKeyUp;
            }
        }

        public Brush ButtonBrush
        {
            get => _buttonBrush;
            set => SetAndNotify(ref _buttonBrush, value);
        }
        
        public DataModelDisplayViewModel DisplayViewModel
        {
            get => _displayViewModel;
            set => SetAndNotify(ref _displayViewModel, value);
        }

        public DataModelInputViewModel InputViewModel
        {
            get => _inputViewModel;
            private set => SetAndNotify(ref _inputViewModel, value);
        }

        public Type TargetType
        {
            get => _targetType;
            set => SetAndNotify(ref _targetType, value);
        }

        public DataModelPropertyAttribute TargetDescription
        {
            get => _targetDescription;
            set => SetAndNotify(ref _targetDescription, value);
        }

        public object Value
        {
            get => _value;
            set
            {
                if (!SetAndNotify(ref _value, value)) return;
                DisplayViewModel?.UpdateValue(_value);
            }
        }

        public string Placeholder
        {
            get => _placeholder;
            set => SetAndNotify(ref _placeholder, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            private set => SetAndNotify(ref _isEnabled, value);
        }

        #region IDisposable

        public void Dispose()
        {
            if (_rootView != null)
            {
                _rootView.MouseUp -= RootViewOnMouseUp;
                _rootView.KeyUp -= RootViewOnKeyUp;
            }
        }

        #endregion

        public void ActivateInputViewModel()
        {
            InputViewModel = _dataModelUIService.GetDataModelInputViewModel(
                TargetType,
                TargetDescription,
                Value,
                ApplyFreeInput
            );
        }

        public void UpdateTargetType(Type target)
        {
            TargetType = target;
            DisplayViewModel = _dataModelUIService.GetDataModelDisplayViewModel(TargetType ?? typeof(object), true);
            IsEnabled = TargetType != null;

            // If null, clear the input
            if (TargetType == null)
            {
                ApplyFreeInput(null, true);
                return;
            }
            
            // If the type changed, reset to the default type
            if (Value == null || !target.IsCastableFrom(Value.GetType()))
            {
                // Force the VM to close if it was open and apply the new value
                ApplyFreeInput(target.GetDefault(), true);
            }

        }
        
        private void ApplyFreeInput(object value, bool submitted)
        {
            if (submitted)
                OnValueUpdated(new DataModelInputStaticEventArgs(value));

            InputViewModel = null;
            Value = value;
        }

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

        public event EventHandler<DataModelInputStaticEventArgs> ValueUpdated;

        protected virtual void OnValueUpdated(DataModelInputStaticEventArgs e)
        {
            ValueUpdated?.Invoke(this, e);
        }

        #endregion
    }
}