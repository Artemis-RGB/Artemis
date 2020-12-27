using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Artemis.UI.Installer.Screens.Steps.Prerequisites;
using Artemis.UI.Installer.Services;
using Stylet;

namespace Artemis.UI.Installer.Screens.Steps
{
    public class PrerequisitesStepViewModel : ConfigurationStep
    {
        private bool _canContinue;

        private bool _displayDownloadButton;
        private bool _displayProcess;
        private double _downloadCurrent;
        private int _downloadProcess;
        private double _downloadTotal;
        private bool _installing;
        private PrerequisiteViewModel _subject;

        public PrerequisitesStepViewModel(IInstallationService installationService)
        {
            Prerequisites = new BindableCollection<PrerequisiteViewModel>(installationService.Prerequisites.Select(p => new PrerequisiteViewModel(p)));
        }

        public BindableCollection<PrerequisiteViewModel> Prerequisites { get; }

        public bool CanContinue
        {
            get => _canContinue;
            set => SetAndNotify(ref _canContinue, value);
        }

        public PrerequisiteViewModel Subject
        {
            get => _subject;
            set => SetAndNotify(ref _subject, value);
        }

        public bool DisplayDownloadButton
        {
            get => _displayDownloadButton;
            set => SetAndNotify(ref _displayDownloadButton, value);
        }

        public bool DisplayProcess
        {
            get => _displayProcess;
            set => SetAndNotify(ref _displayProcess, value);
        }

        public bool Installing
        {
            get => _installing;
            set => SetAndNotify(ref _installing, value);
        }

        public int DownloadProcess
        {
            get => _downloadProcess;
            set => SetAndNotify(ref _downloadProcess, value);
        }

        public double DownloadCurrent
        {
            get => _downloadCurrent;
            set => SetAndNotify(ref _downloadCurrent, value);
        }

        public double DownloadTotal
        {
            get => _downloadTotal;
            set => SetAndNotify(ref _downloadTotal, value);
        }

        public override int Order => 2;

        public void Update()
        {
            foreach (PrerequisiteViewModel prerequisiteViewModel in Prerequisites)
                prerequisiteViewModel.Update();

            CanContinue = Prerequisites.All(p => p.IsMet);
            DisplayDownloadButton = !DisplayProcess && Prerequisites.Any(p => !p.IsMet);
        }

        public async Task InstallMissing()
        {
            DisplayDownloadButton = false;
            DisplayProcess = true;

            foreach (PrerequisiteViewModel prerequisiteViewModel in Prerequisites)
            {
                if (prerequisiteViewModel.IsMet) continue;
                using (WebClient client = new WebClient())
                {
                    Subject = prerequisiteViewModel;

                    client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
                    string runtimeFile = Path.GetTempFileName();
                    runtimeFile = runtimeFile.Replace(".tmp", ".exe");
                    await client.DownloadFileTaskAsync(prerequisiteViewModel.Prerequisite.DownloadUrl, runtimeFile);
                    client.DownloadProgressChanged -= ClientOnDownloadProgressChanged;

                    Installing = true;
                    await prerequisiteViewModel.Prerequisite.Install(runtimeFile);
                    File.Delete(runtimeFile);
                    Installing = false;
                }

                Update();
            }

            DisplayProcess = false;
            DisplayDownloadButton = Prerequisites.Any(p => !p.IsMet);
        }

        protected override void OnActivate()
        {
            Update();
            base.OnActivate();
        }
        
        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadTotal = e.TotalBytesToReceive / 1000000d;
            DownloadCurrent = e.BytesReceived / 1000000d;
            DownloadProcess = e.ProgressPercentage;
        }
    }
}