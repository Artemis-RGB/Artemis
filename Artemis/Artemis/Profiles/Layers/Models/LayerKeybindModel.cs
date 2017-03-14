using System;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using MahApps.Metro.Controls;

namespace Artemis.Profiles.Layers.Models
{
    public class LayerKeybindModel
    {
        private KeybindModel _downKeybind;
        private KeybindModel _upKeybind;

        public ToggleType ToggleType { get; set; }
        public HotKey HotKey { get; set; }
        public MouseButtons? MouseButtons { get; set; }

        public void Unregister()
        {
            if (_downKeybind != null)
            {
                KeybindManager.Remove(_downKeybind);
                _downKeybind = null;
            }
            if (_upKeybind != null)
            {
                KeybindManager.Remove(_upKeybind);
                _upKeybind = null;
            }
        }

        internal void Register(LayerModel layerModel, int index)
        {
            Unregister();

            // Bind EnableHeldDown or DisableHeldDOwn
            if (ToggleType == ToggleType.EnableHeldDown || ToggleType == ToggleType.DisableHeldDown)
            {
                Action downAction = null;
                Action upAction = null;
                switch (ToggleType)
                {
                    case ToggleType.EnableHeldDown:
                        downAction = () => layerModel.RenderAllowed = true;
                        upAction = () => layerModel.RenderAllowed = false;
                        break;
                    case ToggleType.DisableHeldDown:
                        downAction = () => layerModel.RenderAllowed = false;
                        upAction = () => layerModel.RenderAllowed = true;
                        break;
                }

                // Either bind HotKey or mouse buttons depending on what isn't null
                if (HotKey != null)
                {
                    _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", HotKey, PressType.Down, downAction);
                    _upKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-up", HotKey, PressType.Up, upAction);
                }
                else if (MouseButtons != null)
                {
                    _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", MouseButtons.Value, PressType.Down, downAction);
                    _upKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-up", MouseButtons.Value, PressType.Up, upAction);
                }
                KeybindManager.AddOrUpdate(_downKeybind);
                KeybindManager.AddOrUpdate(_upKeybind);
                return;
            }

            // Bind Enable, Disable or Toggle
            Action action = null;
            switch (ToggleType)
            {
                case ToggleType.Enable:
                    action = () => layerModel.RenderAllowed = true;
                    break;
                case ToggleType.Disable:
                    action = () => layerModel.RenderAllowed = false;
                    break;
                case ToggleType.Toggle:
                    action = () => layerModel.RenderAllowed = !layerModel.RenderAllowed;
                    break;
            }

            // Either bind HotKey or mouse buttons depending on what isn't null
            if (HotKey != null)
                _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", HotKey, PressType.Down, action);
            else if (MouseButtons != null)
                _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", MouseButtons.Value, PressType.Down, action);
            KeybindManager.AddOrUpdate(_downKeybind);
        }
    }

    public enum ToggleType
    {
        Enable,
        Disable,
        Toggle,
        EnableHeldDown,
        DisableHeldDown
    }
}
