using System;
using System.Reactive.Linq;
using Material.Icons;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class SidebarScreenViewModel<T> : SidebarScreenViewModel where T : MainScreenViewModel
    {
        public SidebarScreenViewModel(MaterialIconKind icon, string displayName) : base(icon, displayName)
        {
        }

        public override MainScreenViewModel CreateInstance(IKernel kernel)
        {
            return kernel.Get<T>();
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

        public abstract MainScreenViewModel CreateInstance(IKernel kernel);

        public bool IsActive(IObservable<IRoutableViewModel?> routerCurrentViewModel)
        {
            return false;
        }
    }
}