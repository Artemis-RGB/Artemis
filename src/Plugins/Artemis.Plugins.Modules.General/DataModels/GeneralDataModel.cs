using System;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.General.DataModels.Windows;
using SkiaSharp;

namespace Artemis.Plugins.Modules.General.DataModels
{
    public class GeneralDataModel : DataModel
    {
        public GeneralDataModel()
        {
            TimeDataModel = new TimeDataModel();
        }

        public WindowDataModel ActiveWindow { get; set; }
        public TimeDataModel TimeDataModel { get; set; }
    }

    public class TimeDataModel : DataModel
    {
        public DateTimeOffset CurrentTime { get; set; }
        public TimeSpan TimeSinceMidnight { get; set; }
    }

    public class IconColorsDataModel : DataModel 
    {
        public SKColor Vibrant { get; set; }
        public SKColor LightVibrant { get; set; }
        public SKColor DarkVibrant { get; set; }
        public SKColor Muted { get; set; }
        public SKColor LightMuted { get; set; }
        public SKColor DarkMuted { get; set; }
    }
}