using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : Conductor<DataModelConditionGroupViewModel>, IProfileEditorPanelViewModel
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private RenderProfileElement _renderProfileElement;
        private bool _displayStartHint;
        private bool _isEventCondition;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDataModelConditionsVmFactory dataModelConditionsVmFactory, ICoreService coreService)
        {
            _profileEditorService = profileEditorService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;
            _coreService = coreService;
        }

        public bool DisplayStartHint
        {
            get => _displayStartHint;
            set => SetAndNotify(ref _displayStartHint, value);
        }

        public bool IsEventCondition
        {
            get => _isEventCondition;
            set => SetAndNotify(ref _isEventCondition, value);
        }

        public RenderProfileElement RenderProfileElement
        {
            get => _renderProfileElement;
            set
            {
                if (!SetAndNotify(ref _renderProfileElement, value)) return;
                NotifyOfPropertyChange(nameof(DisplayContinuously));
                NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
                NotifyOfPropertyChange(nameof(EventOverlapMode));
            }
        }

        public bool DisplayContinuously
        {
            get => RenderProfileElement?.Timeline.PlayMode == TimelinePlayMode.Repeat;
            set
            {
                TimelinePlayMode playMode = value ? TimelinePlayMode.Repeat : TimelinePlayMode.Once;
                if (RenderProfileElement == null || RenderProfileElement?.Timeline.PlayMode == playMode) return;
                RenderProfileElement.Timeline.PlayMode = playMode;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool AlwaysFinishTimeline
        {
            get => RenderProfileElement?.Timeline.StopMode == TimelineStopMode.Finish;
            set
            {
                TimelineStopMode stopMode = value ? TimelineStopMode.Finish : TimelineStopMode.SkipToEnd;
                if (RenderProfileElement == null || RenderProfileElement?.Timeline.StopMode == stopMode) return;
                RenderProfileElement.Timeline.StopMode = stopMode;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => RenderProfileElement?.Timeline.EventOverlapMode ?? TimeLineEventOverlapMode.Restart;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement?.Timeline.EventOverlapMode == value) return;
                RenderProfileElement.Timeline.EventOverlapMode = value;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool ConditionBehaviourEnabled => RenderProfileElement != null;

        protected override void OnInitialActivate()
        {
            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            base.OnClose();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            if (RenderProfileElement != null)
            {
                RenderProfileElement.DisplayCondition.ChildAdded -= DisplayConditionOnChildrenModified;
                RenderProfileElement.DisplayCondition.ChildRemoved -= DisplayConditionOnChildrenModified;
                RenderProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;
            }

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
            if (e.RenderProfileElement.DisplayCondition == null)
                e.RenderProfileElement.DisplayCondition = new DataModelConditionGroup(null);

            ActiveItem = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(e.RenderProfileElement.DisplayCondition, ConditionGroupType.General);
            ActiveItem.IsRootGroup = true;
            ActiveItem.Update();

            DisplayStartHint = !RenderProfileElement.DisplayCondition.Children.Any();
            IsEventCondition = RenderProfileElement.DisplayCondition.Children.Any(c => c is DataModelConditionEvent);

            RenderProfileElement.DisplayCondition.ChildAdded += DisplayConditionOnChildrenModified;
            RenderProfileElement.DisplayCondition.ChildRemoved += DisplayConditionOnChildrenModified;
            RenderProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;
        }

        private void TimelineOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(DisplayContinuously));
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            NotifyOfPropertyChange(nameof(EventOverlapMode));
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            ActiveItem?.Evaluate();
        }

        private void DisplayConditionOnChildrenModified(object sender, EventArgs e)
        {
            DisplayStartHint = !RenderProfileElement.DisplayCondition.Children.Any();
            IsEventCondition = RenderProfileElement.DisplayCondition.Children.Any(c => c is DataModelConditionEvent);
        }

        public void EventTriggerModeSelected()
        {
            _profileEditorService.UpdateSelectedProfileElement();
        }
    }
}