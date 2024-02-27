using System.Text.Json.Serialization;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile.Abstract;

[JsonDerivedType(typeof(AlwaysOnConditionEntity), "AlwaysOn")]
[JsonDerivedType(typeof(EventConditionEntity), "Event")]
[JsonDerivedType(typeof(PlayOnceConditionEntity), "PlayOnce")]
[JsonDerivedType(typeof(StaticConditionEntity), "Static")]
public interface IConditionEntity;