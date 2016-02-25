using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Artemis.KeyboardProviders;
using Artemis.Settings;

namespace Artemis.Managers
{
    public class KeyboardManager
    {
        private readonly MainManager _mainManager;

        public KeyboardManager(MainManager mainManager)
        {
            _mainManager = mainManager;
            KeyboardProviders = ProviderHelper.GetKeyboardProviders();
        }

        public List<KeyboardProvider> KeyboardProviders { get; set; }
        public KeyboardProvider ActiveKeyboard { get; set; }

        public bool LoadLastKeyboard()
        {
            var keyboard = KeyboardProviders.FirstOrDefault(k => k.Name == General.Default.LastKeyboard);
            return ChangeKeyboard(keyboard ?? KeyboardProviders.First());
        }

        public bool ChangeKeyboard(KeyboardProvider keyboardProvider)
        {
            if (keyboardProvider == null)
                return false;

            if (ActiveKeyboard != null)
                if (keyboardProvider.Name == ActiveKeyboard.Name)
                    return true;

            ReleaseActiveKeyboard();

            // Disable everything if there's no active keyboard found
            if (!keyboardProvider.CanEnable())
            {
                MessageBox.Show(keyboardProvider.CantEnableText, "Artemis  (╯°□°）╯︵ ┻━┻", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            ActiveKeyboard = keyboardProvider;
            ActiveKeyboard.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();

            return true;
        }

        public void ReleaseActiveKeyboard()
        {
            if (ActiveKeyboard == null)
                return;

            ActiveKeyboard.Disable();
            ActiveKeyboard = null;
        }
    }
}