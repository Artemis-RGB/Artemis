using System;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Material.Icons;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Categories;

public partial class CategoryViewModel : ViewModelBase
{
    [Notify] private bool _isSelected;

    public CategoryViewModel(IGetCategories_Categories category)
    {
        Id = category.Id;
        Name = category.Name;
        if (Enum.TryParse(typeof(MaterialIconKind), category.Icon, out object? icon))
            Icon = icon as MaterialIconKind? ?? MaterialIconKind.QuestionMarkCircle;
    }

    public long Id { get; }
    public string Name { get; }
    public MaterialIconKind Icon { get; }
}