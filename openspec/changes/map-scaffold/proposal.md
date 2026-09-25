## Why

CartoPac Mobile needs a cross-platform foundation (Android, iOS, Windows) that proves interactive mapping and simple app navigation before field workflows, GPS, or settings are built. A two-tab shell with a MapLibre-based map and a fixed center selection puck establishes the core UX pattern for “pick a location on the map” without device location.

## What Changes

- Replace single-route Shell with a **bottom tab bar**: **Map** and **Settings** (Settings is an empty placeholder).
- Add **Map** tab using [MaplibreNativeMAUI](https://github.com/TechIdiots-LLC/MaplibreNativeMAUI): pan and zoom on a light street demo style.
- Show a **center puck** (screen-fixed overlay); update the selected map coordinate only when the camera is **idle** (not while panning or zooming).
- **Initial camera**: Fort Collins, Colorado (no GPS, no location permissions).
- **Remove Mac Catalyst** from target frameworks and related platform targeting.
- Retire or replace the template **MainPage** as the primary entry in favor of **MapPage** and **SettingsPage**.

## Capabilities

### New Capabilities

- `mobile-navigation`: Shell tab navigation with Map and Settings routes.
- `map-view`: MapLibre map display, gestures, default extent, and center-puck selection semantics.

### Modified Capabilities

- (none)

## Impact

- **CartoPac Mobile** project: `AppShell`, new pages, `MauiProgram` handler registration, `.csproj` target frameworks.
- **New dependency**: MapLibre Native MAUI packages (NuGet or equivalent reference per design).
- **Platforms**: Android, iOS, Windows only; Mac Catalyst removed.
- **Out of scope**: GPS, offline maps, settings content, dark theme, production basemap URLs.
