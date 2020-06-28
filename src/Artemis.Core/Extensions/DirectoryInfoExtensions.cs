using System.IO;

namespace Artemis.Core.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static void CopyFilesRecursively(this DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public static void DeleteRecursively(this DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
                DeleteRecursively(dir);
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            baseDir.Delete();
        }
    }
}