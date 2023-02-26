using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.DryIoc;
using Artemis.UI.DryIoc;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared.DataModelPicker;
using Artemis.UI.Shared.DryIoc;
using Artemis.UI.Shared.Services;
using Artemis.VisualScripting.DryIoc;
using Artemis.WebClient.Updating.DryIoc;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DryIoc;
using ReactiveUI;
using Splat.DryIoc;

namespace Artemis.UI;

public static class ArtemisBootstrapper
{
    private static Container? _container;
    private static Application? _application;

    public static IContainer Bootstrap(Application application, Action<IContainer>? configureServices = null)
    {
        if (_application != null || _container != null)
            throw new ArtemisUIException("UI already bootstrapped");

        Utilities.PrepareFirstLaunch();

        _application = application;
        _container = new Container(rules => rules
            .WithMicrosoftDependencyInjectionRules()
            .WithConcreteTypeDynamicRegistrations()
            .WithoutThrowOnRegisteringDisposableTransient());

        _container.RegisterCore();
        _container.RegisterUI();
        _container.RegisterSharedUI();
        _container.RegisterNoStringEvaluating();
        configureServices?.Invoke(_container);

        _container.UseDryIocDependencyResolver();
        return _container;
    }

    public static void Initialize()
    {
        if (_application == null || _container == null)
            throw new ArtemisUIException("UI not yet bootstrapped");
        if (_application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        Constants.StartupArguments = new ReadOnlyCollection<string>(new List<string>(desktop.Args));

        // Don't shut down when the last window closes, we might still be active in the tray
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        // Create the root view model that drives the UI
        RootViewModel rootViewModel = _container.Resolve<RootViewModel>();
        // Apply the root view model to the data context of the application so that tray icon commands work
        _application.DataContext = rootViewModel;

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(DisplayUnhandledException);
        DataModelPicker.DataModelUIService = _container.Resolve<IDataModelUIService>();
    }

    private static void DisplayUnhandledException(Exception exception)
    {
        _container?.Resolve<IWindowService>().ShowExceptionDialog("Exception", exception);
    }
}