using Artemis.Models;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplaySettings : EffectSettings
    {
        public VolumeDisplaySettings()
        {
            Load();
        }

        public bool Enabled { get; set; }
        public bool DisplayVolume { get; set; }
        public bool DisplayPlayPause { get; set; }
        public bool DisplayPreviousNext { get; set; }
        public bool DisplayStop { get; set; }

        public override sealed void Load()
        {
            ToDefault();
        }

        public override sealed void Save()
        {
        }

        public override sealed void ToDefault()
        {
            Enabled = true;
            DisplayVolume = true;
            DisplayPlayPause = true;
            DisplayPreviousNext = true;
            DisplayStop = true;
        }
    }
}