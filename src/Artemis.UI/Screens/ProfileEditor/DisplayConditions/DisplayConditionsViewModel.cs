using System.ComponentModel;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : Conductor<DataModelConditionGroupViewModel>, IProfileEditorPanelViewModel
    {
        private readonly INodeVmFactory _nodeVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IWindowManager _windowManager;
        private bool _isEventCondition;
        private RenderProfileElement _renderProfileElement;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IWindowManager windowManager, INodeVmFactory nodeVmFactory)
        {
            _profileEditorService = profileEditorService;
            _windowManager = windowManager;
            _nodeVmFactory = nodeVmFactory;
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
                _profileEditorService.SaveSelectedProfileElement();
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
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => RenderProfileElement?.Timeline.EventOverlapMode ?? TimeLineEventOverlapMode.Restart;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement?.Timeline.EventOverlapMode == value) return;
                RenderProfileElement.Timeline.EventOverlapMode = value;
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public bool ConditionBehaviourEnabled => RenderProfileElement != null;

        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedProfileElementChanged += SelectedProfileEditorServiceOnSelectedProfileElementChanged;
            Update(_profileEditorService.SelectedProfileElement);

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.SelectedProfileElementChanged -= SelectedProfileEditorServiceOnSelectedProfileElementChanged;
            base.OnClose();
        }

        private void Update(RenderProfileElement renderProfileElement)
        {
            if (RenderProfileElement != null) RenderProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;

            RenderProfileElement = renderProfileElement;

            NotifyOfPropertyChange(nameof(DisplayContinuously));
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            NotifyOfPropertyChange(nameof(ConditionBehaviourEnabled));

            if (renderProfileElement == null)
            {
                ActiveItem = null;
                return;
            }

            RenderProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;
        }

        #region Event handlers

        public void ScriptGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            RenderProfileElement.DisplayCondition ??= new NodeScript<bool>("End Result", "");

            _windowManager.ShowDialog(_nodeVmFactory.NodeScriptWindowViewModel(RenderProfileElement.DisplayCondition));
            _profileEditorService.SaveSelectedProfileElement();
        }

        public void EventTriggerModeSelected()
        {
            _profileEditorService.SaveSelectedProfileElement();
        }

        private void SelectedProfileEditorServiceOnSelectedProfileElementChanged(object sender, RenderProfileElementEventArgs e)
        {
            Update(e.RenderProfileElement);
        }

        private void TimelineOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(DisplayContinuously));
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            NotifyOfPropertyChange(nameof(EventOverlapMode));
        }

        #endregion
    }
}