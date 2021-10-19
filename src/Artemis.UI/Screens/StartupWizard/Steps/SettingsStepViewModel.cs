using System;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.Screens.StartupWizard.Steps
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
                NotifyOfPropertyChange(nameof(StartWithWindows));
                Task.Run(() => ApplyAutorun(false));
            }
        }

        public bool StartMinimized
        {
            get => !_settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            set
            {
                _settingsService.GetSetting("UI.ShowOnStartup", true).Value = !value;
                _settingsService.GetSetting("UI.ShowOnStartup", true).Save();
                NotifyOfPropertyChange(nameof(StartMinimized));
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

        private void ApplyAutorun(bool recreate)
        {
            if (!StartWithWindows)
                StartMinimized = false;

            // Remove the old auto-run method of placing a shortcut in shell:startup
            string autoRunFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Artemis.lnk");
            if (File.Exists(autoRunFile))
                File.Delete(autoRunFile);

            // Local builds shouldn't auto-run, this is just annoying
            // if (Constants.BuildInfo.IsLocalBuild)
            //     return;

            // Create or remove the task if necessary
            try
            {
                bool taskCreated = false;
                if (!recreate)
                    taskCreated = SettingsUtilities.IsAutoRunTaskCreated();

                if (StartWithWindows && !taskCreated)
                    SettingsUtilities.CreateAutoRunTask(TimeSpan.FromSeconds(15));
                else if (!StartWithWindows && taskCreated)
                    SettingsUtilities.RemoveAutoRunTask();
            }
            catch (Exception e)
            {
                Execute.PostToUIThread(() => _dialogService.ShowExceptionDialog("An exception occured while trying to apply the auto run setting", e));
                throw;
            }
        }
    }
}