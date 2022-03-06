using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class FolderTreeItemViewModel : TreeItemViewModel
    {
        public FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder, IWindowService windowService, IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory, IRgbService rgbService) 
            : base(parent, folder, windowService, profileEditorService, rgbService, profileEditorVmFactory)
        {
            Folder = folder;
        }

        public Folder Folder { get; }


        #region Overrides of TreeItemViewModel

        /// <inheritdoc />
        public override bool SupportsChildren => true;

        #endregion
    }
}