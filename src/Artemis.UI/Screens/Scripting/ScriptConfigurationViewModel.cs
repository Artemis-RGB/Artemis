using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Scripting
{
    public class ScriptConfigurationViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IScriptingService _scriptingService;

        public ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration, IScriptingService scriptingService, IDialogService dialogService)
        {
            _scriptingService = scriptingService;
            _dialogService = dialogService;
            ScriptConfiguration = scriptConfiguration;
            Script = ScriptConfiguration.Script;
        }

        public Script Script { get; set; }
        public ScriptConfiguration ScriptConfiguration { get; }
    }
}