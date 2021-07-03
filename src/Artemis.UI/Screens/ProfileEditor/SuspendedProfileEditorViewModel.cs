using Artemis.Core;
using Artemis.UI.Events;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class SuspendedProfileEditorViewModel : MainScreenViewModel, IHandle<MainWindowFocusChangedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileEditorService _profileEditorService;
        private ProfileConfiguration _previousSelectedProfileConfiguration;

        public SuspendedProfileEditorViewModel(IEventAggregator eventAggregator, IProfileEditorService profileEditorService)
        {
            _eventAggregator = eventAggregator;
            _profileEditorService = profileEditorService;
        }

        public ProfileConfiguration PreviousSelectedProfileConfiguration
        {
            get => _previousSelectedProfileConfiguration;
            set => SetAndNotify(ref _previousSelectedProfileConfiguration, value);
        }

        public void Handle(MainWindowFocusChangedEvent message)
        {
            if (!message.IsFocused)
                return;

            RootViewModel rootViewModel = (RootViewModel) Parent;
            if (PreviousSelectedProfileConfiguration != null)
                rootViewModel.SidebarViewModel.SelectProfileConfiguration(PreviousSelectedProfileConfiguration);
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            PreviousSelectedProfileConfiguration = _profileEditorService.PreviousSelectedProfileConfiguration;
            _eventAggregator.Subscribe(this);
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _eventAggregator.Unsubscribe(this);
            base.OnClose();
        }

        #endregion
    }
}