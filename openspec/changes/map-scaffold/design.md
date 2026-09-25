## Context

CartoPac Mobile is a stock .NET 10 MAUI app with a single Shell route to template `MainPage`. See `proposal.md` for motivation. Target platforms today include Mac Catalyst; this change narrows to Android, iOS, and Windows and introduces MaplibreNativeMAUI for rendering.

## Goals / Non-Goals

**Goals:**

- Shell `TabBar` with `MapPage` and `SettingsPage`.
- Integrate `MapLibreMap` with handler registration in `MauiProgram`.
- Center puck as a MAUI overlay (`Grid`: map full bleed + centered `Image` or shape, `InputTransparent` on puck).
- Wire `CameraMoveStartedCommand` / `CameraIdleCommand` (or equivalent) to gate updates to `SelectedCoordinate`.
- Initial camera: Fort Collins (~40.5853, -105.0844, zoom ~12).
- Style URL: `https://demotiles.maplibre.org/style.json`.
- Disable built-in GPS UI (`ShowGpsControl="False"`); optional: hide nav/attribution overlays if they clutter the POC.
- Remove `net10.0-maccatalyst` from `CartoPac Mobile.csproj`.

**Non-Goals:**

- GPS, geolocation APIs, platform location permissions.
- Settings functionality beyond placeholder UI.
- Offline regions, custom styles, dark theme toggle.
- Persisting selected coordinate across sessions.
- Automated UI tests on all three platforms in this change.

## Decisions

### 1. Navigation: Shell TabBar

**Choice:** `AppShell` with `<TabBar>` and two `ShellContent` items (`MapPage`, `SettingsPage`).

**Alternatives:** `TabbedPage` (works but diverges from existing Shell setup); single page with custom tab buttons.

**Rationale:** Matches current `AppShell` entry point and existing Shell tab styles in `Resources/Styles/Styles.xaml`.

### 2. Map library: MaplibreNativeMAUI

**Choice:** Add NuGet package(s) published by the MaplibreNativeMAUI project (`MapLibreNative.Maui.Handlers` and dependencies as documented in the repo README), register `MapLibreMapHandler` in `ConfigureMauiHandlers`.

**Alternatives:** `Microsoft.Maui.Controls.Maps` (different stack, not MapLibre); project reference to fork (heavier, native build).

**Rationale:** User-selected library; supports Android, iOS, Windows on .NET 10; demo style URL documented.

**Note:** If NuGet packages are unavailable or fail to restore, fall back to documented project reference / local build per upstream README (capture in tasks).

### 3. Center puck vs `MyLocationEnabled`

**Choice:** Screen-fixed overlay puck; **do not** enable `MyLocationEnabled`.

**Alternatives:** Library user-location layer; map-anchored `Pin` at center updated every frame.

**Rationale:** Spec requires center-fixed indicator and no GPS; overlay avoids fighting tracking modes.

### 4. Selected coordinate source

**Choice:** On `CameraIdleCommand`, set selection from `MapLibreMap.VisibleRegion.Center` when available, else `controller.ScreenPointToLatLng` at view center from `MapReady`/`StyleLoaded` controller.

**Alternatives:** Update on every `CameraMoveCommand` (rejected: jitter and spec violation).

**Rationale:** Aligns with library docs (`VisibleRegion` refreshed on camera idle).

### 5. State placement

**Choice:** Lightweight `MapPage` view model or page bindable properties: `SelectedLatitude`, `SelectedLongitude`, optional read-only label for debugging POC.

**Alternatives:** Full MVVM + DI for two tabs (premature for scaffold).

**Rationale:** Minimal scope; refactor when Settings consumes selection.

### 6. Platform matrix

**Choice:** Keep `net10.0-android`, `net10.0-ios`, `net10.0-windows10.0.19041.0`; remove Mac Catalyst TFM.

**Rationale:** Explicit user scope; reduces build surface.

### 7. Template MainPage

**Choice:** Remove from Shell; keep files or delete template page in same change (prefer delete or leave unused—tasks should pick one: stop routing to `MainPage`).

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| MaplibreNativeMAUI package maturity / restore issues | Verify `dotnet restore` early; pin package version; use sample repo version |
| Windows map via `glReadPixels` may be slower than mobile | Accept for POC; profile later if needed |
| Demo tiles require network | Document; offline out of scope |
| Center lat/lng vs visual center with overlays | Use map control bounds center; keep puck truly centered in same `Grid` |
| Shell tab + full-screen map lifecycle | Test tab switch does not leak handlers; map re-shows correctly |

## Migration Plan

1. Implement behind existing app id; no server migration.
2. Deploy to dev devices per platform; no rollback beyond reverting git change.
3. Removing Mac Catalyst TFM stops Catalyst builds only; no store impact for this POC.

## Open Questions

- Exact NuGet package IDs and version to pin (resolve from upstream README/releases at apply time).
- Whether to remove `Platforms/MacCatalyst` folder or only TFM (cosmetic vs cleanup).
