using Artemis.DeviceProviders;

namespace Artemis.Events
{
    public class ActiveKeyboardChanged
    {
        public ActiveKeyboardChanged(KeyboardProvider oldKeyboard, KeyboardProvider newKeyboard)
        {
            OldKeyboard = oldKeyboard;
            NewKeyboard = newKeyboard;
        }

        public KeyboardProvider OldKeyboard { get; set; }
        public KeyboardProvider NewKeyboard { get; set; }
    }
}