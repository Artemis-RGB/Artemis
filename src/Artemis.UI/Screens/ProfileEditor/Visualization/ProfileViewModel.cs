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
    public class ProfileViewModel : Conductor<CanvasViewModel>.Collection.AllActive, IProfileEditorPanelViewModel, IHandle<MainWindowFocusChangedEvent>, IHandle<MainWindowKeyEvent>
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly ICoreService _coreService;
        private readonly IProfileLayerVmFactory _profileLayerVmFactory;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IVisualizationToolVmFactory _visualizationToolVmFactory;

        private int _activeToolIndex;
        private VisualizationToolViewModel _activeToolViewModel;
        private bool _canApplyToLayer;
        private bool _canSelectEditTool;
        private BindableCollection<ArtemisDevice> _devices;
        private BindableCollection<ArtemisLed> _highlightedLeds;
        private PluginSetting<bool> _highlightSelectedLayer;
        private PluginSetting<bool> _alwaysApplyDataBindings;
        private PanZoomViewModel _panZoomViewModel;
        private Layer _previousSelectedLayer;
        private int _previousTool;
        private DateTime _lastUpdate;

        public ProfileViewModel(IProfileEditorService profileEditorService,
            ICoreService coreService,
            ISurfaceService surfaceService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            IVisualizationToolVmFactory visualizationToolVmFactory,
            IProfileLayerVmFactory profileLayerVmFactory)
        {
            _profileEditorService = profileEditorService;
            _coreService = coreService;
            _surfaceService = surfaceService;
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

        public PluginSetting<bool> AlwaysApplyDataBindings
        {
            get => _alwaysApplyDataBindings;
            set => SetAndNotify(ref _alwaysApplyDataBindings, value);
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
                    lock (Items)
                    {
                        Items.Remove(_activeToolViewModel);
                        NotifyOfPropertyChange(() => Items);
                    }
                }

                // Set the new tool
                SetAndNotify(ref _activeToolViewModel, value);
                // Add the new tool to the canvas
                if (_activeToolViewModel != null)
                {
                    lock (Items)
                    {
                        Items.Add(_activeToolViewModel);
                        NotifyOfPropertyChange(() => Items);
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

        protected override void OnInitialActivate()
        {
            PanZoomViewModel = new PanZoomViewModel {LimitToZero = false};

            Devices = new BindableCollection<ArtemisDevice>();
            HighlightedLeds = new BindableCollection<ArtemisLed>();

            ApplySurfaceConfiguration(_surfaceService.ActiveSurface);
            ActivateToolByIndex(0);

            ApplyActiveProfile();

            AlwaysApplyDataBindings = _settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", true);
            HighlightSelectedLayer = _settingsService.GetSetting("ProfileEditor.HighlightSelectedLayer", true);

            _lastUpdate = DateTime.Now;
            _coreService.FrameRendered += OnFrameRendered;

            HighlightSelectedLayer.SettingChanged += HighlightSelectedLayerOnSettingChanged;
            _surfaceService.ActiveSurfaceConfigurationSelected += OnActiveSurfaceConfigurationSelected;
            _profileEditorService.ProfileSelected += OnProfileSelected;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _coreService.FrameRendered -= OnFrameRendered;
            HighlightSelectedLayer.SettingChanged -= HighlightSelectedLayerOnSettingChanged;
            _surfaceService.ActiveSurfaceConfigurationSelected -= OnActiveSurfaceConfigurationSelected;
            _profileEditorService.ProfileSelected -= OnProfileSelected;
            _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;
            if (_previousSelectedLayer != null)
                _previousSelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;

            AlwaysApplyDataBindings.Save();
            HighlightSelectedLayer.Save();

            base.OnClose();
        }

        private void OnActiveSurfaceConfigurationSelected(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
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

        private void ApplySurfaceConfiguration(ArtemisSurface surface)
        {
            Devices.Clear();
            Devices.AddRange(surface.Devices.Where(d => d.IsEnabled).OrderBy(d => d.ZIndex));
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

        #region Event handlers

        private void OnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            TimeSpan delta = DateTime.Now - _lastUpdate;
            _lastUpdate = DateTime.Now;

            if (!AlwaysApplyDataBindings.Value || _profileEditorService.SelectedProfile == null || _profileEditorService.Playing)
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
            _profileEditorService.UpdateProfilePreview();
        }

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
                _previousSelectedLayer = null;

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