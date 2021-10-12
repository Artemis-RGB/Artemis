using System;
using Material.Icons;
using Ninject;
using Ninject.Parameters;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class SidebarScreenViewModel<T> : SidebarScreenViewModel where T : MainScreenViewModel
    {
        public SidebarScreenViewModel(MaterialIconKind icon, string displayName) : base(icon, displayName)
        {
        }

        public override Type ScreenType => typeof(T);

        public override MainScreenViewModel CreateInstance(IKernel kernel, IScreen screen)
        {
            return kernel.Get<T>(new ConstructorArgument("hostScreen", screen));
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
        public string DisplayName { get; }

        public abstract Type ScreenType { get; }
        public abstract MainScreenViewModel CreateInstance(IKernel kernel, IScreen screen);
    }
}