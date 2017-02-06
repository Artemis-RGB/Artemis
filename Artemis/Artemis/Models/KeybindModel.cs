using System;
using MahApps.Metro.Controls;

namespace Artemis.Models
{
    public class KeybindModel
    {
        public KeybindModel(string name, HotKey hotKey, Action action)
        {
            Name = name;
            HotKey = hotKey;
            Action = action;
        }

        public string Name { get; set; }
        public HotKey HotKey { get; set; }
        public Action Action { get; set; }

        public void InvokeIfMatched(HotKey hotKey)
        {
            if (hotKey.Equals(HotKey))
                Action.Invoke();
        }
    }
}