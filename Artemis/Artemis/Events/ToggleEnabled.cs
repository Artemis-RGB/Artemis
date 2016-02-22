namespace Artemis.Events
{
    public class ToggleEnabled
    {
        public ToggleEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}