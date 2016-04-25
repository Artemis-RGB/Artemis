using System;

namespace Artemis.Utilities.GameState
{
    public class GameDataReceivedEventArgs : EventArgs
    {
        public GameDataReceivedEventArgs(object json)
        {
            Json = json;
        }

        public object Json { get; set; }
    }
}