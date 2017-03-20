using System;
using System.Windows.Forms;
using MahApps.Metro.Controls;

namespace Artemis.Models
{
    public class KeybindModel
    {
        public KeybindModel(string name, HotKey hotKey, PressType pressType, Action action)
        {
            Name = name;
            HotKey = hotKey;
            PressType = pressType;
            Action = action;
        }

        public KeybindModel(string name, MouseButtons mouseButtons, PressType pressType, Action action)
        {
            Name = name;
            MouseButtons = mouseButtons;
            PressType = pressType;
            Action = action;
        }

        public string Name { get; set; }
        public HotKey HotKey { get; set; }
        public MouseButtons? MouseButtons { get; }
        public PressType PressType { get; set; }
        public Action Action { get; set; }

        public void InvokeIfMatched(HotKey hotKey, PressType pressType)
        {
            if (HotKey == null || hotKey == null || PressType != pressType)
                return;

            if (hotKey.Equals(HotKey))
                Action?.Invoke();
        }

        public void InvokeIfMatched(MouseButtons mouseButtons, PressType pressType)
        {
            if (PressType != pressType)
                return;

            if (mouseButtons.Equals(MouseButtons))
                Action?.Invoke();
        }
    }

    public enum PressType
    {
        Down,
        Up
    }
}
