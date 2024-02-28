using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Models;
using Artemis.UI.Shared.Extensions;
using Avalonia.Input;

namespace Artemis.UI.Extensions;

/// <summary>
///     Provides static extension methods for UI related profile element tasks.
/// </summary>
public static class ProfileElementExtensions
{
    public const string ClipboardDataFormat = "Artemis.ProfileElement";

    public static async Task CopyToClipboard(this Folder folder)
    {
        DataObject dataObject = new();
        string copy = CoreJson.Serialize(new FolderClipboardModel(folder));
        dataObject.Set(ClipboardDataFormat, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);
    }

    public static async Task CopyToClipboard(this Layer layer)
    {
        DataObject dataObject = new();
        string copy = CoreJson.Serialize(new LayerClipboardModel(layer));
        dataObject.Set(ClipboardDataFormat, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);
    }


    public static async Task<RenderProfileElement?> PasteChildFromClipboard(this Folder parent)
    {
        IClipboardModel? entity = await Shared.UI.Clipboard.GetJsonAsync<IClipboardModel>(ClipboardDataFormat);
        switch (entity)
        {
            case FolderClipboardModel folderClipboardModel:
                return folderClipboardModel.Paste(parent.Profile, parent);
            case LayerClipboardModel layerClipboardModel:
                return layerClipboardModel.Paste(parent);
            default:
                return null;
        }
    }
}