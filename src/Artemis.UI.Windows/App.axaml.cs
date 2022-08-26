using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Windows.Ninject;
using Artemis.UI.Windows.Providers.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Windows;

public class App : Application
{
    private StandardKernel? _kernel;
    private bool _shutDown;

    // ReSharper disable NotAccessedField.Local
    private ApplicationStateManager? _applicationStateManager;
    private Mutex? _artemisMutex;
    // ReSharper restore NotAccessedField.Local

    public override void Initialize()
    {
        // If Artemis is already running, bring it to foreground and stop this process
        if (FocusExistingInstance())
        {
            _shutDown = true;
            Environment.Exit(1);
        }

        _kernel = ArtemisBootstrapper.Bootstrap(this, new WindowsModule());
        Program.CreateLogger(_kernel);
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || Design.IsDesignMode || _shutDown)
            return;

        ArtemisBootstrapper.Initialize();
        _applicationStateManager = new ApplicationStateManager(_kernel!, desktop.Args);
        RegisterProviders(_kernel!);
    }

    private void RegisterProviders(StandardKernel standardKernel)
    {
        IInputService inputService = standardKernel.Get<IInputService>();
        inputService.AddInputProvider(standardKernel.Get<WindowsInputProvider>());
    }

    private bool FocusExistingInstance()
    {
        _artemisMutex = new Mutex(true, "Artemis-3c24b502-64e6-4587-84bf-9072970e535f", out bool createdNew);
        return !createdNew && RemoteFocus();
    }

    private bool RemoteFocus()
    {
        // At this point we cannot read the database yet to retrieve the web server port.
        // Instead use the method external applications should use as well.
        if (!File.Exists(Path.Combine(Constants.DataFolder, "webserver.txt")))
        {
            KillOtherInstances();
            return false;
        }

        string url = File.ReadAllText(Path.Combine(Constants.DataFolder, "webserver.txt"));
        using HttpClient client = new();
        try
        {
            CancellationTokenSource cts = new();
            cts.CancelAfter(2000);

            HttpResponseMessage httpResponseMessage = client.Send(new HttpRequestMessage(HttpMethod.Post, url + "remote/bring-to-foreground"), cts.Token);
            httpResponseMessage.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception)
        {
            KillOtherInstances();
            return false;
        }
    }

    private void KillOtherInstances()
    {
        // Kill everything else heh
        List<Process> processes = Process.GetProcessesByName("Artemis.UI.Windows").Where(p => p.Id != Process.GetCurrentProcess().Id).ToList();
        foreach (Process process in processes)
        {
            try
            {
                process.Kill(true);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}