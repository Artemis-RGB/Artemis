using System.ComponentModel;
using System.Windows.Forms;
using Artemis.Profiles.Layers.Models;
using MahApps.Metro.Controls;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.ViewModels.Profiles
{
    public sealed class LayerKeybindViewModel : Screen
    {
        private readonly LayerEditorViewModel _editorViewModel;
        private bool _canToggleType;

        private HotKey _hotKey;
        private MouseButtons _mouseButtons;
        private ToggleType _toggleType;

        public LayerKeybindViewModel(LayerEditorViewModel editorViewModel, LayerKeybindModel layerKeybindModel)
        {
            _editorViewModel = editorViewModel;
            LayerKeybindModel = layerKeybindModel;

            PropertyChanged += MapViewToModel;
            editorViewModel.PropertyChanged += EditorViewModelOnPropertyChanged;
            MapModelToView();
        }

        public bool CanToggleType
        {
            get { return _canToggleType; }
            set
            {
                if (value == _canToggleType)
                    return;
                _canToggleType = value;
                NotifyOfPropertyChange(() => CanToggleType);
            }
        }

        public LayerKeybindModel LayerKeybindModel { get; set; }


        public bool MouseButtonsVisible => LayerKeybindModel.MouseButtons != null;
        public bool HotkeyVisible => LayerKeybindModel.MouseButtons == null;

        public MouseButtons MouseButtons
        {
            get { return _mouseButtons; }
            set
            {
                if (value == _mouseButtons)
                    return;
                _mouseButtons = value;
                NotifyOfPropertyChange(() => MouseButtons);
            }
        }

        public HotKey HotKey
        {
            get { return _hotKey; }
            set
            {
                if (Equals(value, _hotKey))
                    return;
                _hotKey = value;
                NotifyOfPropertyChange(() => HotKey);
            }
        }

        public ToggleType ToggleType
        {
            get { return _toggleType; }
            set
            {
                if (value == _toggleType)
                    return;
                _toggleType = value;
                NotifyOfPropertyChange(() => ToggleType);
            }
        }

        /// <summary>
        ///     Responds to the EventPropertiesViewModel being changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "EventPropertiesViewModel")
                return;

            if (_editorViewModel.ProposedLayer.IsEvent)
            {
                CanToggleType = false;
                ToggleType = ToggleType.Enable;
            }
            else
                CanToggleType = true;
        }

        private void MapViewToModel(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MouseButtonsVisible" || e.PropertyName == "HotkeyVisible")
                return;

            if (MouseButtonsVisible)
                SetMouseBind();
            else
                SetKeyBind();
        }

        private void MapModelToView()
        {
            PropertyChanged -= MapViewToModel;

            if (LayerKeybindModel.MouseButtons != null)
                MouseButtons = LayerKeybindModel.MouseButtons.Value;
            HotKey = LayerKeybindModel.HotKey;
            ToggleType = LayerKeybindModel.ToggleType;

            PropertyChanged += MapViewToModel;
        }

        public void ToggleBindType()
        {
            if (MouseButtonsVisible)
                SetKeyBind();
            else
                SetMouseBind();
        }

        public void SetMouseBind()
        {
            LayerKeybindModel.Unregister();
            LayerKeybindModel.HotKey = null;
            LayerKeybindModel.MouseButtons = MouseButtons;
            LayerKeybindModel.ToggleType = ToggleType;

            NotifyOfPropertyChange(() => HotkeyVisible);
            NotifyOfPropertyChange(() => MouseButtonsVisible);
        }

        public void SetKeyBind()
        {
            LayerKeybindModel.Unregister();
            LayerKeybindModel.HotKey = HotKey;
            LayerKeybindModel.MouseButtons = null;
            LayerKeybindModel.ToggleType = ToggleType;

            NotifyOfPropertyChange(() => HotkeyVisible);
            NotifyOfPropertyChange(() => MouseButtonsVisible);
        }

        public void Delete()
        {
            _editorViewModel.LayerKeybindVms.Remove(this);
        }
    }
}
