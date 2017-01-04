using System;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Interfaces;

namespace Artemis.Events
{
    public class ProfileDeviceEventsArg : EventArgs
    {
        public ProfileDeviceEventsArg(DrawType updateType, ModuleDataModel dataModel, bool preview, DrawingContext drawingContext)
        {
            UpdateType = updateType.ToString().ToLower();
            DataModel = dataModel;
            Preview = preview;
            DrawingContext = drawingContext;
        }

        public string UpdateType { get; }
        public ModuleDataModel DataModel { get; }
        public bool Preview { get; }
        public DrawingContext DrawingContext { get; }
    }
}