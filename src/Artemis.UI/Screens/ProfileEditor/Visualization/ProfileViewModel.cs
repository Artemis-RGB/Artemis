using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Visualization.Tools;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public class ProfileViewModel : Screen, IProfileEditorPanelViewModel, IHandle<MainWindowFocusChangedEvent>, IHandle<MainWindowKeyEvent>
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileLayerVmFactory _profileLayerVmFactory;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IVisualizationToolVmFactory _visualizationToolVmFactory;

        private int _activeToolIndex;
        private VisualizationToolViewModel _activeToolViewModel;
        private bool _canApplyToLayer;
        private bool _canSelectEditTool;
        private BindableCollection<CanvasViewModel> _canvasViewModels;
        private BindableCollection<ArtemisDevice> _devices;
        private BindableCollection<ArtemisLed> _highlightedLeds;
        private PluginSetting<bool> _highlightSelectedLayer;
        private bool _isInitializing;
        private PluginSetting<bool> _onlyShowSelectedShape;
        private PanZoomViewModel _panZoomViewModel;
        private Layer _previousSelectedLayer;
        private int _previousTool;
        private BindableCollection<ArtemisLed> _selectedLeds;

        public ProfileViewModel(IProfileEditorService profileEditorService,
            ISurfaceService surfaceService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            IVisualizationToolVmFactory visualizationToolVmFactory,
            IProfileLayerVmFactory profileLayerVmFactory)
        {
            _profileEditorService = profileEditorService;
            _surfaceService = surfaceService;
            _settingsService = settingsService;
            _visualizationToolVmFactory = visualizationToolVmFactory;
            _profileLayerVmFactory = profileLayerVmFactory;

            Execute.OnUIThreadSync(() =>
            {
                PanZoomViewModel = new PanZoomViewModel {LimitToZero = false};

                CanvasViewModels = new BindableCollection<CanvasViewModel>();
                Devices = new BindableCollection<ArtemisDevice>();
                HighlightedLeds = new BindableCollection<ArtemisLed>();
                SelectedLeds = new BindableCollection<ArtemisLed>();
            });

            ApplySurfaceConfiguration(_surfaceService.ActiveSurface);
            ActivateToolByIndex(0);

            eventAggregator.Subscribe(this);
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            private set => SetAndNotify(ref _isInitializing, value);
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

        public BindableCollection<CanvasViewModel> CanvasViewModels
        {
            get => _canvasViewModels;
            set => SetAndNotify(ref _canvasViewModels, value);
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

        public BindableCollection<ArtemisLed> SelectedLeds
        {
            get => _selectedLeds;
            set => SetAndNotify(ref _selectedLeds, value);
        }

        public PluginSetting<bool> OnlyShowSelectedShape
        {
            get => _onlyShowSelectedShape;
            set => SetAndNotify(ref _onlyShowSelectedShape, value);
        }

        public PluginSetting<bool> HighlightSelectedLayer
        {
            get => _highlightSelectedLayer;
            set => SetAndNotify(ref _highlightSelectedLayer, value);
        }

        public VisualizationToolViewModel ActiveToolViewModel
        {
            get => _activeToolViewModel;
            private set
            {
                // Remove the tool from the canvas
                if (_activeToolViewModel != null)
                {
                    lock (CanvasViewModels)
                    {
                        CanvasViewModels.Remove(_activeToolViewModel);
                        NotifyOfPropertyChange(() => CanvasViewModels);
                    }
                }

                // Set the new tool
                SetAndNotify(ref _activeToolViewModel, value);
                // Add the new tool to the canvas
                if (_activeToolViewModel != null)
                {
                    lock (CanvasViewModels)
                    {
                        CanvasViewModels.Add(_activeToolViewModel);
                        NotifyOfPropertyChange(() => CanvasViewModels);
                    }
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

        public List<ArtemisLed> GetLedsInRectangle(Rect selectedRect)
        {
            return Devices.SelectMany(d => d.Leds)
                .Where(led => led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1).IntersectsWith(selectedRect))
                .ToList();
        }

        protected override void OnInitialActivate()
        {
            OnlyShowSelectedShape = _settingsService.GetSetting("ProfileEditor.OnlyShowSelectedShape", true);
            HighlightSelectedLayer = _settingsService.GetSetting("ProfileEditor.HighlightSelectedLayer", true);

            HighlightSelectedLayer.SettingChanged += HighlightSelectedLayerOnSettingChanged;
            _surfaceService.ActiveSurfaceConfigurationSelected += OnActiveSurfaceConfigurationSelected;
            _profileEditorService.ProfileSelected += OnProfileSelected;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            HighlightSelectedLayer.SettingChanged -= HighlightSelectedLayerOnSettingChanged;
            _surfaceService.ActiveSurfaceConfigurationSelected -= OnActiveSurfaceConfigurationSelected;
            _profileEditorService.ProfileSelected -= OnProfileSelected;
            _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;
            if (_previousSelectedLayer != null)
                _previousSelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;

            OnlyShowSelectedShape.Save();
            HighlightSelectedLayer.Save();

            foreach (var canvasViewModel in CanvasViewModels)
                canvasViewModel.Dispose();
            CanvasViewModels.Clear();

            base.OnClose();
        }

        protected override void OnActivate()
        {
            ApplyActiveProfile();
        }

        private void OnActiveSurfaceConfigurationSelected(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
        }

        private void ApplyActiveProfile()
        {
            var layerViewModels = CanvasViewModels.Where(vm => vm is ProfileLayerViewModel).Cast<ProfileLayerViewModel>().ToList();
            var layers = _profileEditorService.SelectedProfile?.GetAllLayers() ?? new List<Layer>();

            // Add new layers missing a VM
            foreach (var layer in layers)
            {
                if (layerViewModels.All(vm => vm.Layer != layer))
                    CanvasViewModels.Add(_profileLayerVmFactory.Create(layer, this));
            }

            // Remove layers that no longer exist
            var toRemove = layerViewModels.Where(vm => !layers.Contains(vm.Layer));
            foreach (var profileLayerViewModel in toRemove)
            {
                profileLayerViewModel.Dispose();
                CanvasViewModels.Remove(profileLayerViewModel);
            }
        }

        private void ApplySurfaceConfiguration(ArtemisSurface surface)
        {
            Devices.Clear();
            Devices.AddRange(surface.Devices.OrderBy(d => d.ZIndex));
        }

        private void UpdateLedsDimStatus()
        {
            HighlightedLeds.Clear();
            if (HighlightSelectedLayer.Value && _profileEditorService.SelectedProfileElement is Layer layer)
                HighlightedLeds.AddRange(layer.Leds);
        }

        private void UpdateCanSelectEditTool()
        {
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

        #region Buttons

        private void ActivateToolByIndex(int value)
        {
            // Consider using DI if dependencies start to add up
            switch (value)
            {
                case 0:
                    ActiveToolViewModel = _visualizationToolVmFactory.ViewpointMoveToolViewModel(this);
                    break;
                case 1:
                    ActiveToolViewModel = _visualizationToolVmFactory.EditToolViewModel(this);
                    break;
                case 2:
                    ActiveToolViewModel = _visualizationToolVmFactory.SelectionToolViewModel(this);
                    break;
                case 3:
                    ActiveToolViewModel = _visualizationToolVmFactory.SelectionRemoveToolViewModel(this);
                    break;
            }

            ActiveToolIndex = value;
        }

        public void ResetZoomAndPan()
        {
            PanZoomViewModel.Reset();
        }

        #endregion

        #region Mouse

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            ActiveToolViewModel?.MouseDown(sender, e);
        }

        public void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
            ActiveToolViewModel?.MouseUp(sender, e);
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

        #region Context menu actions

        public void CreateLayer()
        {
        }

        public void ApplyToLayer()
        {
            if (!(_profileEditorService.SelectedProfileElement is Layer layer))
                return;

            layer.ClearLeds();
            layer.AddLeds(SelectedLeds);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void SelectAll()
        {
            SelectedLeds.Clear();
            SelectedLeds.AddRange(Devices.SelectMany(d => d.Leds));
        }

        public void InverseSelection()
        {
            var current = SelectedLeds.ToList();
            SelectedLeds.Clear();
            SelectedLeds.AddRange(Devices.SelectMany(d => d.Leds).Except(current));
        }

        public void ClearSelection()
        {
            SelectedLeds.Clear();
        }

        #endregion

        #region Event handlers

        private void HighlightSelectedLayerOnSettingChanged(object sender, EventArgs e)
        {
            UpdateLedsDimStatus();
        }

        private void OnProfileSelected(object sender, EventArgs e)
        {
            ApplyActiveProfile();
        }

        private void OnProfileElementSelected(object sender, EventArgs e)
        {
            if (_previousSelectedLayer != null)
                _previousSelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;
            if (_profileEditorService.SelectedProfileElement is Layer layer)
            {
                _previousSelectedLayer = layer;
                _previousSelectedLayer.LayerBrushUpdated += SelectedLayerOnLayerBrushUpdated;
            }
            else
                _previousSelectedLayer = null;

            UpdateLedsDimStatus();
            UpdateCanSelectEditTool();
        }

        private void SelectedLayerOnLayerBrushUpdated(object sender, EventArgs e)
        {
            UpdateCanSelectEditTool();
        }

        private void OnSelectedProfileElementUpdated(object sender, EventArgs e)
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

        public void Handle(MainWindowFocusChangedEvent message)
        {
            // if (PauseRenderingOnFocusLoss == null || ScreenState != ScreenState.Active)
            //     return;
            //
            // try
            // {
            //     if (PauseRenderingOnFocusLoss.Value && !message.IsFocused)
            //         _updateTrigger.Stop();
            //     else if (PauseRenderingOnFocusLoss.Value && message.IsFocused)
            //         _updateTrigger.Start();
            // }
            // catch (NullReferenceException)
            // {
            //     // TODO: Remove when fixed in RGB.NET, or avoid double stopping
            // }
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