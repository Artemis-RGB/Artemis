namespace Artemis.Events
{
    public class ToggleEnabled
    {
        public bool Enabled { get; set; }

        public ToggleEnabled(bool enabled)
        {
            Enabled = enabled;
        }
    }
}