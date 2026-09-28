namespace CartoPac_Mobile;

/// <summary>
/// How the map shows the app-owned location puck and responds to location updates.
/// </summary>
public enum LocationPuckMode
{
    /// <summary>No puck; camera is not driven by location fixes.</summary>
    Off,

    /// <summary>Draw the puck at each fix; camera stays where the user left it.</summary>
    Show,

    /// <summary>Draw the puck and ease the camera to each fix.</summary>
    Follow,
}
