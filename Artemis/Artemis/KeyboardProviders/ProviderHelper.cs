using System.Collections.Generic;
using Artemis.KeyboardProviders.Corsair;
using Artemis.KeyboardProviders.Logitech;
using Artemis.KeyboardProviders.Razer;

namespace Artemis.KeyboardProviders
{
    public static class ProviderHelper
    {
        public static List<KeyboardProvider> GetKeyboardProviders()
        {
            return new List<KeyboardProvider>
            {
                new Orion(),
                new CorsairRGB()
                //new BlackWidow()
            };
        }
    }
}