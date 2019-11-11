using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
   public class ProfileEditorViewModel : ModuleViewModel
    {
        public ProfileEditorViewModel(Module module) : base(module, "Profile Editor")
        {
        }
    }
}
