//The MIT License(MIT)

//Copyright(c) 2015 ihtfw

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Artemis.Services
{
    public abstract class DialogService
    {
        public void ShowErrorMessageBox(Exception e)
        {
            ShowErrorMessageBox(e.Message);
        }

        public void ShowErrorMessageBox(string message)
        {
            ShowMessageBox("Error", message);
        }

        public abstract void ShowMessageBox(string title, string message);

        public abstract bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null);

        public abstract Task<string> ShowInputDialog(string title, string message, MetroDialogSettings settings = null);

        public abstract Task<bool?> ShowQuestionMessageBox(string title, string message);
    }
}