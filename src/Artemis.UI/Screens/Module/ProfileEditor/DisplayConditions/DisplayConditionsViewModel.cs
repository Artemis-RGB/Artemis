using System.Linq;
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
        private readonly IProfileEditorService _profileEditorService;
        private RenderProfileElement _renderProfileElement;
        private DisplayConditionGroupViewModel _rootGroup;
        private int _transitionerIndex;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDisplayConditionsVmFactory displayConditionsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
        }

        public int TransitionerIndex
        {
            get => _transitionerIndex;
            set => SetAndNotify(ref _transitionerIndex, value);
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

        public int ConditionBehaviourIndex
        {
            get => RenderProfileElement != null && RenderProfileElement.AlwaysFinishTimeline ? 0 : 1;
            set
            {
                if (RenderProfileElement == null)
                    return;

                RenderProfileElement.AlwaysFinishTimeline = value == 0;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool ConditionBehaviourEnabled => RenderProfileElement != null;

        protected override void OnActivate()
        {
            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
        }

        protected override void OnDeactivate()
        {
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;

            RootGroup?.Dispose();
            RootGroup = null;
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            RenderProfileElement = e.RenderProfileElement;
            NotifyOfPropertyChange(nameof(ConditionBehaviourIndex));
            NotifyOfPropertyChange(nameof(ConditionBehaviourEnabled));

            if (e.RenderProfileElement == null)
            {
                RootGroup?.Dispose();
                RootGroup = null;
                return;
            }

            // Ensure the layer has a root display condition group
            if (e.RenderProfileElement.DisplayConditionGroup == null)
                e.RenderProfileElement.DisplayConditionGroup = new DisplayConditionGroup(null);

            RootGroup?.Dispose();
            RootGroup = _displayConditionsVmFactory.DisplayConditionGroupViewModel(e.RenderProfileElement.DisplayConditionGroup, null);
            RootGroup.IsRootGroup = true;
            RootGroup.Update();

            // Only show the intro to conditions once, and only if the layer has no conditions
            if (TransitionerIndex != 1)
                TransitionerIndex = RootGroup.Children.Any() ? 1 : 0;
        }
    }
}