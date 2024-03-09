using System.Text.Json.Serialization;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile.Conditions;

[JsonDerivedType(typeof(AlwaysOnConditionEntity), "AlwaysOn")]
[JsonDerivedType(typeof(EventConditionEntity), "Event")]
[JsonDerivedType(typeof(PlayOnceConditionEntity), "PlayOnce")]
[JsonDerivedType(typeof(StaticConditionEntity), "Static")]
public interface IConditionEntity;