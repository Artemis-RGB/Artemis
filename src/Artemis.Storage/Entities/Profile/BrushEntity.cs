using System;

namespace Artemis.Storage.Entities.Profile
{
    public class BrushEntity
    {
        public Guid BrushPluginGuid { get; set; }
        public string BrushType { get; set; }
        public string Configuration { get; set; }
    }
}