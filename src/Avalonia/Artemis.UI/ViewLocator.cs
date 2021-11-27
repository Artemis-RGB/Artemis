using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace Artemis.UI
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            Type dataType = data.GetType();
            string name = dataType.FullName!.Split('`')[0].Replace("ViewModel", "View");
            Type? type = dataType.Assembly.GetType(name);

            if (type != null)
                return (Control) Activator.CreateInstance(type)!;
            return new TextBlock {Text = "Not Found: " + name};
        }

        public bool Match(object data)
        {
            return data is ReactiveObject;
        }
    }
}