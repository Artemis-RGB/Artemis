using System;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Stylet;

// Remove, annoying while working on it
#pragma warning disable 1591

namespace Artemis.UI.Shared.Input
{
    public class DataModelStaticViewModel : PropertyChangedBase
    {
        private readonly IDataModelUIService _dataModelUIService;
        private Brush _buttonBrush = new SolidColorBrush(Color.FromRgb(171, 71, 188));
        private DataModelInputViewModel _inputViewModel;
        private string _placeholder = "Enter a value";
        private DataModelPropertyAttribute _targetDescription;
        private Type _targetType;
        private int _transitionIndex;
        private object _value;

        internal DataModelStaticViewModel(Type targetType, IDataModelUIService dataModelUIService)
        {
            TargetType = targetType;
            _dataModelUIService = dataModelUIService;
        }

        public Brush ButtonBrush
        {
            get => _buttonBrush;
            set => SetAndNotify(ref _buttonBrush, value);
        }

        public int TransitionIndex
        {
            get => _transitionIndex;
            set => SetAndNotify(ref _transitionIndex, value);
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
            set => SetAndNotify(ref _value, value);
        }

        public string Placeholder
        {
            get => _placeholder;
            set => SetAndNotify(ref _placeholder, value);
        }

        public void ActivateInputViewModel()
        {
            TransitionIndex = 1;
            InputViewModel = _dataModelUIService.GetDataModelInputViewModel(
                TargetType,
                TargetDescription,
                Value,
                ApplyFreeInput
            );
        }

        public void UpdateTargetType(Type target)
        {
            TargetType = target ?? throw new ArgumentNullException(nameof(target));

            // If the type changed, reset to the default type
            if (!target.IsCastableFrom(Value.GetType()))
            {
                // Force the VM to close if it was open and apply the new value
                ApplyFreeInput(target.GetDefault(), true);
            }
        }

        private void ApplyFreeInput(object value, bool submitted)
        {
            if (submitted)
                OnValueUpdated(new DataModelInputStaticEventArgs(value));

            TransitionIndex = 0;
            InputViewModel = null;
            Value = value;
        }

        #region Events

        public event EventHandler<DataModelInputStaticEventArgs> ValueUpdated;

        protected virtual void OnValueUpdated(DataModelInputStaticEventArgs e)
        {
            ValueUpdated?.Invoke(this, e);
        }

        #endregion
    }
}