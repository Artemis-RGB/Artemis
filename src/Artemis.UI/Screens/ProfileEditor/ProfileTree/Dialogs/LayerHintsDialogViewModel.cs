using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs
{
    public class LayerHintsDialogViewModel : Conductor<AdaptionHintViewModel>.Collection.AllActive
    {
        private readonly IRgbService _rgbService;
        private readonly ILayerHintVmFactory _vmFactory;
        private SnackbarMessageQueue _layerHintsMessageQueue;

        public LayerHintsDialogViewModel(Layer layer, IRgbService rgbService, ILayerHintVmFactory vmFactory)
        {
            _rgbService = rgbService;
            _vmFactory = vmFactory;

            Layer = layer;
            DisplayName = "Layer hints | Artemis";
        }

        public Layer Layer { get; }

        public SnackbarMessageQueue LayerHintsMessageQueue
        {
            get => _layerHintsMessageQueue;
            set => SetAndNotify(ref _layerHintsMessageQueue, value);
        }

        public void AutoDetermineHints()
        {
            List<IAdaptionHint> newHints = Layer.Adapter.DetermineHints(_rgbService.EnabledDevices);
            CreateHintViewModels(newHints);
        }

        public void AddCategoryHint()
        {
            CategoryAdaptionHint hint = new();
            Layer.Adapter.AdaptionHints.Add(hint);
            Items.Add(_vmFactory.CategoryAdaptionHintViewModel(hint));
        }

        public void AddDeviceHint()
        {
            DeviceAdaptionHint hint = new();
            Layer.Adapter.AdaptionHints.Add(hint);
            Items.Add(_vmFactory.DeviceAdaptionHintViewModel(hint));
        }

        public void AddKeyboardSectionHint()
        {
            KeyboardSectionAdaptionHint hint = new();
            Layer.Adapter.AdaptionHints.Add(hint);
            Items.Add(_vmFactory.KeyboardSectionAdaptionHintViewModel(hint));
        }

        public void RemoveAdaptionHint(AdaptionHintViewModel adaptionHintViewModel)
        {
            Layer.Adapter.AdaptionHints.Remove(adaptionHintViewModel.AdaptionHint);
            Items.Remove(adaptionHintViewModel);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            LayerHintsMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            CreateHintViewModels(Layer.Adapter.AdaptionHints);

            base.OnInitialActivate();
        }

        #endregion

        private void CreateHintViewModels(List<IAdaptionHint> hints)
        {
            foreach (IAdaptionHint adapterAdaptionHint in hints)
            {
                switch (adapterAdaptionHint)
                {
                    case CategoryAdaptionHint categoryAdaptionHint:
                        Items.Add(_vmFactory.CategoryAdaptionHintViewModel(categoryAdaptionHint));
                        break;
                    case DeviceAdaptionHint deviceAdaptionHint:
                        Items.Add(_vmFactory.DeviceAdaptionHintViewModel(deviceAdaptionHint));
                        break;
                    case KeyboardSectionAdaptionHint keyboardSectionAdaptionHint:
                        Items.Add(_vmFactory.KeyboardSectionAdaptionHintViewModel(keyboardSectionAdaptionHint));
                        break;
                }
            }
        }
    }
}