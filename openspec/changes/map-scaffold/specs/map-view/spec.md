## Purpose

Deliver an interactive street map with a fixed center selection indicator so users can pan and zoom to choose a geographic point without using device location services.

## ADDED Requirements

### Requirement: Interactive map display

The Map tab SHALL show a full-area interactive map that supports pan and zoom gestures on Android, iOS, and Windows.

#### Scenario: User pans the map

- **WHEN** the user drags on the map
- **THEN** the map view moves in response
- **AND** the map remains interactive

#### Scenario: User zooms the map

- **WHEN** the user performs a zoom gesture supported on the platform
- **THEN** the map zoom level changes
- **AND** the map remains interactive

### Requirement: Light street basemap

The map SHALL use a publicly reachable vector street style appropriate for a light, general-purpose street map at launch.

#### Scenario: Map loads style

- **WHEN** the Map tab is shown and network access is available
- **THEN** the map displays tiled street map content from the configured style
- **AND** the initial appearance is suitable for daytime / light UI (not a dedicated dark or satellite basemap)

### Requirement: Initial extent at Fort Collins

On first display of the map for a session, the camera SHALL be positioned over Fort Collins, Colorado, without using device GPS or location permissions.

#### Scenario: Cold start map position

- **WHEN** the user opens the Map tab and the map becomes ready
- **THEN** the visible area is centered on Fort Collins, Colorado
- **AND** no location permission prompt is required solely to show the map

### Requirement: Center puck indicator

The Map tab SHALL show a location puck (or equivalent center marker) fixed at the visual center of the map area while the user pans and zooms.

#### Scenario: Puck stays centered during gesture

- **WHEN** the user is panning or zooming
- **THEN** the puck remains at the screen center of the map view
- **AND** the puck does not move with map tiles as if it were a map feature

### Requirement: Selected coordinate on camera idle

The application SHALL determine the geographic coordinate under the center puck only when map camera movement has stopped. While the user is actively panning or zooming, the committed selected coordinate SHALL NOT be updated on each intermediate camera frame.

#### Scenario: Coordinate updates after pan ends

- **WHEN** the user finishes panning and the map camera becomes idle
- **THEN** the selected coordinate reflects the geographic point at the map view center
- **AND** that coordinate differs from the pre-pan selection when the map center moved

#### Scenario: Coordinate stable during gesture

- **WHEN** the user is actively panning or zooming
- **THEN** the last committed selected coordinate is not replaced by per-frame camera updates

#### Scenario: Initial selection at Fort Collins

- **WHEN** the map first becomes idle after the initial Fort Collins camera position is applied
- **THEN** the selected coordinate corresponds to the map view center at that position

### Requirement: No device location in this change

The map experience SHALL NOT depend on device GPS, continuous location updates, or a user-location dot tied to the device position for this change.

#### Scenario: Map without GPS

- **WHEN** the user uses only pan and zoom on the Map tab
- **THEN** the map functions without requesting location access for GPS tracking
