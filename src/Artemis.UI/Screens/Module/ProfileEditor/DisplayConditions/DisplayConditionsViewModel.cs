using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : ProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private DisplayConditionGroupViewModel _rootGroup;

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

        private void ProfileEditorServiceOnProfileElementSelected(object sender, ProfileElementEventArgs e)
        {
            if (e.ProfileElement is Layer layer)
            {
                // Ensure the layer has a root display condition group
                if (layer.DisplayConditionGroup == null)
                    layer.DisplayConditionGroup = new DisplayConditionGroup();

                RootGroup = _displayConditionsVmFactory.DisplayConditionGroupViewModel(layer.DisplayConditionGroup, null);
                RootGroup.IsRootGroup = true;
                RootGroup.Update();
            }
            else
                RootGroup = null;
        }
    }
}