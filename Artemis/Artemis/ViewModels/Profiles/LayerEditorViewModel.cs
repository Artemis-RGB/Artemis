﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Layers;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles.Events;
using Artemis.ViewModels.Profiles.Layers;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels.Profiles
{
    public sealed class LayerEditorViewModel : Screen
    {
        private readonly IDataModel _dataModel;
        private EventPropertiesViewModel _eventPropertiesViewModel;
        private LayerModel _layer;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private LayerType _layerType;
        private LayerModel _proposedLayer;

        public LayerEditorViewModel(IDataModel dataModel, LayerModel layer)
        {
            _dataModel = dataModel;

            Layer = layer;
            ProposedLayer = GeneralHelpers.Clone(layer);

            if (Layer.Properties == null)
                Layer.SetupProperties();

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap(dataModel));
            LayerConditionVms = new BindableCollection<LayerConditionViewModel>(layer.Properties.Conditions
                .Select(c => new LayerConditionViewModel(this, c, DataModelProps)));

            PropertyChanged += PropertiesViewModelHandler;

            PreSelect();
        }


        public bool ModelChanged { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public BindableCollection<string> LayerTypes => new BindableCollection<string>();

        public BindableCollection<LayerConditionViewModel> LayerConditionVms { get; set; }

        public LayerModel Layer
        {
            get { return _layer; }
            set
            {
                if (Equals(value, _layer)) return;
                _layer = value;
                NotifyOfPropertyChange(() => Layer);
            }
        }

        public LayerModel ProposedLayer
        {
            get { return _proposedLayer; }
            set
            {
                if (Equals(value, _proposedLayer)) return;
                _proposedLayer = value;
                NotifyOfPropertyChange(() => ProposedLayer);
            }
        }

        public LayerType LayerType
        {
            get { return _layerType; }
            set
            {
                if (value == _layerType) return;
                _layerType = value;
                NotifyOfPropertyChange(() => LayerType);
            }
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel
        {
            get { return _layerPropertiesViewModel; }
            set
            {
                if (Equals(value, _layerPropertiesViewModel)) return;
                _layerPropertiesViewModel = value;
                NotifyOfPropertyChange(() => LayerPropertiesViewModel);
            }
        }

        public EventPropertiesViewModel EventPropertiesViewModel
        {
            get { return _eventPropertiesViewModel; }
            set
            {
                if (Equals(value, _eventPropertiesViewModel)) return;
                _eventPropertiesViewModel = value;
                NotifyOfPropertyChange(() => EventPropertiesViewModel);
            }
        }

        public bool KeyboardGridIsVisible => ProposedLayer.LayerType == LayerType.Keyboard;
        public bool GifGridIsVisible => ProposedLayer.LayerType == LayerType.KeyboardGif;

        public void PreSelect()
        {
            LayerType = ProposedLayer.LayerType;
            if (LayerType == LayerType.Folder && !(LayerPropertiesViewModel is FolderPropertiesViewModel))
                LayerPropertiesViewModel = new FolderPropertiesViewModel(_dataModel, ProposedLayer.Properties);

            ToggleIsEvent();
        }

        private void PropertiesViewModelHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "LayerType")
                return;

            // Store the brush in case the user wants to reuse it
            var oldBrush = LayerPropertiesViewModel?.GetAppliedProperties().Brush;

            // Update the model
            if (ProposedLayer.LayerType != LayerType)
            {
                ProposedLayer.LayerType = LayerType;
                ProposedLayer.SetupProperties();
            }

            if (oldBrush != null)
                ProposedLayer.Properties.Brush = oldBrush;

            // Update the KeyboardPropertiesViewModel if it's being used
            var model = LayerPropertiesViewModel as KeyboardPropertiesViewModel;
            if (model != null)
                model.IsGif = LayerType == LayerType.KeyboardGif;

            // Apply the proper PropertiesViewModel
            if ((LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif) &&
                !(LayerPropertiesViewModel is KeyboardPropertiesViewModel))
            {
                LayerPropertiesViewModel = new KeyboardPropertiesViewModel(_dataModel, ProposedLayer.Properties)
                {
                    IsGif = LayerType == LayerType.KeyboardGif
                };
            }
            else if (LayerType == LayerType.Mouse && !(LayerPropertiesViewModel is MousePropertiesViewModel))
                LayerPropertiesViewModel = new MousePropertiesViewModel(_dataModel, ProposedLayer.Properties);
            else if (LayerType == LayerType.Headset && !(LayerPropertiesViewModel is HeadsetPropertiesViewModel))
                LayerPropertiesViewModel = new HeadsetPropertiesViewModel(_dataModel, ProposedLayer.Properties);
            else if (LayerType == LayerType.Folder && !(LayerPropertiesViewModel is FolderPropertiesViewModel))
                LayerPropertiesViewModel = new FolderPropertiesViewModel(_dataModel, ProposedLayer.Properties);

            NotifyOfPropertyChange(() => LayerPropertiesViewModel);
        }

        public void ToggleIsEvent()
        {
            EventPropertiesViewModel = ProposedLayer.IsEvent
                ? new EventPropertiesViewModel(Layer.EventProperties)
                : null;
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            LayerConditionVms.Add(new LayerConditionViewModel(this, condition, DataModelProps));
        }

        public void Apply()
        {
            Layer.Name = ProposedLayer.Name;
            Layer.LayerType = ProposedLayer.LayerType;
            Layer.IsEvent = ProposedLayer.IsEvent;

            if (LayerPropertiesViewModel != null)
                Layer.Properties = LayerPropertiesViewModel.GetAppliedProperties();
            if (EventPropertiesViewModel != null)
                Layer.EventProperties = EventPropertiesViewModel.GetAppliedProperties();

            Layer.Properties.Conditions.Clear();
            foreach (var conditionViewModel in LayerConditionVms)
            {
                Layer.Properties.Conditions.Add(conditionViewModel.LayerConditionModel);
            }

            if (Layer.LayerType != LayerType.KeyboardGif)
                return; // Don't bother checking for a GIF path unless the type is GIF
            if (!File.Exists(((KeyboardPropertiesModel) Layer.Properties).GifFile))
                DialogService.ShowErrorMessageBox("Couldn't find or access the provided GIF file.");
        }

        public void DeleteCondition(LayerConditionViewModel layerConditionViewModel,
            LayerConditionModel layerConditionModel)
        {
            LayerConditionVms.Remove(layerConditionViewModel);
            Layer.Properties.Conditions.Remove(layerConditionModel);
        }

        public override async void CanClose(Action<bool> callback)
        {
            // Create a fake layer and apply the properties to it
            var fakeLayer = GeneralHelpers.Clone(ProposedLayer);
            if (LayerPropertiesViewModel != null)
                fakeLayer.Properties = LayerPropertiesViewModel.GetAppliedProperties();
            fakeLayer.Properties.Conditions.Clear();
            foreach (var conditionViewModel in LayerConditionVms)
                fakeLayer.Properties.Conditions.Add(conditionViewModel.LayerConditionModel);


            var fake = GeneralHelpers.Serialize(fakeLayer);
            var real = GeneralHelpers.Serialize(Layer);

            if (fake.Equals(real))
            {
                callback(true);
                return;
            }

            var close = await DialogService
                .ShowQuestionMessageBox("Unsaved changes", "Do you want to discard your changes?");
            callback(close.Value);
        }
    }
}