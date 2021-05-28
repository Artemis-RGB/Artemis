﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs
{
    public class LayerHintsDialogViewModel : Conductor<AdaptionHintViewModel>.Collection.AllActive
    {
        private readonly IRgbService _rgbService;
        private readonly ILayerHintVmFactory _vmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private SnackbarMessageQueue _layerHintsMessageQueue;

        public LayerHintsDialogViewModel(Layer layer, IRgbService rgbService, ILayerHintVmFactory vmFactory, IProfileEditorService profileEditorService)
        {
            _rgbService = rgbService;
            _vmFactory = vmFactory;
            _profileEditorService = profileEditorService;

            Layer = layer;
            DisplayName = "Layer hints | Artemis";
        }

        public Layer Layer { get; }
        public bool HasAdaptionHints => Items.Any();

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
            NotifyOfPropertyChange(nameof(HasAdaptionHints));
        }

        public void AddDeviceHint()
        {
            DeviceAdaptionHint hint = new();
            Layer.Adapter.AdaptionHints.Add(hint);
            Items.Add(_vmFactory.DeviceAdaptionHintViewModel(hint));
            NotifyOfPropertyChange(nameof(HasAdaptionHints));
        }

        public void AddKeyboardSectionHint()
        {
            KeyboardSectionAdaptionHint hint = new();
            Layer.Adapter.AdaptionHints.Add(hint);
            Items.Add(_vmFactory.KeyboardSectionAdaptionHintViewModel(hint));
            NotifyOfPropertyChange(nameof(HasAdaptionHints));
        }

        public void RemoveAdaptionHint(AdaptionHintViewModel adaptionHintViewModel)
        {
            Layer.Adapter.AdaptionHints.Remove(adaptionHintViewModel.AdaptionHint);
            Items.Remove(adaptionHintViewModel);
            NotifyOfPropertyChange(nameof(HasAdaptionHints));
        }

        public void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Core.Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            LayerHintsMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            CreateHintViewModels(Layer.Adapter.AdaptionHints);

            base.OnInitialActivate();
        }

        #region Overrides of AllActive

        /// <inheritdoc />
        protected override void OnClose()
        {
            _profileEditorService.SaveSelectedProfileElement();
            base.OnClose();
        }

        #endregion

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
            NotifyOfPropertyChange(nameof(HasAdaptionHints));
        }
    }
}