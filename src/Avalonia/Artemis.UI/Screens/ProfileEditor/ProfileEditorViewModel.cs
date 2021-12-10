using System;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel
    {
        private ProfileConfiguration? _profile;

        /// <inheritdoc />
        public ProfileEditorViewModel(IScreen hostScreen, VisualEditorViewModel visualEditorViewModel, IProfileEditorService profileEditorService) : base(hostScreen, "profile-editor")
        {
            VisualEditorViewModel = visualEditorViewModel;
            this.WhenActivated(disposables => { profileEditorService.CurrentProfileConfiguration.WhereNotNull().Subscribe(p => Profile = p).DisposeWith(disposables); });
        }

        public VisualEditorViewModel VisualEditorViewModel { get; }

        public ProfileConfiguration? Profile
        {
            get => _profile;
            set => this.RaiseAndSetIfChanged(ref _profile, value);
        }

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }
    }
}