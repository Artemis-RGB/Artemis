using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class DebugViewModel : Screen
    {
        private readonly DeviceManager _deviceManager;
        private DrawingImage _generic;
        private DrawingImage _headset;
        private DrawingImage _keyboard;
        private DrawingImage _mouse;
        private DrawingImage _mousemat;

        private DrawingImage _razerDisplay;

        public DebugViewModel(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public DrawingImage RazerDisplay
        {
            get { return _razerDisplay; }
            set
            {
                if (Equals(value, _razerDisplay)) return;
                _razerDisplay = value;
                NotifyOfPropertyChange(() => RazerDisplay);
            }
        }


        public DrawingImage Keyboard
        {
            get { return _keyboard; }
            set
            {
                if (Equals(value, _keyboard)) return;
                _keyboard = value;
                NotifyOfPropertyChange(() => Keyboard);
            }
        }

        public DrawingImage Mouse
        {
            get { return _mouse; }
            set
            {
                if (Equals(value, _mouse)) return;
                _mouse = value;
                NotifyOfPropertyChange(() => Mouse);
            }
        }

        public DrawingImage Headset
        {
            get { return _headset; }
            set
            {
                if (Equals(value, _headset)) return;
                _headset = value;
                NotifyOfPropertyChange(() => Headset);
            }
        }

        public DrawingImage Mousemat
        {
            get { return _mousemat; }
            set
            {
                if (Equals(value, _mousemat)) return;
                _mousemat = value;
                NotifyOfPropertyChange(() => Mousemat);
            }
        }

        public DrawingImage Generic
        {
            get { return _generic; }
            set
            {
                if (Equals(value, _generic)) return;
                _generic = value;
                NotifyOfPropertyChange(() => Generic);
            }
        }

        public void OpenLog()
        {
            // Get the logging directory
            var logDir = new DirectoryInfo(GeneralHelpers.DataFolder + "logs");
            // Get the newest log file
            var currentLog = logDir.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            // Open the file with the user's default program
            if (currentLog != null)
                System.Diagnostics.Process.Start(currentLog.FullName);
        }

        public void UpdateRazerDisplay(Color[,] colors)
        {
            // No point updating the display if the view isn't visible
            if (!IsActive)
                return;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushClip(new RectangleGeometry(new Rect(0, 0, 22, 6)));
                for (var y = 0; y < 6; y++)
                for (var x = 0; x < 22; x++)
                    dc.DrawRectangle(new SolidColorBrush(colors[y, x]), null, new Rect(x, y, 1, 1));
            }
            var drawnDisplay = new DrawingImage(visual.Drawing);
            drawnDisplay.Freeze();
            RazerDisplay = drawnDisplay;
        }

        public void DrawFrame(FrameModel frameModel)
        {
            // No point updating the display if the view isn't visible
            if (!IsActive)
                return;

            // Only update keyboard if there is an active keyboard
            if (_deviceManager.ActiveKeyboard != null)
            {
                var rect = _deviceManager.ActiveKeyboard.KeyboardRectangle(1);
                Keyboard = ImageUtilities.BitmapToDrawingImage(frameModel.KeyboardBitmap, rect);
            }

            if (frameModel.MouseBitmap != null)
                Mouse = ImageUtilities.BitmapToDrawingImage(frameModel.MouseBitmap, new Rect(0, 0, 10, 10));
            if (frameModel.HeadsetBitmap != null)
                Headset = ImageUtilities.BitmapToDrawingImage(frameModel.HeadsetBitmap, new Rect(0, 0, 10, 10));
            if (frameModel.MousematBitmap != null)
                Mousemat = ImageUtilities.BitmapToDrawingImage(frameModel.MousematBitmap, new Rect(0, 0, 10, 10));
            if (frameModel.GenericBitmap != null)
                Generic = ImageUtilities.BitmapToDrawingImage(frameModel.GenericBitmap, new Rect(0, 0, 10, 10));
        }
    }
}