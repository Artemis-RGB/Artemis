using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions.Static
{
    public class StaticConditionViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly INodeVmFactory _nodeVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly RenderProfileElement _renderProfileElement;

        public StaticConditionViewModel(StaticCondition staticCondition, IWindowManager windowManager, INodeVmFactory nodeVmFactory, IProfileEditorService profileEditorService)
        {
            _windowManager = windowManager;
            _nodeVmFactory = nodeVmFactory;
            _profileEditorService = profileEditorService;
            _renderProfileElement = (RenderProfileElement) staticCondition.ProfileElement;
            StaticCondition = staticCondition;
        }

        public StaticCondition StaticCondition { get; }

        public bool DisplayContinuously
        {
            get => _renderProfileElement.Timeline.PlayMode == TimelinePlayMode.Repeat;
            set
            {
                TimelinePlayMode playMode = value ? TimelinePlayMode.Repeat : TimelinePlayMode.Once;
                if (_renderProfileElement.Timeline.PlayMode == playMode) return;
                _renderProfileElement.Timeline.PlayMode = playMode;
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public bool AlwaysFinishTimeline
        {
            get => _renderProfileElement?.Timeline.StopMode == TimelineStopMode.Finish;
            set
            {
                TimelineStopMode stopMode = value ? TimelineStopMode.Finish : TimelineStopMode.SkipToEnd;
                if (_renderProfileElement.Timeline.StopMode == stopMode) return;
                _renderProfileElement.Timeline.StopMode = stopMode;
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public void ScriptGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            _windowManager.ShowDialog(_nodeVmFactory.NodeScriptWindowViewModel(StaticCondition.Script));
            _profileEditorService.SaveSelectedProfileElement();
        }
    }
}