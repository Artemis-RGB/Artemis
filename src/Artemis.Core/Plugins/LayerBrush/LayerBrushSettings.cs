using System;
using Stylet;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrushSettings : PropertyChangedBase
    {
        private Action<Action> _propertyChangedDispatcher = Execute.DefaultPropertyChangedDispatcher;

        /// <summary>
        /// Gets or sets the dispatcher to use to dispatch PropertyChanged events. Defaults to Execute.DefaultPropertyChangedDispatcher
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public override Action<Action> PropertyChangedDispatcher
        {
            get { return this._propertyChangedDispatcher; }
            set { this._propertyChangedDispatcher = value; }
        }
    }
}