using System.Collections.Generic;
using Artemis.Core.ProfileElements.Interfaces;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements
{
    public class Profile : IProfileElement
    {
        public List<IProfileElement> Children { get; set; }

        public void Update()
        {
        }

        public void Render(IRGBDevice rgbDevice)
        {
            foreach (var profileElement in Children)
                profileElement.Render(rgbDevice);
        }
    }
}