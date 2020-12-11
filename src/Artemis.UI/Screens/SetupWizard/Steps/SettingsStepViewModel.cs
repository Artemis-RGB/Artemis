using System;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.SetupWizard.Steps
{
    public class SettingsStepViewModel : Conductor<PluginFeatureViewModel>.Collection.AllActive
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;

        public SettingsStepViewModel(IDialogService dialogService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;

            ColorSchemes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ApplicationColorScheme)));
        }

        public BindableCollection<ValueDescription> ColorSchemes { get; }

        public bool StartWithWindows
        {
            get => _settingsService.GetSetting("UI.AutoRun", false).Value;
            set
            {
                _settingsService.GetSetting("UI.AutoRun", false).Value = value;
                _settingsService.GetSetting("UI.AutoRun", false).Save();
                Task.Run(ApplyAutorun);
            }
        }

        public bool StartMinimized
        {
            get => !_settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            set
            {
                _settingsService.GetSetting("UI.ShowOnStartup", true).Value = !value;
                _settingsService.GetSetting("UI.ShowOnStartup", true).Save();
            }
        }


        public ApplicationColorScheme SelectedColorScheme
        {
            get => _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Value;
            set
            {
                _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Value = value;
                _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Save();
            }
        }

        private void ApplyAutorun()
        {
            try
            {
                string autoRunFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Artemis.lnk");
                string executableFile = Constants.ExecutablePath;

                if (File.Exists(autoRunFile))
                    File.Delete(autoRunFile);
                if (StartWithWindows)
                    ShortcutUtilities.Create(autoRunFile, executableFile, "--autorun", new FileInfo(executableFile).DirectoryName, "Artemis", "", "");
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to apply the auto run setting", e);
                throw;
            }
        }
    }
}