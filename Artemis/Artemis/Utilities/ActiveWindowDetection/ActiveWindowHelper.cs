namespace Artemis.Utilities.ActiveWindowDetection
{
    public static class ActiveWindowHelper
    {
        #region Properties & Fields

        private static ActiveWindowDetectionType _currentDetectionType = ActiveWindowDetectionType.Disabled;
        private static IActiveWindowDetector _activeWindowDetector;

        public static string ActiveWindowProcessName => _activeWindowDetector?.ActiveWindowProcessName ?? string.Empty;
        public static string ActiveWindowWindowTitle => _activeWindowDetector?.ActiveWindowWindowTitle ?? string.Empty;
        public static bool MainWindowActive => ActiveWindowProcessName.Contains("Artemis");

        #endregion

        #region Methods

        public static void Dispose()
        {
            _activeWindowDetector?.Dispose();
        }

        public static void SetActiveWindowDetectionType(ActiveWindowDetectionType detectionType)
        {
            if (detectionType == _currentDetectionType) return;

            _activeWindowDetector?.Dispose();

            switch (detectionType)
            {
                case ActiveWindowDetectionType.Events:
                    _activeWindowDetector = new EventActiveWindowDetector();
                    break;
                case ActiveWindowDetectionType.Timer:
                    _activeWindowDetector = new TimerActiveWindowDetector();
                    break;
                case ActiveWindowDetectionType.Disabled:
                    _activeWindowDetector = null;
                    break;
            }

            _activeWindowDetector?.Initialize();
            _currentDetectionType = detectionType;
        }

        #endregion
    }
}