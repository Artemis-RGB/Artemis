using System;
using Artemis.UI.Avalonia.Shared;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Artemis.UI.Avalonia
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            string name = data.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
            Type? type = Type.GetType(name);

            if (type != null)
                return (Control) Activator.CreateInstance(type)!;
            return new TextBlock {Text = "Not Found: " + name};
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}