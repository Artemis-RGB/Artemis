using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Visualization.Tools;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Shared.Services;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public class ProfileViewModel : Conductor<CanvasViewModel>.Collection.AllActive, IProfileEditorPanelViewModel, IHandle<MainWindowKeyEvent>
    {
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileLayerVmFactory _profileLayerVmFactory;
        private readonly IRgbService _rgbService;
        private readonly ISettingsService _settingsService;
        private readonly IVisualizationToolVmFactory _visualizationToolVmFactory;

        private int _activeToolIndex;
        private VisualizationToolViewModel _activeToolViewModel;
        private bool _canApplyToLayer;
        private bool _canSelectEditTool;
        private BindableCollection<ArtemisDevice> _devices;
        private BindableCollection<ArtemisLed> _highlightedLeds;
        private DateTime _lastUpdate;
        private PanZoomViewModel _panZoomViewModel;
        private Layer _previousSelectedLayer;
        private int _previousTool;

        public ProfileViewModel(IProfileEditorService profileEditorService,
            IRgbService rgbService,
            ICoreService coreService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            IVisualizationToolVmFactory visualizationToolVmFactory,
            IProfileLayerVmFactory profileLayerVmFactory)
        {
            _profileEditorService = profileEditorService;
            _rgbService = rgbService;
            _coreService = coreService;
            _settingsService = settingsService;
            _visualizationToolVmFactory = visualizationToolVmFactory;
            _profileLayerVmFactory = profileLayerVmFactory;

            eventAggregator.Subscribe(this);
        }

        public bool CanSelectEditTool
        {
            get => _canSelectEditTool;
            set => SetAndNotify(ref _canSelectEditTool, value);
        }

        public PanZoomViewModel PanZoomViewModel
        {
            get => _panZoomViewModel;
            set => SetAndNotify(ref _panZoomViewModel, value);
        }

        public BindableCollection<ArtemisDevice> Devices
        {
            get => _devices;
            set => SetAndNotify(ref _devices, value);
        }

        public BindableCollection<ArtemisLed> HighlightedLeds
        {
            get => _highlightedLeds;
            set => SetAndNotify(ref _highlightedLeds, value);
        }

        public VisualizationToolViewModel ActiveToolViewModel
        {
            get => _activeToolViewModel;
            private set
            {
                // Remove the tool from the canvas
                if (_activeToolViewModel != null)
                    lock (Items)
                    {
                        Items.Remove(_activeToolViewModel);
                        NotifyOfPropertyChange(() => Items);
                    }

                // Set the new tool
                SetAndNotify(ref _activeToolViewModel, value);
                // Add the new tool to the canvas
                if (_activeToolViewModel != null)
                    lock (Items)
                    {
                        Items.Add(_activeToolViewModel);
                        NotifyOfPropertyChange(() => Items);
                    }
            }
        }

        public int ActiveToolIndex
        {
            get => _activeToolIndex;
            set
            {
                if (!SetAndNotify(ref _activeToolIndex, value)) return;
                ActivateToolByIndex(value);
            }
        }

        public bool CanApplyToLayer
        {
            get => _canApplyToLayer;
            set => SetAndNotify(ref _canApplyToLayer, value);
        }

        public bool SuspendedEditing => _profileEditorService.SuspendEditing;

        protected override void OnInitialActivate()
        {
            PanZoomViewModel = new PanZoomViewModel
            {
                LimitToZero = false,
                PanX = _settingsService.GetSetting("ProfileEditor.PanX", 0d).Value,
                PanY = _settingsService.GetSetting("ProfileEditor.PanY", 0d).Value,
                Zoom = _settingsService.GetSetting("ProfileEditor.Zoom", 0d).Value
            };

            Devices = new BindableCollection<ArtemisDevice>();
            HighlightedLeds = new BindableCollection<ArtemisLed>();

            ApplyDevices();
            ActivateToolByIndex(0);

            ApplyActiveProfile();

            _lastUpdate = DateTime.Now;
            _coreService.FrameRendered += OnFrameRendered;

            _rgbService.DeviceAdded += RgbServiceOnDevicesModified;
            _rgbService.DeviceRemoved += RgbServiceOnDevicesModified;
            _profileEditorService.SuspendEditingChanged += ProfileEditorServiceOnSuspendEditingChanged;
            _profileEditorService.SelectedProfileChanged += OnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementSaved += OnSelectedProfileElementSaved;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _coreService.FrameRendered -= OnFrameRendered;
            _rgbService.DeviceAdded -= RgbServiceOnDevicesModified;
            _rgbService.DeviceRemoved -= RgbServiceOnDevicesModified;
            _profileEditorService.SuspendEditingChanged -= ProfileEditorServiceOnSuspendEditingChanged;
            _profileEditorService.SelectedProfileChanged -= OnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged -= OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementSaved -= OnSelectedProfileElementSaved;
            if (_previousSelectedLayer != null)
                _previousSelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;

            _settingsService.GetSetting("ProfileEditor.PanX", 0d).Value = PanZoomViewModel.PanX;
            _settingsService.GetSetting("ProfileEditor.PanX", 0d).Save();
            _settingsService.GetSetting("ProfileEditor.PanY", 0d).Value = PanZoomViewModel.PanY;
            _settingsService.GetSetting("ProfileEditor.PanY", 0d).Save();
            _settingsService.GetSetting("ProfileEditor.Zoom", 0d).Value = PanZoomViewModel.Zoom;
            _settingsService.GetSetting("ProfileEditor.Zoom", 0d).Save();

            base.OnClose();
        }

        private void RgbServiceOnDevicesModified(object sender, DeviceEventArgs e)
        {
            ApplyDevices();
        }

        private void ApplyActiveProfile()
        {
            List<ProfileLayerViewModel> layerViewModels = Items.Where(vm => vm is ProfileLayerViewModel).Cast<ProfileLayerViewModel>().ToList();
            List<Layer> layers = _profileEditorService.SelectedProfile?.GetAllLayers() ?? new List<Layer>();

            // Add new layers missing a VM
            foreach (Layer layer in layers)
            {
                if (layerViewModels.All(vm => vm.Layer != layer))
                    Items.Add(_profileLayerVmFactory.Create(layer, PanZoomViewModel));
            }

            // Remove layers that no longer exist
            IEnumerable<ProfileLayerViewModel> toRemove = layerViewModels.Where(vm => !layers.Contains(vm.Layer));
            foreach (ProfileLayerViewModel profileLayerViewModel in toRemove)
                Items.Remove(profileLayerViewModel);
        }

        private void ApplyDevices()
        {
            Devices.Clear();
            Devices.AddRange(_rgbService.EnabledDevices.Where(d => d.IsEnabled).OrderBy(d => d.ZIndex));
        }

        private void UpdateLedsDimStatus()
        {
            HighlightedLeds.Clear();
            if (_profileEditorService.SelectedProfileElement is Layer layer)
                HighlightedLeds.AddRange(layer.Leds);
        }

        private void UpdateCanSelectEditTool()
        {
            if (SuspendedEditing)
            {
                CanApplyToLayer = false;
                CanSelectEditTool = false;
                return;
            }

            if (_profileEditorService.SelectedProfileElement is Layer layer)
            {
                CanApplyToLayer = true;
                CanSelectEditTool = (layer.LayerBrush == null || layer.LayerBrush.SupportsTransformation) && layer.Leds.Any();
            }
            else
            {
                CanApplyToLayer = false;
                CanSelectEditTool = false;
            }

            if (CanSelectEditTool == false && ActiveToolIndex == 1)
                ActivateToolByIndex(2);
        }

        private void ProfileEditorServiceOnSuspendEditingChanged(object? sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(SuspendedEditing));
        }

        #region Buttons

        private void ActivateToolByIndex(int value)
        {
            if (value == 1 && !CanSelectEditTool)
                return;

            switch (value)
            {
                case 0:
                    ActiveToolViewModel = _visualizationToolVmFactory.ViewpointMoveToolViewModel(PanZoomViewModel);
                    break;
                case 1:
                    ActiveToolViewModel = _visualizationToolVmFactory.EditToolViewModel(PanZoomViewModel);
                    break;
                case 2:
                    ActiveToolViewModel = _visualizationToolVmFactory.SelectionToolViewModel(PanZoomViewModel);
                    break;
                case 3:
                    ActiveToolViewModel = _visualizationToolVmFactory.SelectionRemoveToolViewModel(PanZoomViewModel);
                    break;
            }

            ActiveToolIndex = value;
        }

        public void ResetZoomAndPan()
        {
            // Create a rect surrounding all devices
            SKRect rect = new SKRect(
                Devices.Min(d => d.Rectangle.Left),
                Devices.Min(d => d.Rectangle.Top),
                Devices.Max(d => d.Rectangle.Right),
                Devices.Max(d => d.Rectangle.Bottom)
            );

            PanZoomViewModel.Reset(rect);
        }

        #endregion

        #region Mouse

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            ActiveToolViewModel?.MouseDown(sender, e);
            e.Handled = false;
        }

        public void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
            ActiveToolViewModel?.MouseUp(sender, e);
            e.Handled = false;
        }

        public void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            ActiveToolViewModel?.MouseMove(sender, e);
        }

        public void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            PanZoomViewModel.ProcessMouseScroll(sender, e);
            ActiveToolViewModel?.MouseWheel(sender, e);
        }

        #endregion

        #region Event handlers

        private void OnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            TimeSpan delta = DateTime.Now - _lastUpdate;
            _lastUpdate = DateTime.Now;

            if (!_settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", true).Value || _profileEditorService.SelectedProfile == null)
                return;

            foreach (IDataBindingRegistration dataBindingRegistration in _profileEditorService.SelectedProfile.GetAllFolders()
                .SelectMany(f => f.GetAllLayerProperties(), (f, p) => p)
                .SelectMany(p => p.GetAllDataBindingRegistrations()))
                dataBindingRegistration.GetDataBinding()?.UpdateWithDelta(delta);
            foreach (IDataBindingRegistration dataBindingRegistration in _profileEditorService.SelectedProfile.GetAllLayers()
                .SelectMany(f => f.GetAllLayerProperties(), (f, p) => p)
                .SelectMany(p => p.GetAllDataBindingRegistrations()))
                dataBindingRegistration.GetDataBinding()?.UpdateWithDelta(delta);

            // TODO: Only update when there are data bindings
            if (!_profileEditorService.Playing)
                _profileEditorService.UpdateProfilePreview();
        }

        private void HighlightSelectedLayerOnSettingChanged(object sender, EventArgs e)
        {
            UpdateLedsDimStatus();
        }

        private void OnSelectedProfileChanged(object sender, EventArgs e)
        {
            ApplyActiveProfile();
        }

        private void OnSelectedProfileElementChanged(object sender, EventArgs e)
        {
            if (_previousSelectedLayer != null)
            {
                _previousSelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;
                _previousSelectedLayer.Transform.LayerPropertyOnCurrentValueSet -= TransformValueChanged;
            }

            if (_profileEditorService.SelectedProfileElement is Layer layer)
            {
                _previousSelectedLayer = layer;
                _previousSelectedLayer.LayerBrushUpdated += SelectedLayerOnLayerBrushUpdated;
                _previousSelectedLayer.Transform.LayerPropertyOnCurrentValueSet += TransformValueChanged;
            }
            else
            {
                _previousSelectedLayer = null;
            }

            ApplyActiveProfile();
            UpdateLedsDimStatus();
            UpdateCanSelectEditTool();
        }

        private void SelectedLayerOnLayerBrushUpdated(object sender, EventArgs e)
        {
            UpdateCanSelectEditTool();
        }

        private void TransformValueChanged(object sender, LayerPropertyEventArgs e)
        {
            if (ActiveToolIndex != 1)
                ActivateToolByIndex(1);
        }

        private void OnSelectedProfileElementSaved(object sender, EventArgs e)
        {
            ApplyActiveProfile();
            UpdateLedsDimStatus();
            if (_profileEditorService.SelectedProfileElement is Layer layer)
            {
                CanApplyToLayer = true;
                CanSelectEditTool = layer.Leds.Any();
            }
            else
            {
                CanApplyToLayer = false;
                CanSelectEditTool = false;
            }

            if (CanSelectEditTool == false && ActiveToolIndex == 1)
                ActivateToolByIndex(2);
        }

        public void Handle(MainWindowKeyEvent message)
        {
            if (message.KeyDown)
            {
                if (ActiveToolIndex != 0)
                {
                    _previousTool = ActiveToolIndex;
                    if ((message.EventArgs.Key == Key.LeftCtrl || message.EventArgs.Key == Key.RightCtrl) && message.EventArgs.IsDown)
                        ActivateToolByIndex(0);
                }

                ActiveToolViewModel?.KeyDown(message.EventArgs);

                // If T is pressed while Ctrl is down, that makes it Ctrl+T > swap to transformation tool on Ctrl release
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (message.EventArgs.Key == Key.T)
                        _previousTool = 1;
                    else if (message.EventArgs.Key == Key.Q)
                        _previousTool = 2;
                    else if (message.EventArgs.Key == Key.W)
                        _previousTool = 3;
                }
            }
            else
            {
                if ((message.EventArgs.Key == Key.LeftCtrl || message.EventArgs.Key == Key.RightCtrl) && message.EventArgs.IsUp)
                    ActivateToolByIndex(_previousTool);

                ActiveToolViewModel?.KeyUp(message.EventArgs);
            }
        }

        #endregion
    }
}