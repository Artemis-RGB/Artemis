using System.Reflection;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.About
{
    public class AboutTabViewModel : Screen
    {
        private string _version;

        public AboutTabViewModel()
        {
            DisplayName = "ABOUT";
        }

        public string Version
        {
            get => _version;
            set => SetAndNotify(ref _version, value);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Version = $"Version {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";
            
            base.OnActivate();
        }

        #endregion
    }
}