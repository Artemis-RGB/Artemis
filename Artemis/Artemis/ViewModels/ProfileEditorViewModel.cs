using System.Windows.Media;
using Artemis.KeyboardProviders;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    internal class ProfileEditorViewModel : Screen
    {
        private readonly KeyboardProvider _keyboard;

        public ProfileEditorViewModel(KeyboardProvider keyboard)
        {
            _keyboard = keyboard;
        }

        private ImageSource GenerateKeyboardImage()
        {
            return null;
        }
    }
}