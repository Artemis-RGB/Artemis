//css_inc GeneralViewModel.cs;
//css_inc GeneralDataModel.cs;

using System;
using Artemis.Plugins.Abstract;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralModule : ProfileModule
    {
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