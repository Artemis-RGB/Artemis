using System.Text.Json.Serialization;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile.AdaptionHints;

[JsonDerivedType(typeof(CategoryAdaptionHintEntity), "Category")]
[JsonDerivedType(typeof(DeviceAdaptionHintEntity), "Device")]
[JsonDerivedType(typeof(KeyboardSectionAdaptionHintEntity), "KeyboardSection")]
[JsonDerivedType(typeof(SingleLedAdaptionHintEntity), "SingleLed")]
public interface IAdaptionHintEntity;