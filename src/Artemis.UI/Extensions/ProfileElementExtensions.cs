using System;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Models;
using Avalonia;
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
        string copy = CoreJson.SerializeObject(new FolderClipboardModel(folder), true);
        dataObject.Set(ClipboardDataFormat, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);
    }

    public static async Task CopyToClipboard(this Layer layer)
    {
        DataObject dataObject = new();
        string copy = CoreJson.SerializeObject(layer.LayerEntity, true);
        dataObject.Set(ClipboardDataFormat, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);
    }


    public static async Task<RenderProfileElement?> PasteChildFromClipboard(this Folder parent)
    {
        byte[]? bytes = (byte[]?) await Shared.UI.Clipboard.GetDataAsync(ClipboardDataFormat);
        if (bytes == null!)
            return null;

        object? entity = CoreJson.DeserializeObject(Encoding.Unicode.GetString(bytes), true);
        switch (entity)
        {
            case FolderClipboardModel folderClipboardModel:
                return folderClipboardModel.Paste(parent.Profile, parent);
            case LayerEntity layerEntity:
                layerEntity.Id = Guid.NewGuid();
                return new Layer(parent.Profile, parent, layerEntity, true);
            default:
                return null;
        }
    }
}