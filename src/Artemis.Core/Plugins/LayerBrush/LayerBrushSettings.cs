using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Stylet;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrushSettings : PropertyChangedBase
    {
        /// <summary>
        ///     Gets or sets the dispatcher to use to dispatch PropertyChanged events. Defaults to
        ///     Execute.DefaultPropertyChangedDispatcher
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public override Action<Action> PropertyChangedDispatcher { get; set; } = Execute.DefaultPropertyChangedDispatcher;
    }
}