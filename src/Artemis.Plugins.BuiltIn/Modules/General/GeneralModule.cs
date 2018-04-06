//css_inc GeneralViewModel.cs;
//css_inc GeneralDataModel.cs;

using System;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralModule : ProfileModule
    {
        private readonly IRgbService _rgbService;
        private RGBSurface _surface;

        public GeneralModule(IRgbService rgbService)
        {
            _rgbService = rgbService;
            _surface = _rgbService.Surface;

            
        }

        public override Type ViewModelType
        {
            get { return typeof(GeneralViewModel); }
        }

        // True since the main data model is all this module shows
        public override bool ExpandsMainDataModel
        {
            get { return true; }
        }
    }
}