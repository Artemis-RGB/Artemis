using System.Collections.Generic;
using Artemis.Core.Plugins.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Models
{
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string MainFile { get; set; }
        public IReadOnlyList<string> SubFiles { get; set; }

        [JsonIgnore]
        public IPlugin Plugin { get; set; }
    }
}