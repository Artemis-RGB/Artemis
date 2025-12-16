using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Artemis.UI.Shared.TagsInput;

public partial class TagsInput : TemplatedControl
{
    /// <summary>
    ///     Defines the <see cref="Tags" /> property
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<string>> TagsProperty =
        AvaloniaProperty.Register<TagsInput, ObservableCollection<string>>(nameof(Tags), new ObservableCollection<string>());

    /// <summary>
    ///     Gets or sets the selected tags.
    /// </summary>
    public ObservableCollection<string> Tags
    {
        get => GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }
    
    /// <summary>
    ///     Defines the <see cref="MaxLength" /> property
    /// </summary>
    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<TagsInput, int>(nameof(MaxLength), 20);
    
    /// <summary>
    ///     Gets or sets the max length of each tag
    /// </summary>
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }
    
    /// <summary>
    ///     Defines the <see cref="MaxTags" /> property
    /// </summary>
    public static readonly StyledProperty<int> MaxTagsProperty =
        AvaloniaProperty.Register<TagsInput, int>(nameof(MaxTags), 20);
    
    /// <summary>
    ///     Gets or sets the max amount of tags to be added
    /// </summary>
    public int MaxTags
    {
        get => GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, value);
    }
}