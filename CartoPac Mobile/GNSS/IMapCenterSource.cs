using GNSS;

namespace CartoPac_Mobile.GNSS;

public interface IMapCenterSource
{
    public GPSLocation? TryGetMapCenter();
}
