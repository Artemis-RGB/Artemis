using System;
using Artemis.DeviceProviders;

namespace Artemis.Events
{
    public class KeyboardChangedEventArgs : EventArgs
    {
        public KeyboardChangedEventArgs(KeyboardProvider oldKeyboard, KeyboardProvider newKeyboard)
        {
            OldKeyboard = oldKeyboard;
            NewKeyboard = newKeyboard;
        }

        public KeyboardProvider OldKeyboard { get; }
        public KeyboardProvider NewKeyboard { get; }
    }
}