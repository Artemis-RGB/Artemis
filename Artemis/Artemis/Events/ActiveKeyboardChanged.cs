namespace Artemis.Events
{
    public class ActiveKeyboardChanged
    {
        public ActiveKeyboardChanged(string activeKeyboard)
        {
            ActiveKeyboard = activeKeyboard;
        }

        public string ActiveKeyboard { get; set; }
    }
}