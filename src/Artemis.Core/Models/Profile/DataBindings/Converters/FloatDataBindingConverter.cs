using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Models.Profile.DataBindings.Converters
{
   public class FloatDataBindingConverter : IDataBindingConverter
    {
        public BaseLayerProperty BaseLayerProperty { get; set; }
        public object Sum(object a, object b)
        {
            return (float) a + (float) b;
        }

        public object Interpolate(object a, object b, float progress)
        {
            throw new NotImplementedException();
        }

        public void ApplyValue(object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
