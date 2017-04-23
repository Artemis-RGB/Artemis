using System;

namespace Artemis.Utilities.ActiveWindowDetection
{
    public interface IActiveWindowDetector : IDisposable
    {
        string ActiveWindowProcessName { get; }
        string ActiveWindowWindowTitle { get; }

        void Initialize();
    }
}
