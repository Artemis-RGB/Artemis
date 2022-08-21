using System;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
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
        if (Application.Current?.Clipboard == null)
            return;

        DataObject dataObject = new();
        string copy = CoreJson.SerializeObject(folder.FolderEntity, true);
        dataObject.Set(ClipboardDataFormat, copy);
        await Application.Current.Clipboard.SetDataObjectAsync(dataObject);
    }

    public static async Task CopyToClipboard(this Layer layer)
    {
        if (Application.Current?.Clipboard == null)
            return;

        DataObject dataObject = new();
        string copy = CoreJson.SerializeObject(layer.LayerEntity, true);
        dataObject.Set(ClipboardDataFormat, copy);
        await Application.Current.Clipboard.SetDataObjectAsync(dataObject);
    }


    public static async Task<RenderProfileElement?> PasteChildFromClipboard(this Folder parent)
    {
        if (Application.Current?.Clipboard == null)
            return null;

        byte[]? bytes = (byte[]?) await Application.Current.Clipboard.GetDataAsync(ClipboardDataFormat);
        if (bytes == null!)
            return null;

        object? entity = CoreJson.DeserializeObject(Encoding.Unicode.GetString(bytes), true);
        switch (entity)
        {
            case FolderEntity folderEntity:
                folderEntity.Id = Guid.NewGuid();
                return new Folder(parent.Profile, parent, folderEntity);
            case LayerEntity layerEntity:
                layerEntity.Id = Guid.NewGuid();
                return new Layer(parent.Profile, parent, layerEntity);
            default:
                return null;
        }
    }
}