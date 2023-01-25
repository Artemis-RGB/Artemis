using System;
using Artemis.UI.Shared;
using DryIoc;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarScreenViewModel<T> : SidebarScreenViewModel where T : MainScreenViewModel
{
    public SidebarScreenViewModel(MaterialIconKind icon, string displayName) : base(icon, displayName)
    {
    }

    public override Type ScreenType => typeof(T);

    public override MainScreenViewModel CreateInstance(IContainer kernel, IScreen screen)
    {
        return kernel.Resolve<T>(new object[] { screen });
    }
}

public abstract class SidebarScreenViewModel : ViewModelBase
{
    protected SidebarScreenViewModel(MaterialIconKind icon, string displayName)
    {
        Icon = icon;
        DisplayName = displayName;
    }

    public MaterialIconKind Icon { get; }

    public abstract Type ScreenType { get; }
    public abstract MainScreenViewModel CreateInstance(IContainer kernel, IScreen screen);
}