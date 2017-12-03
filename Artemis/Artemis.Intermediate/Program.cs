using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Artemis.Intermediate
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Updating Artemis...\n");
            // Check GitHub for a new version
            var jsonClient = new WebClient();

            // GitHub trips if we don't add a user agent
            jsonClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            // Random number to get around cache issues
            var rand = new Random(DateTime.Now.Millisecond);
            // Grab the latest and greatest off of GitHub, it won't use Squirrel anymore.
            var json = jsonClient.DownloadString("https://api.github.com/repos/SpoinkyNL/Artemis/releases/latest?random=" + rand.Next());

            var release = JObject.Parse(json);

            // Download the release file, it's the one starting with "artemis-setup"
            var releaseFile = release["assets"].Children().FirstOrDefault(c => c["name"].Value<string>().StartsWith("artemis-setup") &&
                                                                               c["name"].Value<string>().EndsWith(".msi"));
            // If there's no matching release it means whoever published the new version fucked up, can't do much about that
            if (releaseFile == null)
            {
                Console.WriteLine("Couldn't find the update file. Please install the latest version manually, sorry!");
                return;
            }

            Console.WriteLine($"Found release: {release["tag_name"].Value<string>()}");
            var downloadClient = new WebClient();
            downloadClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Console.WriteLine($"Downloading {releaseFile["browser_download_url"].Value<string>()}..");
            var setupBytes = downloadClient.DownloadData(releaseFile["browser_download_url"].Value<string>());

            // Ensure the update folder exists
            var updateFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Artemis\\updates";
            var updatePath = updateFolder + "\\" + releaseFile["name"].Value<string>();
            if (!Directory.Exists(updateFolder))
                Directory.CreateDirectory(updateFolder);

            // Store the bytes
            File.WriteAllBytes(updatePath, setupBytes);
            // Create a bat file that'll take care of the installation (Artemis gets shut down during install) the bat file will
            // carry forth our legacy (read that in an heroic tone)
            Console.WriteLine("Running setup..");
            var updateScript = "ECHO OFF\r\n" +
                               "CLS\r\n" +
                               $"\"{updatePath}\"";
            File.WriteAllText(updateFolder + "\\updateScript.bat", updateScript);
            var psi = new ProcessStartInfo
            {
                FileName = updateFolder + "\\updateScript.bat",
                Verb = "runas"
            };

            var process = new Process {StartInfo = psi};
            process.Start();
            process.WaitForExit();
            Console.WriteLine("\nDone! You can now close this window and start Artemis using the start menu shortcut.");
            Console.WriteLine("Note: This updater will be removed automatically the next time you run Artemis");
        }
    }
}
