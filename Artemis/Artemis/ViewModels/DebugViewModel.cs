using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class DebugViewModel : Screen
    {
        private DrawingImage _razerDisplay;

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

        public void OpenLog()
        {
            // Get the logging directory
            var logDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                           + @"\Artemis\logs");
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
    }
}