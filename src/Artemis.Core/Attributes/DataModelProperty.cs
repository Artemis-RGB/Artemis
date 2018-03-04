using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core.Attributes
{
    public class DataModelPropertyAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }
}
