using System.Diagnostics;
using System.Threading.Tasks;

namespace Artemis.UI.Installer.Utilities
{
    public static class ProcessUtilities
    {
        public static Task<int> RunProcessAsync(string fileName, string arguments)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            Process process = new Process
            {
                StartInfo = {FileName = fileName, Arguments = arguments},
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}