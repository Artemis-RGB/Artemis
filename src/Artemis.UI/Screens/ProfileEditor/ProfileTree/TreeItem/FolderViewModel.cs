using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
        // I hate this about DI, oh well
        public FolderViewModel(ProfileElement folder,
            IRgbService rgbService,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService) :
            base(folder, rgbService, profileEditorService, dialogService, profileTreeVmFactory, layerBrushService)
        {
        }

        public override bool SupportsChildren => true;

        public override bool IsExpanded
        {
            get => ((Folder) ProfileElement).IsExpanded;
            set => ((Folder) ProfileElement).IsExpanded = value;
        }

        public override void UpdateBrokenState()
        {
            List<IBreakableModel> brokenModels = ProfileElement.GetBrokenHierarchy().ToList();
            if (!brokenModels.Any())
                BrokenState = null;
            else
            {
                BrokenState = "Folder is in a broken state, click to view exception(s).\r\n" +
                              $"{string.Join("\r\n", brokenModels.Select(e => $" • {e.BrokenDisplayName} - {e.BrokenState}"))}";
            }

            foreach (TreeItemViewModel treeItemViewModel in GetChildren())
                treeItemViewModel.UpdateBrokenState();
        }

        private void ProfileElementOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Folder.IsExpanded))
                NotifyOfPropertyChange(nameof(IsExpanded));
        }


        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            ProfileElement.PropertyChanged += ProfileElementOnPropertyChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ProfileElement.PropertyChanged -= ProfileElementOnPropertyChanged;
            base.OnClose();
        }

        #endregion
    }
}