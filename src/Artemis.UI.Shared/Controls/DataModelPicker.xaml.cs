using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared.Controls
{
    /// <summary>
    ///     Interaction logic for DataModelPicker.xaml
    /// </summary>
    public partial class DataModelPicker : INotifyPropertyChanged
    {
        private static IDataModelUIService _dataModelUIService;

        /// <summary>
        ///     Gets or sets data model path
        /// </summary>
        public static readonly DependencyProperty DataModelPathProperty = DependencyProperty.Register(
            nameof(DataModelPath), typeof(DataModelPath), typeof(DataModelPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DataModelPathPropertyChangedCallback)
        );

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(DataModelPicker),
            new FrameworkPropertyMetadata("Click to select")
        );

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public static readonly DependencyProperty ShowDataModelValuesProperty = DependencyProperty.Register(nameof(ShowDataModelValues), typeof(bool), typeof(DataModelPicker));

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public static readonly DependencyProperty ShowFullPathProperty = DependencyProperty.Register(nameof(ShowFullPath), typeof(bool), typeof(DataModelPicker));

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public static readonly DependencyProperty ButtonBrushProperty = DependencyProperty.Register(nameof(ButtonBrush), typeof(Brush), typeof(DataModelPicker));

        /// <summary>
        ///     A list of extra modules to show data models of
        /// </summary>
        public static readonly DependencyProperty ModulesProperty = DependencyProperty.Register(
            nameof(Modules), typeof(BindableCollection<Module>), typeof(DataModelPicker),
            new FrameworkPropertyMetadata(new BindableCollection<Module>(), FrameworkPropertyMetadataOptions.None, ModulesPropertyChangedCallback)
        );

        /// <summary>
        ///     The data model view model to show, if not provided one will be retrieved by the control
        /// </summary>
        public static readonly DependencyProperty DataModelViewModelProperty = DependencyProperty.Register(
            nameof(DataModelViewModel), typeof(DataModelPropertiesViewModel), typeof(DataModelPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, DataModelViewModelPropertyChangedCallback)
        );

        /// <summary>
        ///     A list of data model view models to show
        /// </summary>
        public static readonly DependencyProperty ExtraDataModelViewModelsProperty = DependencyProperty.Register(
            nameof(ExtraDataModelViewModels), typeof(BindableCollection<DataModelPropertiesViewModel>), typeof(DataModelPicker),
            new FrameworkPropertyMetadata(new BindableCollection<DataModelPropertiesViewModel>(), FrameworkPropertyMetadataOptions.None, ExtraDataModelViewModelsPropertyChangedCallback)
        );

        /// <summary>
        ///     A list of data model view models to show
        /// </summary>
        public static readonly DependencyProperty FilterTypesProperty = DependencyProperty.Register(
            nameof(FilterTypes), typeof(BindableCollection<Type>), typeof(DataModelPicker),
            new FrameworkPropertyMetadata(new BindableCollection<Type>())
        );

        public DataModelPicker()
        {
            SelectPropertyCommand = new DelegateCommand(ExecuteSelectPropertyCommand);

            InitializeComponent();
            GetDataModel();
            UpdateValueDisplay();
        }

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public DataModelPath? DataModelPath
        {
            get => (DataModelPath) GetValue(DataModelPathProperty);
            set => SetValue(DataModelPathProperty, value);
        }

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public string Placeholder
        {
            get => (string) GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public bool ShowDataModelValues
        {
            get => (bool) GetValue(ShowDataModelValuesProperty);
            set => SetValue(ShowDataModelValuesProperty, value);
        }

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public bool ShowFullPath
        {
            get => (bool) GetValue(ShowFullPathProperty);
            set => SetValue(ShowFullPathProperty, value);
        }

        /// <summary>
        ///     Gets or sets the brush to use when drawing the button
        /// </summary>
        public Brush ButtonBrush
        {
            get => (Brush) GetValue(ButtonBrushProperty);
            set => SetValue(ButtonBrushProperty, value);
        }

        /// <summary>
        ///     Gets or sets a list of extra modules to show data models of
        /// </summary>
        public BindableCollection<Module>? Modules
        {
            get => (BindableCollection<Module>) GetValue(ModulesProperty);
            set => SetValue(ModulesProperty, value);
        }

        /// <summary>
        ///     Gets or sets the data model view model to show
        /// </summary>
        public DataModelPropertiesViewModel? DataModelViewModel
        {
            get => (DataModelPropertiesViewModel) GetValue(DataModelViewModelProperty);
            set => SetValue(DataModelViewModelProperty, value);
        }

        /// <summary>
        ///     Gets or sets a list of data model view models to show
        /// </summary>
        public BindableCollection<DataModelPropertiesViewModel>? ExtraDataModelViewModels
        {
            get => (BindableCollection<DataModelPropertiesViewModel>) GetValue(ExtraDataModelViewModelsProperty);
            set => SetValue(ExtraDataModelViewModelsProperty, value);
        }

        /// <summary>
        ///     Gets or sets the types of properties this view model will allow to be selected
        /// </summary>
        public BindableCollection<Type>? FilterTypes
        {
            get => (BindableCollection<Type>) GetValue(FilterTypesProperty);
            set => SetValue(FilterTypesProperty, value);
        }

        /// <summary>
        ///     Command used by view
        /// </summary>
        public DelegateCommand SelectPropertyCommand { get; }

        internal static IDataModelUIService DataModelUIService
        {
            set
            {
                if (_dataModelUIService != null)
                    throw new AccessViolationException("This is not for you to touch");
                _dataModelUIService = value;
            }
        }

        /// <summary>
        ///     Occurs when a new path has been selected
        /// </summary>
        public event EventHandler<DataModelSelectedEventArgs>? DataModelPathSelected;

        /// <summary>
        ///     Invokes the <see cref="DataModelPathSelected" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataModelPathSelected(DataModelSelectedEventArgs e)
        {
            DataModelPathSelected?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetDataModel()
        {
            ChangeDataModel(_dataModelUIService.GetPluginDataModelVisualization(Modules?.ToList() ?? new List<Module>(), true));
        }

        private void ChangeDataModel(DataModelPropertiesViewModel? dataModel)
        {
            if (DataModelViewModel != null)
            {
                DataModelViewModel.Dispose();
                DataModelViewModel.UpdateRequested -= DataModelOnUpdateRequested;
            }

            DataModelViewModel = dataModel;
            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
        }

        private void UpdateValueDisplay()
        {
            ValueDisplay.Visibility = DataModelPath == null || DataModelPath.IsValid ? Visibility.Visible : Visibility.Collapsed;
            ValuePlaceholder.Visibility = DataModelPath == null || DataModelPath.IsValid ? Visibility.Collapsed : Visibility.Visible;

            string? formattedPath = null;
            if (DataModelPath != null && DataModelPath.IsValid)
                formattedPath = string.Join(" › ", DataModelPath.Segments.Where(s => s.GetPropertyDescription() != null).Select(s => s.GetPropertyDescription()!.Name));

            DataModelButton.ToolTip = formattedPath;
            ValueDisplayTextBlock.Text = ShowFullPath
                ? formattedPath
                : DataModelPath?.Segments.LastOrDefault()?.GetPropertyDescription()?.Name ?? DataModelPath?.Segments.LastOrDefault()?.Identifier;
        }

        private void DataModelOnUpdateRequested(object? sender, EventArgs e)
        {
            DataModelViewModel?.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
            if (ExtraDataModelViewModels == null) return;
            foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
                extraDataModelViewModel.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
        }

        private void ExecuteSelectPropertyCommand(object? context)
        {
            if (context is not DataModelVisualizationViewModel selected)
                return;

            DataModelPath = selected.DataModelPath;
            OnDataModelPathSelected(new DataModelSelectedEventArgs(DataModelPath));
        }

        private void PropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            // DataContext is not set when using left button, I don't know why but there it is
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
        }

        private static void DataModelPathPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataModelPicker dataModelPicker)
                return;

            if (e.OldValue is DataModelPath oldPath)
                oldPath.Dispose();

            dataModelPicker.UpdateValueDisplay();
        }

        private static void ModulesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataModelPicker dataModelPicker)
                return;

            dataModelPicker.GetDataModel();
        }

        private static void DataModelViewModelPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataModelPicker dataModelPicker)
                return;
        }

        private static void ExtraDataModelViewModelsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataModelPicker dataModelPicker)
                return;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}