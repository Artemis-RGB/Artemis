using System;
using System.Windows.Media;
using Artemis.Models.Interfaces;

namespace Artemis.Events
{
    public class ProfileDeviceEventsArg : EventArgs
    {
        public ProfileDeviceEventsArg(string updateType, IDataModel dataModel, bool preview, DrawingContext drawingContext)
        {
            UpdateType = updateType;
            DataModel = dataModel;
            Preview = preview;
            DrawingContext = drawingContext;
        }

        public string UpdateType { get; }
        public IDataModel DataModel { get; }
        public bool Preview { get; }
        public DrawingContext DrawingContext { get; }
    }
}