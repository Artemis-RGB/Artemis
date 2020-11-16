using System.IO;

namespace Artemis.Core
{
    internal static class DirectoryInfoExtensions
    {
        public static void CopyFilesRecursively(this DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public static void DeleteRecursively(this DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (DirectoryInfo dir in baseDir.EnumerateDirectories())
                DeleteRecursively(dir);
            FileInfo[] files = baseDir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            baseDir.Delete();
        }
    }
}