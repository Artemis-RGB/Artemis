using System;
using System.Collections.Generic;
using System.Reflection;
using Stylet;

namespace Artemis.UI.Stylet
{
    public class ArtemisViewManager : ViewManager
    {
        public ArtemisViewManager(ViewManagerConfig config) : base(config)
        {
        }

        protected override string ViewTypeNameForModelTypeName(string modelTypeName)
        {
            var cleaned = modelTypeName.Split('`')[0];
            return base.ViewTypeNameForModelTypeName(cleaned);
        }
    }
}