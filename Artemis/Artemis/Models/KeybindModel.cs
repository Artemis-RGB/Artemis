using System;
using System.Windows.Forms;
using MahApps.Metro.Controls;

namespace Artemis.Models
{
    public class KeybindModel
    {
        public KeybindModel(string name, HotKey hotKey, KeyType keyType, Action action)
        {
            Name = name;
            HotKey = hotKey;
            KeyType = keyType;
            Action = action;
        }

        public KeybindModel(string name, MouseButtons mouseButtons, KeyType keyType, Action action)
        {
            Name = name;
            MouseButtons = mouseButtons;
            KeyType = keyType;
            Action = action;
        }

        public string Name { get; set; }
        public HotKey HotKey { get; set; }
        public MouseButtons MouseButtons { get; }
        public KeyType KeyType { get; set; }
        public Action Action { get; set; }

        public void InvokeIfMatched(HotKey hotKey, KeyType keyType)
        {
            if (HotKey == null || hotKey == null || KeyType != keyType)
                return;

            if (hotKey.Equals(HotKey))
                Action?.Invoke();
        }

        public void InvokeIfMatched(MouseButtons mouseButtons, KeyType keyType)
        {
            if (KeyType != keyType)
                return;

            if (mouseButtons.Equals(MouseButtons))
                Action?.Invoke();
        }
    }

    public enum KeyType
    {
        KeyDown,
        KeyUp,
        MouseDown,
        MouseUp
    }
}
