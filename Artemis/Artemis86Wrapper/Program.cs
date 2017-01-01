using System;
using Artemis86Wrapper.Intergrations.Skype;

namespace Artemis86Wrapper
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Artemis x86 wrapper");
            var skypeManager = new SkypeManager();
            skypeManager.Start();
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}