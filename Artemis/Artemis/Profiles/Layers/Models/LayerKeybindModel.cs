using System;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Conditions;
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

            if (layerModel.IsEvent)
                RegisterEvent(layerModel, index);
            else if (ToggleType == ToggleType.EnableHeldDown || ToggleType == ToggleType.DisableHeldDown)
                RegisterToggle(layerModel, index);
            else
                RegisterRegular(layerModel, index);
        }

        private void RegisterEvent(LayerModel layerModel, int index)
        {
            Action action = () =>
            {
                layerModel.EventProperties.TriggerEvent(layerModel);
            };
            // Either bind HotKey or mouse buttons depending on what isn't null
            if (HotKey != null)
                _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", HotKey, PressType.Down, action);
            else if (MouseButtons != null)
                _downKeybind = new KeybindModel($"{layerModel.GetHashCode()}-{layerModel.Name}-{index}-down", MouseButtons.Value, PressType.Down, action);
            KeybindManager.AddOrUpdate(_downKeybind);
        }

        private void RegisterRegular(LayerModel layerModel, int index)
        {
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

        private void RegisterToggle(LayerModel layerModel, int index)
        {
            Action downAction = null;
            Action upAction = null;
            switch (ToggleType)
            {
                case ToggleType.EnableHeldDown:
                    layerModel.RenderAllowed = false;
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
