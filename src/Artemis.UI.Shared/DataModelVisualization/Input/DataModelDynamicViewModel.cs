using System;
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

// Remove, annoying while working on it
#pragma warning disable 1591

namespace Artemis.UI.Shared.Input
{
    public class DataModelDynamicViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly Module _module;
        private readonly Timer _updateTimer;
        private SolidColorBrush _buttonBrush = new SolidColorBrush(Color.FromRgb(171, 71, 188));
        private DataModelPath _dataModelPath;
        private DataModelPropertiesViewModel _dataModelViewModel;
        private bool _displaySwitchButton;
        private Type[] _filterTypes;
        private bool _isDataModelViewModelOpen;
        private bool _isEnabled = true;
        private string _placeholder = "Select a property";

        internal DataModelDynamicViewModel(Module module, ISettingsService settingsService, IDataModelUIService dataModelUIService)
        {
            _module = module;
            _dataModelUIService = dataModelUIService;
            _updateTimer = new Timer(500);

            ExtraDataModelViewModels = new BindableCollection<DataModelPropertiesViewModel>();
            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");
            SelectPropertyCommand = new DelegateCommand(ExecuteSelectPropertyCommand);

            Initialize();
        }

        public SolidColorBrush ButtonBrush
        {
            get => _buttonBrush;
            set
            {
                if (!SetAndNotify(ref _buttonBrush, value)) return;
                NotifyOfPropertyChange(nameof(SwitchButtonBrush));
            }
        }

        public SolidColorBrush SwitchButtonBrush => new SolidColorBrush(ButtonBrush.Color.Darken());

        public string Placeholder
        {
            get => _placeholder;
            set => SetAndNotify(ref _placeholder, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetAndNotify(ref _isEnabled, value);
        }

        public bool DisplaySwitchButton
        {
            get => _displaySwitchButton;
            set => SetAndNotify(ref _displaySwitchButton, value);
        }

        public Type[] FilterTypes
        {
            get => _filterTypes;
            set
            {
                if (!SetAndNotify(ref _filterTypes, value)) return;
                DataModelViewModel?.ApplyTypeFilter(true, FilterTypes);
            }
        }

        public BindableCollection<DataModelPropertiesViewModel> ExtraDataModelViewModels { get; }
        public bool HasExtraDataModels => ExtraDataModelViewModels.Any();

        public DelegateCommand SelectPropertyCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }

        public DataModelPropertiesViewModel DataModelViewModel
        {
            get => _dataModelViewModel;
            private set => SetAndNotify(ref _dataModelViewModel, value);
        }

        public bool IsDataModelViewModelOpen
        {
            get => _isDataModelViewModelOpen;
            set => SetAndNotify(ref _isDataModelViewModelOpen, value);
        }

        public DataModelPath DataModelPath
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

        public bool IsValid => DataModelPath?.IsValid ?? true;
        public string DisplayValue => DataModelPath?.GetPropertyDescription()?.Name ?? DataModelPath?.Segments.LastOrDefault()?.Identifier;

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

        public void ChangeDataModel(DataModelPropertiesViewModel dataModel)
        {
            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested -= DataModelOnUpdateRequested;

            DataModelViewModel = dataModel;

            if (DataModelViewModel != null)
                DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
        }

        public void ChangeDataModelPath(DataModelPath dataModelPath)
        {
            DataModelPath?.Dispose();
            DataModelPath = dataModelPath != null ? new DataModelPath(dataModelPath) : null;
        }

        public void SwitchToStatic()
        {
            ChangeDataModelPath(null);
            OnPropertySelected(new DataModelInputDynamicEventArgs(null));
            OnSwitchToStaticRequested();
        }

        private void Initialize()
        {
            // Get the data models
            DataModelViewModel = _dataModelUIService.GetPluginDataModelVisualization(_module, true);
            DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
            ExtraDataModelViewModels.CollectionChanged += ExtraDataModelViewModelsOnCollectionChanged;
            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void ExecuteSelectPropertyCommand(object context)
        {
            if (!(context is DataModelVisualizationViewModel selected))
                return;

            ChangeDataModelPath(selected.DataModelPath);
            OnPropertySelected(new DataModelInputDynamicEventArgs(DataModelPath));
        }

        public void Dispose()
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;

            DataModelPath?.Dispose();
        }

        #region Event handlers

        private void DataModelOnUpdateRequested(object sender, EventArgs e)
        {
            DataModelViewModel.ApplyTypeFilter(true, FilterTypes);
            foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
                extraDataModelViewModel.ApplyTypeFilter(true, FilterTypes);
        }

        private void ExtraDataModelViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasExtraDataModels));
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsDataModelViewModelOpen)
                return;

            DataModelViewModel.Update(_dataModelUIService);
            foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
                extraDataModelViewModel.Update(_dataModelUIService);
        }

        #endregion

        #region Events

        public event EventHandler<DataModelInputDynamicEventArgs> PropertySelected;
        public event EventHandler SwitchToStaticRequested;

        protected virtual void OnPropertySelected(DataModelInputDynamicEventArgs e)
        {
            PropertySelected?.Invoke(this, e);
        }

        protected virtual void OnSwitchToStaticRequested()
        {
            SwitchToStaticRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}