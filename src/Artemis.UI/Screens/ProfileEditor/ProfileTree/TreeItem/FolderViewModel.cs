using System.ComponentModel;
using Artemis.Core;
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