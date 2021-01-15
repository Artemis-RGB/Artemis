using System;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using Newtonsoft.Json.Linq;

namespace Artemis.UI.Screens.Settings.Dialogs
{
    public class UpdateDialogViewModel : DialogViewModelBase
    {
        private readonly JToken _buildInfo;
        private readonly IDialogService _dialogService;
        private readonly IUpdateService _updateService;
        private bool _canUpdate = true;

        public UpdateDialogViewModel(JToken buildInfo, IUpdateService updateService, IDialogService dialogService)
        {
            _buildInfo = buildInfo;
            _updateService = updateService;
            _dialogService = dialogService;
            
            CurrentBuild = Constants.BuildInfo.BuildNumberDisplay;
            LatestBuild = buildInfo?.SelectToken("value[0].buildNumber")?.Value<string>();
        }

        public string CurrentBuild { get; }
        public string LatestBuild { get; }

        public bool CanUpdate
        {
            get => _canUpdate;
            set => SetAndNotify(ref _canUpdate, value);
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