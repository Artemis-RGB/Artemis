using System;
using System.Linq;
using System.Threading;
using Artemis.Core;
using Artemis.Core.Ninject;
using Artemis.Core.Services;
using Ninject;

namespace Artemis.UI.Console
{
    /// <summary>
    ///     This is just a little experiment to show that Artemis can run without the UI and even on other OSes
    ///     Some notes
    ///     - Any plugin relying on WPF and/or Artemis.UI.Shared won't load
    ///     - There is no input provider so key-press events and brushes won't work
    ///     - Device providers using Windows SDKs won't work, OpenRGB will though!
    ///     - You may need to fiddle around to get SkiaSharp binaries going
    ///     - There is no UI obviously
    /// </summary>
    internal class Program
    {
        private static readonly AutoResetEvent Closing = new(false);

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Closing.Set();
        }

        private static void Main(string[] args)
        {
            Utilities.PrepareFirstLaunch();
            Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            StandardKernel kernel = new() {Settings = {InjectNonPublic = true}};
            kernel.Load<CoreModule>();

            ICoreService core = kernel.Get<ICoreService>();
            core.StartupArguments = args.ToList();
            core.IsElevated = false;
            core.Initialize();

            System.Console.CancelKeyPress += OnExit;
            Closing.WaitOne();
        }

        private static void UtilitiesOnShutdownRequested(object sender, EventArgs e)
        {
            Closing.Set();
        }
    }
}