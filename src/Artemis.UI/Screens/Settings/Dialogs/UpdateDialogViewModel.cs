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
        private readonly IUpdateService _updateService;

        public UpdateDialogViewModel(JToken buildInfo, IUpdateService updateService)
        {
            _buildInfo = buildInfo;
            _updateService = updateService;
            CurrentBuild = Constants.BuildInfo.BuildNumberDisplay;
            LatestBuild = buildInfo?.SelectToken("value[0].buildNumber")?.Value<string>();
        }

        public string CurrentBuild { get; }
        public string LatestBuild { get; }

        public async Task Update()
        {
            await _updateService.ApplyUpdate();
            Session.Close();
        }
    }
}