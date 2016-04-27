using System.Collections.Generic;
using System.Drawing;
using Artemis.KeyboardProviders.Razer.Utilities;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using ColoreColor = Corale.Colore.Core.Color;
using Constants = Corale.Colore.Razer.Keyboard.Constants;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Artemis.KeyboardProviders.Razer
{
    public class BlackWidow : KeyboardProvider
    {
        public BlackWidow()
        {
            Name = "Razer BlackWidow Chroma";
            CantEnableText = "Couldn't connect to your Razer BlackWidow Chroma.\n " +
                             "Please check your cables and try updating Razer Synapse.\n\n " +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            KeyboardRegions = new List<KeyboardRegion>
            {
                new KeyboardRegion("TopRow", new Point(0, 0), new Point(19, 0)),
                new KeyboardRegion("NumPad", new Point(20, 1), new Point(23, 6)),
                new KeyboardRegion("QWER", new Point(2, 2), new Point(5, 2))
            };
        }

        public override bool CanEnable()
        {
            if (!Chroma.IsSdkAvailable())
                return false;

            // Some people have Synapse installed, but not a Chroma keyboard, deal with this
            var blackWidowFound = Chroma.Instance.Query(Devices.Blackwidow).Connected;
            var blackWidowTeFound = Chroma.Instance.Query(Devices.BlackwidowTe).Connected;
            return blackWidowFound || blackWidowTeFound;
        }

        public override void Enable()
        {
            Chroma.Instance.Initialize();
            Height = Constants.MaxRows;
            Width = Constants.MaxColumns;
        }

        public override void Disable()
        {
            Chroma.Instance.Uninitialize();
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            var razerArray = RazerUtilities.BitmapColorArray(bitmap, Height, Width);

            Chroma.Instance.Keyboard.SetCustom(razerArray);
        }
    }
}