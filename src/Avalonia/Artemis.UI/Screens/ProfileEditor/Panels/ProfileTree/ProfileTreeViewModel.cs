using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : ActivatableViewModelBase
    {
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory)
        {
            _profileEditorVmFactory = profileEditorVmFactory;
            this.WhenActivated(d => profileEditorService.CurrentProfileConfiguration.WhereNotNull().Subscribe(Repopulate).DisposeWith(d));
        }

        public ObservableCollection<TreeItemViewModel> TreeItems { get; } = new();

        private void Repopulate(ProfileConfiguration profileConfiguration)
        {
            if (TreeItems.Any())
                TreeItems.Clear();

            if (profileConfiguration.Profile == null)
                return;

            foreach (ProfileElement profileElement in profileConfiguration.Profile.GetRootFolder().Children)
            {
                if (profileElement is Folder folder)
                    TreeItems.Add(_profileEditorVmFactory.FolderTreeItemViewModel(folder));
                else if (profileElement is Layer layer)
                    TreeItems.Add(_profileEditorVmFactory.LayerTreeItemViewModel(layer));
            }
            
        }
    }
}
