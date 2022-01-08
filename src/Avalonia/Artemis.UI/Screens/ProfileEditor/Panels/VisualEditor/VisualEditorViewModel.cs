using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Services.ProfileEditor;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor
{
    public class VisualEditorViewModel : ActivatableViewModelBase
    {
        private ProfileConfiguration? _profileConfiguration;

        public VisualEditorViewModel(IProfileEditorService profileEditorService, IRgbService rgbService)
        {
            Devices = new ObservableCollection<ArtemisDevice>(rgbService.EnabledDevices);
            this.WhenActivated(d => profileEditorService.ProfileConfiguration.Subscribe(configuration => ProfileConfiguration = configuration).DisposeWith(d));
        }

        public ProfileConfiguration? ProfileConfiguration
        {
            get => _profileConfiguration;
            set => this.RaiseAndSetIfChanged(ref _profileConfiguration, value);
        }

        public ObservableCollection<ArtemisDevice> Devices { get; }
    }
}