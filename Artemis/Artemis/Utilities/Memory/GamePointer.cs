using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Utilities.Memory
{
    public class GamePointer
    {
        public string Description { get; set; }
        public IntPtr BasePointer { get; set; }
        public int[] Offsets { get; set; }

        public override string ToString()
        {
            return Offsets.Aggregate(BasePointer.ToString("X"),
                (current, offset) => current + $"+{offset.ToString("X")}");
        }
    }

    public class GamePointersCollection
    {
        public string Game { get; set; }
        public string GameVersion { get; set; }
        public List<GamePointer> GameAddresses { get; set; }
    }
}