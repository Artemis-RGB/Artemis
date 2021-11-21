using System;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Media.Imaging;
using Flurl.Http;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Tabs
{
    public class AboutTabViewModel : ActivatableViewModelBase
    {
        private Bitmap? _darthAffeProfileImage;
        private Bitmap? _drMeteorProfileImage;
        private Bitmap? _kaiProfileImage;
        private Bitmap? _robertProfileImage;
        private string? _version;

        public AboutTabViewModel()
        {
            DisplayName = "About";
            this.WhenActivated((CompositeDisposable _) => Task.Run(Activate));
        }

        public string? Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }

        public Bitmap? RobertProfileImage
        {
            get => _robertProfileImage;
            set => this.RaiseAndSetIfChanged(ref _robertProfileImage, value);
        }

        public Bitmap? DarthAffeProfileImage
        {
            get => _darthAffeProfileImage;
            set => this.RaiseAndSetIfChanged(ref _darthAffeProfileImage, value);
        }

        public Bitmap? DrMeteorProfileImage
        {
            get => _drMeteorProfileImage;
            set => this.RaiseAndSetIfChanged(ref _drMeteorProfileImage, value);
        }

        public Bitmap? KaiProfileImage
        {
            get => _kaiProfileImage;
            set => this.RaiseAndSetIfChanged(ref _kaiProfileImage, value);
        }

        private async Task Activate()
        {
            AssemblyInformationalVersionAttribute? versionAttribute = typeof(AboutTabViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Version = $"Version {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";

            try
            {
                RobertProfileImage = new Bitmap(await "https://avatars.githubusercontent.com/u/8858506".GetStreamAsync());
                RobertProfileImage = new Bitmap(await "https://avatars.githubusercontent.com/u/8858506".GetStreamAsync());
                DarthAffeProfileImage = new Bitmap(await "https://avatars.githubusercontent.com/u/1094841".GetStreamAsync());
                DrMeteorProfileImage = new Bitmap(await "https://avatars.githubusercontent.com/u/29486064".GetStreamAsync());
                KaiProfileImage = new Bitmap(await "https://i.imgur.com/8mPWY1j.png".GetStreamAsync());
            }
            catch (Exception)
            {
                // ignored, unluckyyyy
            }
        }
    }
}