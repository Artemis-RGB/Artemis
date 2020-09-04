using System;
using System.Timers;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet;

// Remove, annoying while working on it
#pragma warning disable 1591

namespace Artemis.UI.Shared
{
    public class DataModelSelectionViewModel : PropertyChangedBase
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly Module _module;
        private readonly Timer _updateTimer;
        private Brush _buttonBrush = new SolidColorBrush(Color.FromRgb(171, 71, 188));

        private DataModelPropertiesViewModel _dataModelViewModel;
        private bool _isDataModelViewModelOpen;
        private bool _isEnabled;
        private string _placeholder = "Select a property";
        private DataModelVisualizationViewModel _selectedPropertyViewModel;

        public DataModelSelectionViewModel(Module module, ISettingsService settingsService, IDataModelUIService dataModelUIService)
        {
            _module = module;
            _dataModelUIService = dataModelUIService;
            _updateTimer = new Timer(500);

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");
            SelectPropertyCommand = new DelegateCommand(ExecuteSelectPropertyCommand);

            Initialize();
        }
        
        public string Placeholder
        {
            get => _placeholder;
            set => SetAndNotify(ref _placeholder, value);
        }

        public Brush ButtonBrush
        {
            get => _buttonBrush;
            set => SetAndNotify(ref _buttonBrush, value);
        }

        public DelegateCommand SelectPropertyCommand { get; }
        public Type[] FilterTypes { get; set; }

        public PluginSetting<bool> ShowDataModelValues { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetAndNotify(ref _isEnabled, value);
        }

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

        public DataModelVisualizationViewModel SelectedPropertyViewModel
        {
            get => _selectedPropertyViewModel;
            private set => SetAndNotify(ref _selectedPropertyViewModel, value);
        }

        public void PopulateSelectedPropertyViewModel(DataModel datamodel, string path)
        {
            if (datamodel == null)
                SelectedPropertyViewModel = null;
            else
                SelectedPropertyViewModel = DataModelViewModel.GetChildByPath(datamodel.PluginInfo.Guid, path);
        }

        private void Initialize()
        {
            // Get the data models
            DataModelViewModel = _dataModelUIService.GetMainDataModelVisualization();
            if (!_dataModelUIService.GetPluginExtendsDataModel(_module))
                DataModelViewModel.Children.Add(_dataModelUIService.GetPluginDataModelVisualization(_module));

            DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void DataModelOnUpdateRequested(object sender, EventArgs e)
        {
            DataModelViewModel.ApplyTypeFilter(true, FilterTypes);
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsDataModelViewModelOpen)
                DataModelViewModel.Update(_dataModelUIService);
        }

        private void ExecuteSelectPropertyCommand(object context)
        {
            if (!(context is DataModelVisualizationViewModel selected))
                return;

            SelectedPropertyViewModel = selected;
            OnPropertySelected(new DataModelPropertySelectedEventArgs(selected));
        }

        #region Events

        public event EventHandler<DataModelPropertySelectedEventArgs> PropertySelected;

        protected virtual void OnPropertySelected(DataModelPropertySelectedEventArgs e)
        {
            PropertySelected?.Invoke(this, e);
        }

        #endregion
    }
}