using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions
{
    public static class EnumExtension
    {
        #region Methods

        public static Enum SetFlag(this Enum e, Enum value, bool set, Type t)
        {
            if (e == null || value == null || t == null) return e;

            int eValue = Convert.ToInt32(e);
            int valueValue = Convert.ToInt32(value);

            if (set)
                eValue |= valueValue;
            else
                eValue &= ~valueValue;

            return (Enum)Enum.ToObject(t, eValue);
        }

        #endregion
    }
}
