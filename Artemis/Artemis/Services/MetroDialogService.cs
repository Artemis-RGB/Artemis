// The MIT License(MIT)

// Copyright(c) 2015 ihtfw

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Artemis.Services
{
    public class MetroDialogService : DialogService
    {
        private MetroWindow GetActiveWindow()
        {
            MetroWindow window = null;

            Execute.OnUIThread(() =>
            {
                window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault(w => w.IsActive) ??
                         Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            });

            return window;
        }

        public override void ShowMessageBox(string title, string message)
        {
            if (GetActiveWindow() == null)
                return;

            Execute.OnUIThread(() => GetActiveWindow().ShowMessageAsync(title, message));
        }

        public override async Task<bool?> ShowQuestionMessageBox(string title, string message)
        {
            if (GetActiveWindow() == null)
                return null;

            var metroDialogSettings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
            var result =
                await
                    GetActiveWindow()
                        .ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings);
            switch (result)
            {
                case MessageDialogResult.Negative:
                    return false;
                case MessageDialogResult.Affirmative:
                    return true;
                default:
                    return null;
            }
        }

        public override Task<string> ShowInputDialog(string title, string message, MetroDialogSettings settings = null)
        {
            if (GetActiveWindow() == null)
                return null;

            return GetActiveWindow().ShowInputAsync(title, message, settings);
        }

        public override bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null)
        {
            if (GetActiveWindow() == null)
            {
                path = null;
                return false;
            }

            bool? res = null;
            string lPath = null;

            Execute.OnUIThread(() =>
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = defaultExt,
                    Filter = filter
                };

                if (initialDir != null)
                {
                    ofd.InitialDirectory = initialDir;
                }

                if (Application.Current.MainWindow != null)
                {
                    res = ofd.ShowDialog(Application.Current.MainWindow);
                }
                else
                {
                    res = ofd.ShowDialog();
                }
                if (res == true)
                {
                    lPath = ofd.FileName;
                }
                else
                {
                    res = false;
                }
            });

            path = lPath;

            return res.Value;
        }
    }
}