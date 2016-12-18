using System;
using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public static class EnumOperations
    {
        public static T StringToEnum<T>(string name)
        {
            return (T) Enum.Parse(typeof(T), name);
        }
    }

    public static class EnumGetDescription
    {
        public static string GetDescription(this Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            var attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
                return enumObj.ToString();
            var attrib = attribArray[0] as DescriptionAttribute;
            return attrib.Description;
        }
    }
}