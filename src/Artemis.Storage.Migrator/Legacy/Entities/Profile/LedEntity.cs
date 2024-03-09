namespace Artemis.Storage.Migrator.Legacy.Entities.Profile;

public class LedEntity
{
    public string LedName { get; set; } = string.Empty;
    public string DeviceIdentifier { get; set; } = string.Empty;

    public int? PhysicalLayout { get; set; }

    #region LedEntityEqualityComparer

    private sealed class LedEntityEqualityComparer : IEqualityComparer<LedEntity>
    {
        public bool Equals(LedEntity? x, LedEntity? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;
            return x.LedName == y.LedName && x.DeviceIdentifier == y.DeviceIdentifier && x.PhysicalLayout == y.PhysicalLayout;
        }

        public int GetHashCode(LedEntity obj)
        {
            return HashCode.Combine(obj.LedName, obj.DeviceIdentifier, obj.PhysicalLayout);
        }
    }

    public static IEqualityComparer<LedEntity> LedEntityComparer { get; } = new LedEntityEqualityComparer();

    #endregion
}