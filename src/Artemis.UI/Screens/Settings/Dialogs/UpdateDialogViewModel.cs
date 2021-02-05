using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Services;
using Artemis.UI.Services.Models.UpdateService;
using Artemis.UI.Shared.Services;
using Newtonsoft.Json.Linq;
using Stylet;

namespace Artemis.UI.Screens.Settings.Dialogs
{
    public class UpdateDialogViewModel : DialogViewModelBase
    {
        private readonly DevOpsBuild _buildInfo;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IUpdateService _updateService;
        private bool _canUpdate = true;
        private bool _retrievingChanges;

        public UpdateDialogViewModel(DevOpsBuild buildInfo, IUpdateService updateService, IDialogService dialogService, IMessageService messageService)
        {
            _buildInfo = buildInfo;
            _updateService = updateService;
            _dialogService = dialogService;
            _messageService = messageService;

            CurrentBuild = Constants.BuildInfo.BuildNumberDisplay;
            LatestBuild = buildInfo.BuildNumber;
            Changes = new BindableCollection<string>();

            Task.Run(GetBuildChanges);
        }

        public string CurrentBuild { get; }
        public string LatestBuild { get; }
        public BindableCollection<string> Changes { get; }

        public bool RetrievingChanges
        {
            get => _retrievingChanges;
            set => SetAndNotify(ref _retrievingChanges, value);
        }

        public bool CanUpdate
        {
            get => _canUpdate;
            set => SetAndNotify(ref _canUpdate, value);
        }

        private async Task GetBuildChanges()
        {
            try
            {
                RetrievingChanges = true;
                Task<DevOpsBuild> currentTask = _updateService.GetBuildInfo(1, CurrentBuild);
                Task<DevOpsBuild> latestTask = _updateService.GetBuildInfo(1, LatestBuild);

                DevOpsBuild current = await currentTask;
                DevOpsBuild latest = await latestTask;

                if (current != null && latest != null)
                {
                    GitHubDifference difference = await _updateService.GetBuildDifferences(current, latest);

                    // Only take commits with one parents (no merges)
                    Changes.Clear();
                    Changes.AddRange(difference.Commits.Where(c => c.Parents.Count == 1)
                        .SelectMany(c => c.Commit.Message.Split("\n"))
                        .Select(m => m.Trim())
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .OrderBy(m => m)
                    );
                }
            }
            catch (Exception e)
            {
                _messageService.ShowMessage($"Failed to retrieve build changes - {e.Message}");
            }
            finally
            {
                RetrievingChanges = false;
            }
        }

        public async Task Update()
        {
            try
            {
                CanUpdate = false;
                await _updateService.ApplyUpdate();
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occurred while applying the update", e);
            }
            finally
            {
                CanUpdate = true;
            }

            Session.Close();
        }
    }
}