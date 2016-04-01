using System.Text.RegularExpressions;

namespace Artemis.Utilities
{
    public static class ExtensionMethods
    {
        #region String

        /// <summary>
        ///     Takes a string LikeThisOne and turns it into Like This One.
        ///     ZombieSheep - http://stackoverflow.com/a/5796793/5015269
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        #endregion

        #region Color

        // TODO: Convert ColorHelpers to ExtensionMethods

        #endregion
    }
}