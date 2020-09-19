using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : Conductor<DisplayConditionGroupViewModel>, IProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private RenderProfileElement _renderProfileElement;
        private bool _displayStartHint;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDisplayConditionsVmFactory displayConditionsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
        }

        public bool DisplayStartHint
        {
            get => _displayStartHint;
            set => SetAndNotify(ref _displayStartHint, value);
        }

        public RenderProfileElement RenderProfileElement
        {
            get => _renderProfileElement;
            set => SetAndNotify(ref _renderProfileElement, value);
        }

        public bool DisplayContinuously
        {
            get => RenderProfileElement?.DisplayContinuously ?? false;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement.DisplayContinuously == value) return;
                RenderProfileElement.DisplayContinuously = value;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool AlwaysFinishTimeline
        {
            get => RenderProfileElement?.AlwaysFinishTimeline ?? false;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement.AlwaysFinishTimeline == value) return;
                RenderProfileElement.AlwaysFinishTimeline = value;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool ConditionBehaviourEnabled => RenderProfileElement != null;

        protected override void OnInitialActivate()
        {
            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            base.OnClose();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            RenderProfileElement = e.RenderProfileElement;
            NotifyOfPropertyChange(nameof(DisplayContinuously));
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            NotifyOfPropertyChange(nameof(ConditionBehaviourEnabled));

            if (e.RenderProfileElement == null)
            {
                ActiveItem = null;
                return;
            }

            // Ensure the layer has a root display condition group
            if (e.RenderProfileElement.DataModelConditionGroup == null)
                e.RenderProfileElement.DataModelConditionGroup = new DataModelConditionGroup(null);

            ActiveItem = _displayConditionsVmFactory.DisplayConditionGroupViewModel(e.RenderProfileElement.DataModelConditionGroup, false);
            ActiveItem.IsRootGroup = true;
            ActiveItem.Update();

            // Only show the intro to conditions once, and only if the layer has no conditions
            DisplayStartHint = !ActiveItem.Items.Any();
        }
    }
}