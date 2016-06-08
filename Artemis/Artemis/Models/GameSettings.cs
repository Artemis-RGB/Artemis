namespace Artemis.Models
{
    public abstract class GameSettings : EffectSettings
    {
        public bool Enabled { get; set; }
        public string LastProfile { get; set; }
    }
}