using System;
using System.Windows;
using Artemis.UI.Shared.LayerBrushes;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Windows
{
    public class LayerBrushSettingsWindowViewModel : Conductor<BrushConfigurationViewModel>
    {
        private readonly LayerBrushConfigurationDialog _configuration;

        public LayerBrushSettingsWindowViewModel(BrushConfigurationViewModel configurationViewModel, LayerBrushConfigurationDialog configuration)
        {
            _configuration = configuration;
            ActiveItem = configurationViewModel ?? throw new ArgumentNullException(nameof(configurationViewModel));
            ActiveItem.Closed += ActiveItemOnClosed;
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnViewLoaded()
        {
            // Setting the width/height via a binding and depending on WindowStartupLocation does not work
            Window window = View as Window;
            Window mainWindow = Application.Current.MainWindow;
            if (window == null || mainWindow == null)
                return;

            window.Width = _configuration.DialogWidth;
            window.Height = _configuration.DialogHeight;
            window.Left = mainWindow.Left + (mainWindow.Width - window.Width) / 2;
            window.Top = mainWindow.Top + (mainWindow.Height - window.Height) / 2;

            base.OnViewLoaded();
        }

        #endregion

        private void ActiveItemOnClosed(object sender, CloseEventArgs e)
        {
            ActiveItem.Closed -= ActiveItemOnClosed;
            RequestClose();
        }
    }
}