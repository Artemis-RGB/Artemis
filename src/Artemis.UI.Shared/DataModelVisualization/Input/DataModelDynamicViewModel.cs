using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignColors.ColorManipulation;
using Stylet;

namespace Artemis.UI.Shared.Input
{
    /// <summary>
    ///     Represents a view model that allows selecting a data model property used by boolean operations on a certain data
    ///     model property
    /// </summary>
    public class DataModelDynamicViewModel : PropertyChangedBase, IDisposable
    {
        private readonly List<Module> _modules;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly Timer _updateTimer;
        private SolidColorBrush _buttonBrush = new(Color.FromRgb(171, 71, 188));
        private DataModelPath? _dataModelPath;
        private DataModelPropertiesViewModel? _dataModelViewModel;
        private bool _displaySwitchButton;
        private Type[] _filterTypes = Array.Empty<Type>();
        private bool _isDataModelViewModelOpen;
        private bool _isEnabled = true;
        private string _placeholder = "Select a property";

        internal DataModelDynamicViewModel(List<Module> modules, ISettingsService settingsService, IDataModelUIService dataModelUIService)
        {
            _modules = modules;
            _dataModelUIService = dataModelUIService;
            _updateTimer = new Timer(500);

            ExtraDataModelViewModels = new BindableCollection<DataModelPropertiesViewModel>();
            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");
            SelectPropertyCommand = new DelegateCommand(ExecuteSelectPropertyCommand);

            Initialize();
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
            set => SetAndNotify(ref _isEnabled, value);
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
        ///     Gets or sets the types of properties this view model will allow to be selected
        /// </summary>
        public Type[] FilterTypes
        {
            get => _filterTypes;
            set
            {
                if (!SetAndNotify(ref _filterTypes, value)) return;
                DataModelViewModel?.ApplyTypeFilter(true, FilterTypes);
            }
        }

        /// <summary>
        ///     Gets a bindable collection of extra data model view models to show
        /// </summary>
        public BindableCollection<DataModelPropertiesViewModel> ExtraDataModelViewModels { get; }

        /// <summary>
        ///     Gets a boolean indicating whether there are any extra data models
        /// </summary>
        public bool HasExtraDataModels => ExtraDataModelViewModels.Any();

        /// <summary>
        ///     Command used by view
        /// </summary>
        public DelegateCommand SelectPropertyCommand { get; }

        /// <summary>
        ///     Setting used by view
        /// </summary>
        public PluginSetting<bool> ShowDataModelValues { get; }

        /// <summary>
        ///     Gets or sets root the data model view model
        /// </summary>
        public DataModelPropertiesViewModel? DataModelViewModel
        {
            get => _dataModelViewModel;
            private set => SetAndNotify(ref _dataModelViewModel, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the data model is open
        /// </summary>
        public bool IsDataModelViewModelOpen
        {
            get => _isDataModelViewModelOpen;
            set
            {
                if (!SetAndNotify(ref _isDataModelViewModelOpen, value)) return;
                if (value)
                {
                    UpdateDataModelVisualization();
                    if (DataModelViewModel != null)
                        OpenSelectedValue(DataModelViewModel);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the data model path of the currently selected data model property
        /// </summary>
        public DataModelPath? DataModelPath
        {
            get => _dataModelPath;
            private set
            {
                if (!SetAndNotify(ref _dataModelPath, value)) return;
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(DisplayValue));
                NotifyOfPropertyChange(nameof(DisplayPath));
            }
        }

        /// <summary>
        ///     Gets a boolean indicating whether the current selection is valid
        /// </summary>
        public bool IsValid => DataModelPath?.IsValid ?? true;

        /// <summary>
        ///     Gets the display name of the currently selected property
        /// </summary>
        public string? DisplayValue => DataModelPath?.GetPropertyDescription()?.Name ?? DataModelPath?.Segments.LastOrDefault()?.Identifier;

        /// <summary>
        ///     Gets the human readable path of the currently selected property
        /// </summary>
        public string DisplayPath
        {
            get
            {
                if (DataModelPath == null)
                    return "Click to select a property";
                if (!DataModelPath.IsValid)
                    return "Invalid path";
                return string.Join(" › ", DataModelPath.Segments.Select(s => s.GetPropertyDescription()?.Name ?? s.Identifier));
            }
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether event child VMs should be loaded, defaults to <see langword="true" />
        /// </summary>
        public bool LoadEventChildren { get; set; } = true;

        /// <summary>
        ///     Changes the root data model VM stored in <see cref="DataModelViewModel" /> to the provided
        ///     <paramref name="dataModel" />
        /// </summary>
        /// <param name="dataModel">The data model VM to set the new root data model to</param>
        public void ChangeDataModel(DataModelPropertiesViewModel dataModel)
        {
            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested -= DataModelOnUpdateRequested;

            DataModelViewModel = dataModel;

            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
        }

        /// <summary>
        ///     Changes the currently selected property by its path
        /// </summary>
        /// <param name="dataModelPath">The path of the property to set the selection to</param>
        public void ChangeDataModelPath(DataModelPath? dataModelPath)
        {
            DataModelPath?.Dispose();
            DataModelPath = dataModelPath != null ? new DataModelPath(dataModelPath) : null;
        }

        /// <summary>
        ///     Requests switching the input type to static using a <see cref="DataModelStaticViewModel" />
        /// </summary>
        public void SwitchToStatic()
        {
            ChangeDataModelPath(null);
            OnPropertySelected(new DataModelInputDynamicEventArgs(null));
            OnSwitchToStaticRequested();
        }

        private void Initialize()
        {
            // Get the data models
            DataModelViewModel = _dataModelUIService.GetPluginDataModelVisualization(_modules, true);
            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
            ExtraDataModelViewModels.CollectionChanged += ExtraDataModelViewModelsOnCollectionChanged;
            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void ExecuteSelectPropertyCommand(object? context)
        {
            if (context is not DataModelVisualizationViewModel selected)
                return;

            ChangeDataModelPath(selected.DataModelPath);
            OnPropertySelected(new DataModelInputDynamicEventArgs(DataModelPath));
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
                _updateTimer.Stop();
                _updateTimer.Dispose();
                _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;

                DataModelPath?.Dispose();
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

        private void DataModelOnUpdateRequested(object? sender, EventArgs e)
        {
            DataModelViewModel?.ApplyTypeFilter(true, FilterTypes);
            foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
                extraDataModelViewModel.ApplyTypeFilter(true, FilterTypes);
        }

        private void ExtraDataModelViewModelsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasExtraDataModels));
        }

        private void OnUpdateTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsDataModelViewModelOpen)
                return;

            UpdateDataModelVisualization();
        }

        private void UpdateDataModelVisualization()
        {
            DataModelViewModel?.Update(_dataModelUIService, new DataModelUpdateConfiguration(LoadEventChildren));
            foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
                extraDataModelViewModel.Update(_dataModelUIService, new DataModelUpdateConfiguration(LoadEventChildren));
        }

        private void OpenSelectedValue(DataModelVisualizationViewModel dataModelPropertiesViewModel)
        {
            if (DataModelPath == null)
                return;

            if (dataModelPropertiesViewModel.Children.Any(c => c.DataModelPath != null && DataModelPath.Path.StartsWith(c.DataModelPath.Path)))
            {
                dataModelPropertiesViewModel.IsVisualizationExpanded = true;
                foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in dataModelPropertiesViewModel.Children)
                {
                    OpenSelectedValue(dataModelVisualizationViewModel);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when anew property has been selected
        /// </summary>
        public event EventHandler<DataModelInputDynamicEventArgs>? PropertySelected;

        /// <summary>
        ///     Occurs when a switch to static input has been requested
        /// </summary>
        public event EventHandler? SwitchToStaticRequested;

        /// <summary>
        ///     Invokes the <see cref="PropertySelected" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertySelected(DataModelInputDynamicEventArgs e)
        {
            PropertySelected?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="SwitchToStaticRequested" /> event
        /// </summary>
        protected virtual void OnSwitchToStaticRequested()
        {
            SwitchToStaticRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}