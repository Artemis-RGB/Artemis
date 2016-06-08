using System.IO;
using System.IO.Compression;
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

        #region Zip files

        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (var file in archive.Entries)
            {
                var completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                if (file.Name == "")
                {
                    // Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }

        #endregion

        #region Color

        // TODO: Convert ColorHelpers to ExtensionMethods

        #endregion

        #region Reflection

        /// <summary>
        ///     Gets a value by path
        ///     jheddings - http://stackoverflow.com/a/1954663/5015269
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name">Path, such as "TimeOfDay.Minutes"</param>
        /// <returns></returns>
        public static object GetPropValue(this object obj, string name)
        {
            foreach (var part in name.Split('.'))
            {
                if (obj == null)
                    return null;

                var type = obj.GetType();
                var info = type.GetProperty(part);
                if (info == null)
                    return null;

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        /// <summary>
        ///     Gets a value by path
        ///     jheddings - http://stackoverflow.com/a/1954663/5015269
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetPropValue<T>(this object obj, string name)
        {
            if (name == null)
                return default(T);

            var retVal = GetPropValue(obj, name);
            if (retVal == null)
                return default(T);

            // throws InvalidCastException if types are incompatible
            return (T) retVal;
        }

        #endregion
    }
}