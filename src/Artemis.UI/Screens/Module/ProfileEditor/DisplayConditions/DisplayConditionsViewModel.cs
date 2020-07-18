using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : ProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private DisplayConditionGroupViewModel _rootGroup;
        private RenderProfileElement _renderProfileElement;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDisplayConditionsVmFactory displayConditionsVmFactory)
        {
            _displayConditionsVmFactory = displayConditionsVmFactory;
            profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
        }

        public DisplayConditionGroupViewModel RootGroup
        {
            get => _rootGroup;
            set => SetAndNotify(ref _rootGroup, value);
        }

        public RenderProfileElement RenderProfileElement
        {
            get => _renderProfileElement;
            set => SetAndNotify(ref _renderProfileElement, value);
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            RenderProfileElement = e.RenderProfileElement;

            if (e.RenderProfileElement == null)
            {
                RootGroup = null;
                return;
            }

            // Ensure the layer has a root display condition group
            if (e.RenderProfileElement.DisplayConditionGroup == null)
                e.RenderProfileElement.DisplayConditionGroup = new DisplayConditionGroup(null);

            RootGroup = _displayConditionsVmFactory.DisplayConditionGroupViewModel(e.RenderProfileElement.DisplayConditionGroup, null);
            RootGroup.IsRootGroup = true;
            RootGroup.Update();
        }
    }
}