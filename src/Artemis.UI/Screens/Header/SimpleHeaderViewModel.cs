using System.Timers;
using Artemis.Core.Services;
using Artemis.UI.Services;
using Stylet;

namespace Artemis.UI.Screens.Header
{
    public class SimpleHeaderViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private string _frameTime;
        private Timer _frameTimeUpdateTimer;

        public SimpleHeaderViewModel(string displayName, ICoreService coreService, IDebugService debugService)
        {
            DisplayName = displayName;

            _coreService = coreService;
            _debugService = debugService;
        }
        
        public string FrameTime
        {
            get => _frameTime;
            set => SetAndNotify(ref _frameTime, value);
        }

        public void ShowDebugger()
        {
            _debugService.ShowDebugger();
        }

        private void UpdateFrameTime()
        {
            FrameTime = $"Frame time: {_coreService.FrameTime.TotalMilliseconds:F2} ms";
        }

        private void OnFrameTimeUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            UpdateFrameTime();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            _frameTimeUpdateTimer = new Timer(500);
            _frameTimeUpdateTimer.Elapsed += OnFrameTimeUpdateTimerOnElapsed;
            _frameTimeUpdateTimer.Start();

            UpdateFrameTime();
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            _frameTimeUpdateTimer.Elapsed -= OnFrameTimeUpdateTimerOnElapsed;
            _frameTimeUpdateTimer?.Dispose();
            _frameTimeUpdateTimer = null;

            base.OnClose();
        }

        #endregion
    }
}