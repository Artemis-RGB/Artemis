/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ColorBox
{
    [TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartSpinner, Type = typeof(Spinner))]
    public abstract class UpDownBase : Control, IValidateInput
    {
        #region Event Handlers

        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            if (AllowSpin && !IsReadOnly)
                OnSpin(e);
        }

        #endregion

        #region Members

        internal const string PartTextBox = "PART_TextBox";
        internal const string PartSpinner = "PART_Spinner";
        private bool _isSyncingTextAndValueProperties;
        private bool _isTextChangedFromUi;

        #endregion

        #region Properties

        internal Spinner Spinner { get; private set; }

        internal TextBox TextBox { get; private set; }

        #region CultureInfo

        public static readonly DependencyProperty CultureInfoProperty = DependencyProperty.Register("CultureInfo",
            typeof(CultureInfo), typeof(UpDownBase),
            new UIPropertyMetadata(CultureInfo.CurrentCulture, OnCultureInfoChanged));

        public CultureInfo CultureInfo
        {
            get { return (CultureInfo) GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }

        private static void OnCultureInfoChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var inputBase = o as UpDownBase;
            if (inputBase != null)
                inputBase.OnCultureInfoChanged((CultureInfo) e.OldValue, (CultureInfo) e.NewValue);
        }

        #endregion //CultureInfo

        #region IsReadOnly

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly",
            typeof(bool), typeof(UpDownBase), new UIPropertyMetadata(false, OnReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool) GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnReadOnlyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var inputBase = o as UpDownBase;
            if (inputBase != null)
                inputBase.OnReadOnlyChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        #endregion //IsReadOnly

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(UpDownBase),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged, null, false, UpdateSourceTrigger.LostFocus));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var inputBase = o as UpDownBase;
            if (inputBase != null)
                inputBase.OnTextChanged((string) e.OldValue, (string) e.NewValue);
        }

        #endregion //Text

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString",
            typeof(string), typeof(UpDownBase), new UIPropertyMetadata(string.Empty, OnFormatStringChanged));

        public string FormatString
        {
            get { return (string) GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = o as UpDownBase;
            if (numericUpDown != null)
                numericUpDown.OnFormatStringChanged((string) e.OldValue, (string) e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
                SyncTextAndValue(false, null);
        }

        #endregion //FormatString

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment",
            typeof(double?), typeof(UpDownBase),
            new PropertyMetadata(default(double), OnIncrementChanged, OnCoerceIncrement));

        public double? Increment
        {
            get { return (double?) GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        private static void OnIncrementChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = o as UpDownBase;
            if (numericUpDown != null)
                numericUpDown.OnIncrementChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnIncrementChanged(double oldValue, double newValue)
        {
            if (IsInitialized)
                SetValidSpinDirection();
        }

        private static object OnCoerceIncrement(DependencyObject d, object baseValue)
        {
            var numericUpDown = d as UpDownBase;
            if (numericUpDown != null)
                return numericUpDown.OnCoerceIncrement((double) baseValue);

            return baseValue;
        }

        protected virtual double? OnCoerceIncrement(double? baseValue)
        {
            return baseValue;
        }

        #endregion

        #region Maximum

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum",
            typeof(double), typeof(UpDownBase),
            new UIPropertyMetadata(default(double), OnMaximumChanged, OnCoerceMaximum));

        public double Maximum
        {
            get { return (double) GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = o as UpDownBase;
            if (numericUpDown != null)
                numericUpDown.OnMaximumChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnMaximumChanged(double oldValue, double newValue)
        {
            if (IsInitialized)
                SetValidSpinDirection();
        }

        private static object OnCoerceMaximum(DependencyObject d, object baseValue)
        {
            var numericUpDown = d as UpDownBase;
            if (numericUpDown != null)
                return numericUpDown.OnCoerceMaximum((double) baseValue);

            return baseValue;
        }

        protected virtual double OnCoerceMaximum(double baseValue)
        {
            return baseValue;
        }

        #endregion //Maximum

        #region Minimum

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum",
            typeof(double), typeof(UpDownBase),
            new UIPropertyMetadata(default(double), OnMinimumChanged, OnCoerceMinimum));

        public double Minimum
        {
            get { return (double) GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = o as UpDownBase;
            if (numericUpDown != null)
                numericUpDown.OnMinimumChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnMinimumChanged(double oldValue, double newValue)
        {
            if (IsInitialized)
                SetValidSpinDirection();
        }

        private static object OnCoerceMinimum(DependencyObject d, object baseValue)
        {
            var numericUpDown = d as UpDownBase;
            if (numericUpDown != null)
                return numericUpDown.OnCoerceMinimum((double) baseValue);

            return baseValue;
        }

        protected virtual double? OnCoerceMinimum(double baseValue)
        {
            return baseValue;
        }

        #endregion //Minimum        

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin",
            typeof(bool), typeof(UpDownBase), new UIPropertyMetadata(true));

        public bool AllowSpin
        {
            get { return (bool) GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region DefaultValue

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue",
            typeof(double?), typeof(UpDownBase), new UIPropertyMetadata(default(double), OnDefaultValueChanged));

        public double? DefaultValue
        {
            get { return (double?) GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        private static void OnDefaultValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ((UpDownBase) source).OnDefaultValueChanged((double) args.OldValue, (double) args.NewValue);
        }

        private void OnDefaultValueChanged(double oldValue, double newValue)
        {
            if (IsInitialized && string.IsNullOrEmpty(Text))
                SyncTextAndValue(true, Text);
        }

        #endregion //DefaultValue

        #region AllowInputSpecialValues

        private static readonly DependencyProperty AllowInputSpecialValuesProperty =
            DependencyProperty.Register("AllowInputSpecialValues", typeof(AllowedSpecialValues), typeof(UpDownBase),
                new UIPropertyMetadata(AllowedSpecialValues.None));

        private AllowedSpecialValues AllowInputSpecialValues
        {
            get { return (AllowedSpecialValues) GetValue(AllowInputSpecialValuesProperty); }
            set { SetValue(AllowInputSpecialValuesProperty, value); }
        }

        #endregion //AllowInputSpecialValues

        #region ParsingNumberStyle

        public static readonly DependencyProperty ParsingNumberStyleProperty =
            DependencyProperty.Register("ParsingNumberStyle", typeof(NumberStyles), typeof(UpDownBase),
                new UIPropertyMetadata(NumberStyles.Any));

        public NumberStyles ParsingNumberStyle
        {
            get { return (NumberStyles) GetValue(ParsingNumberStyleProperty); }
            set { SetValue(ParsingNumberStyleProperty, value); }
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double?),
            typeof(UpDownBase),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, OnCoerceValue, false, UpdateSourceTrigger.PropertyChanged));

        public double? Value
        {
            get { return (double?) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object basevalue)
        {
            return ((UpDownBase) o).OnCoerceValue(basevalue);
        }

        protected virtual object OnCoerceValue(object newValue)
        {
            return newValue;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var upDownBase = o as UpDownBase;
            if (upDownBase != null)
                upDownBase.OnValueChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
            if (IsInitialized)
                SyncTextAndValue(false, null);

            SetValidSpinDirection();

            var args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #endregion //Properties

        #region Base Class Overrides

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();

            base.OnAccessKey(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(PartTextBox) as TextBox;
            if (TextBox != null)
            {
                if (string.IsNullOrEmpty(Text))
                    TextBox.Text = "0.0";
                else
                    TextBox.Text = Text;

                TextBox.LostFocus += TextBox_LostFocus;
                TextBox.TextChanged += TextBox_TextChanged;
            }

            if (Spinner != null)
                Spinner.Spin -= OnSpinnerSpin;

            Spinner = GetTemplateChild(PartSpinner) as Spinner;

            if (Spinner != null)
                Spinner.Spin += OnSpinnerSpin;

            SetValidSpinDirection();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                {
                    if (AllowSpin && !IsReadOnly)
                        DoIncrement();
                    e.Handled = true;
                    break;
                }
                case Key.Down:
                {
                    if (AllowSpin && !IsReadOnly)
                        DoDecrement();
                    e.Handled = true;
                    break;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                {
                    var commitSuccess = CommitInput();
                    e.Handled = !commitSuccess;
                    break;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && AllowSpin && !IsReadOnly && TextBox.IsFocused)
            {
                if (e.Delta < 0)
                    DoDecrement();
                else if (0 < e.Delta)
                    DoIncrement();

                e.Handled = true;
            }
        }

        protected void OnTextChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
                SyncTextAndValue(true, Text);
        }

        protected void OnCultureInfoChanged(CultureInfo oldValue, CultureInfo newValue)
        {
            if (IsInitialized)
                SyncTextAndValue(false, null);
        }

        protected void OnReadOnlyChanged(bool oldValue, bool newValue)
        {
            SetValidSpinDirection();
        }

        public void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        #endregion

        #region Events

        public event InputValidationErrorEventHandler InputValidationError;

        #region ValueChanged Event

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(UpDownBase));

        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion

        #endregion //Events

        #region Methods

        private void DoDecrement()
        {
            if (Spinner == null)
                OnDecrement();
        }

        private void DoIncrement()
        {
            if (Spinner == null)
                OnIncrement();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _isTextChangedFromUi = true;
                Text = ((TextBox) sender).Text;
            }
            finally
            {
                _isTextChangedFromUi = false;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitInput();
        }

        private void RaiseInputValidationError(Exception e)
        {
            if (InputValidationError != null)
            {
                var args = new InputValidationErrorEventArgs(e);
                InputValidationError(this, args);
                if (args.ThrowException)
                    throw args.Exception;
            }
        }

        public bool CommitInput()
        {
            return SyncTextAndValue(true, Text);
        }

        protected bool SyncTextAndValue(bool updateValueFromText, string text)
        {
            if (_isSyncingTextAndValueProperties)
                return true;

            _isSyncingTextAndValueProperties = true;
            var parsedTextIsValid = true;
            try
            {
                if (updateValueFromText)
                    if (string.IsNullOrEmpty(text))
                        Value = DefaultValue;
                    else
                        try
                        {
                            Value = ConvertTextToValue(text);
                        }
                        catch (Exception e)
                        {
                            parsedTextIsValid = false;

                            // From the UI, just allow any input.
                            if (!_isTextChangedFromUi)
                                RaiseInputValidationError(e);
                        }

                // Do not touch the ongoing text input from user.
                if (!_isTextChangedFromUi)
                {
                    // Don't replace the empty Text with the non-empty representation of DefaultValue.
                    var shouldKeepEmpty = string.IsNullOrEmpty(Text) && Equals(Value, DefaultValue);
                    if (!shouldKeepEmpty)
                        Text = ConvertValueToText();

                    // Sync Text and textBox
                    if (TextBox != null)
                        if (string.IsNullOrEmpty(Text))
                            TextBox.Text = "0.0";
                        else
                            TextBox.Text = Text;
                }

                if (_isTextChangedFromUi && !parsedTextIsValid)
                {
                    //// Text input was made from the user and the text
                    //// repesents an invalid value. Disable the spinner
                    //// in this case.
                    if (Spinner != null)
                        Spinner.ValidSpinDirection = ValidSpinDirections.None;
                }
                else
                {
                    SetValidSpinDirection();
                }
            }
            finally
            {
                _isSyncingTextAndValueProperties = false;
            }
            return parsedTextIsValid;
        }

        protected static decimal ParsePercent(string text, IFormatProvider cultureInfo)
        {
            var info = NumberFormatInfo.GetInstance(cultureInfo);

            text = text.Replace(info.PercentSymbol, null);

            var result = decimal.Parse(text, NumberStyles.Any, info);
            result = result/100;

            return result;
        }

        #endregion

        #region Abstract

        protected abstract double? ConvertTextToValue(string text);
        protected abstract string ConvertValueToText();
        protected abstract void OnIncrement();
        protected abstract void OnDecrement();
        protected abstract void SetValidSpinDirection();

        #endregion
    }
}