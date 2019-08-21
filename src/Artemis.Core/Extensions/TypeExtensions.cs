using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsGenericType(this Type type, Type genericType)
        {
            if (type == null)
                return false;
            var baseType = type.BaseType;
            if (baseType == null)
                return false;

            return baseType.GetGenericTypeDefinition() == genericType;
        }
    }
}
