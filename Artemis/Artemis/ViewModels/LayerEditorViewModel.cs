using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.KeyboardGif;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles;
using Artemis.ViewModels.Profiles.Events;
using Caliburn.Micro;
using Newtonsoft.Json;
using Ninject;

namespace Artemis.ViewModels
{
    public sealed class LayerEditorViewModel : Screen
    {
        private EventPropertiesViewModel _eventPropertiesViewModel;
        private LayerModel _layer;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private LayerModel _proposedLayer;
        private ILayerType _selectedLayerType;

        public LayerEditorViewModel(LayerModel layer, ModuleDataModel dataModel, IEnumerable<ILayerType> types,
            IEnumerable<ILayerAnimation> layerAnimations)
        {
            Layer = layer;
            ProposedLayer = GeneralHelpers.Clone(layer);
            ProposedLayer.Children.Clear();
            DataModel = DataModel;
            LayerTypes = new BindableCollection<ILayerType>(types.OrderBy(t => t.Name));
            LayerAnimations = layerAnimations.OrderBy(l => l.Name).ToList();

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>(GeneralHelpers.GenerateTypeMap(dataModel));

            if (Layer.Properties == null)
                Layer.SetupProperties();

            // Setup existing conditions   
            var conditions = ProposedLayer.Properties.Conditions.Select(c => new LayerConditionViewModel(this, c));
            var keyBinds = ProposedLayer.Properties.LayerKeybindModels.Select(c => new LayerKeybindViewModel(this, c));
            LayerConditionVms = new BindableCollection<LayerConditionViewModel>(conditions);
            LayerKeybindVms = new BindableCollection<LayerKeybindViewModel>(keyBinds);

            PropertyChanged += PropertiesViewModelHandler;

            // Setup existiing properties
            PreSelect();
        }

        public object DataModel { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public BindableCollection<ILayerType> LayerTypes { get; set; }
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<LayerConditionViewModel> LayerConditionVms { get; set; }
        public BindableCollection<LayerKeybindViewModel> LayerKeybindVms { get; set; }
        public bool KeyboardGridIsVisible => ProposedLayer.LayerType is KeyboardType;
        public bool GifGridIsVisible => ProposedLayer.LayerType is KeyboardGifType;

        public LayerModel Layer
        {
            get { return _layer; }
            set
            {
                if (Equals(value, _layer))
                    return;
                _layer = value;
                NotifyOfPropertyChange(() => Layer);
            }
        }

        public List<ILayerAnimation> LayerAnimations { get; set; }

        public LayerModel ProposedLayer
        {
            get { return _proposedLayer; }
            set
            {
                if (Equals(value, _proposedLayer))
                    return;
                _proposedLayer = value;
                NotifyOfPropertyChange(() => ProposedLayer);
            }
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel
        {
            get { return _layerPropertiesViewModel; }
            set
            {
                if (Equals(value, _layerPropertiesViewModel))
                    return;
                _layerPropertiesViewModel = value;
                NotifyOfPropertyChange(() => LayerPropertiesViewModel);
            }
        }

        public EventPropertiesViewModel EventPropertiesViewModel
        {
            get { return _eventPropertiesViewModel; }
            set
            {
                if (Equals(value, _eventPropertiesViewModel))
                    return;
                _eventPropertiesViewModel = value;
                NotifyOfPropertyChange(() => EventPropertiesViewModel);
            }
        }

        public ILayerType SelectedLayerType
        {
            get { return _selectedLayerType; }
            set
            {
                if (Equals(value, _selectedLayerType))
                    return;
                _selectedLayerType = value;
                NotifyOfPropertyChange(() => SelectedLayerType);
            }
        }

        public void PreSelect()
        {
            SelectedLayerType = LayerTypes.FirstOrDefault(t => t.Name == ProposedLayer.LayerType.Name);
            ToggleIsEvent();
        }

        private void PropertiesViewModelHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedLayerType")
                return;

            // Store the brush in case the user wants to reuse it
            var oldBrush = ProposedLayer.Properties.Brush;

            // Update the model
            if (ProposedLayer.LayerType.GetType() != SelectedLayerType.GetType())
            {
                ProposedLayer.LayerType = SelectedLayerType;
                ProposedLayer.SetupProperties();
            }

            // Let the layer type handle the viewmodel setup
            LayerPropertiesViewModel = ProposedLayer.LayerType.SetupViewModel(this, LayerPropertiesViewModel);

            if (oldBrush != null)
                ProposedLayer.Properties.Brush = oldBrush;

            NotifyOfPropertyChange(() => LayerPropertiesViewModel);
        }

        public void ToggleIsEvent()
        {
            EventPropertiesViewModel = ProposedLayer.IsEvent
                ? new EventPropertiesViewModel(Layer.EventProperties)
                : null;

            // The form inside each condition VM changes upon event toggle
            foreach (var layerConditionViewModel in LayerConditionVms)
                layerConditionViewModel.SetupPropertyInput();
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            LayerConditionVms.Add(new LayerConditionViewModel(this, condition));
        }

        public void AddKeybind()
        {
            var keybind = new LayerKeybindModel();
            LayerKeybindVms.Add(new LayerKeybindViewModel(this, keybind));
        }

        public void Apply()
        {
            LayerPropertiesViewModel?.ApplyProperties();

            Layer.Properties.DynamicProperties.Clear();
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(ProposedLayer), Layer);

            Layer.Properties.Conditions.Clear();
            foreach (var conditionViewModel in LayerConditionVms)
                Layer.Properties.Conditions.Add(conditionViewModel.ConditionModel);

            Layer.Properties.LayerKeybindModels.Clear();
            foreach (var layerKeybindViewModel in LayerKeybindVms)
                Layer.Properties.LayerKeybindModels.Add(layerKeybindViewModel.LayerKeybindModel);
            

            // TODO: EventPropVM must have layer too
            if (EventPropertiesViewModel != null)
                Layer.EventProperties = EventPropertiesViewModel.GetAppliedProperties();

            Layer.SetupCondition();

            // Don't bother checking for a GIF path unless the type is GIF
            if (!(Layer.LayerType is KeyboardGifType))
                return;
            if (!File.Exists(((KeyboardPropertiesModel) Layer.Properties).GifFile))
                DialogService.ShowErrorMessageBox("Couldn't find or access the provided GIF file.");
        }

        public override async void CanClose(Action<bool> callback)
        {
            // Create a fake layer and apply the properties to it
            LayerPropertiesViewModel?.ApplyProperties();
            // TODO: EventPropVM must have layer too
            if (EventPropertiesViewModel != null)
                ProposedLayer.EventProperties = EventPropertiesViewModel.GetAppliedProperties();

            ProposedLayer.Properties.Conditions.Clear();
            foreach (var conditionViewModel in LayerConditionVms)
                ProposedLayer.Properties.Conditions.Add(conditionViewModel.ConditionModel);

            ProposedLayer.Properties.LayerKeybindModels.Clear();
            foreach (var layerKeybindViewModel in LayerKeybindVms)
                ProposedLayer.Properties.LayerKeybindModels.Add(layerKeybindViewModel.LayerKeybindModel);

            // Ignore this property as it isn't user input
            ProposedLayer.RenderAllowed = Layer.RenderAllowed;

            // If not a keyboard, ignore size and position
            if ((ProposedLayer.LayerType.DrawType != DrawType.Keyboard) || !ProposedLayer.LayerType.ShowInEdtor)
            {
                ProposedLayer.Properties.Width = Layer.Properties.Width;
                ProposedLayer.Properties.Height = Layer.Properties.Height;
                ProposedLayer.Properties.X = Layer.Properties.X;
                ProposedLayer.Properties.Y = Layer.Properties.Y;
                ProposedLayer.Properties.Contain = Layer.Properties.Contain;
            }

            // Ignore the children, can't just temporarily add them to the proposed layer because
            // that would upset the child layers' relations (sounds like Dr. Phil amirite?)
            var currentObj = GeneralHelpers.Clone(Layer);
            currentObj.Children.Clear();

            // Apply the IsEvent boolean
            currentObj.SetupCondition();
            ProposedLayer.SetupCondition();

            var current = JsonConvert.SerializeObject(currentObj, Formatting.Indented);
            var proposed = JsonConvert.SerializeObject(ProposedLayer, Formatting.Indented);

            if (current.Equals(proposed))
            {
                callback(true);
                return;
            }

            var close = await DialogService.ShowQuestionMessageBox("Unsaved changes", "Do you want to discard your changes?");
            callback(close.Value);
        }
    }
}
