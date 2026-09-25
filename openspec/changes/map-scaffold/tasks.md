## 1. Project and dependencies

- [x] 1.1 Remove `net10.0-maccatalyst` from `CartoPac Mobile.csproj` and verify `dotnet build "CartoPac Mobile/CartoPac Mobile.csproj" -f net10.0-windows10.0.19041.0` succeeds without Mac Catalyst targets
- [x] 1.2 Add MaplibreNativeMAUI package reference(s) per upstream README and verify `dotnet restore` on the mobile project completes without errors
- [x] 1.3 Register `MapLibreMapHandler` in `MauiProgram.cs` and verify the project builds for at least one target framework after registration

## 2. Navigation shell

- [x] 2.1 Add `SettingsPage` (placeholder label or empty state) and verify it renders when navigated to directly
- [x] 2.2 Update `AppShell.xaml` to use a `TabBar` with Map and Settings `ShellContent` routes to `MapPage` and `SettingsPage`, and verify launching the app shows the Map tab by default with both tabs visible
- [x] 2.3 Remove `MainPage` from Shell routing (delete or stop referencing template `MainPage`) and verify the app no longer depends on the template home page

## 3. Map page — display and camera

- [x] 3.1 Create `MapPage` with full-screen `MapLibreMap` using `StyleUrl` `https://demotiles.maplibre.org/style.json`, `ShowGpsControl="False"`, and verify the map loads street tiles when the Map tab is shown (network available)
- [x] 3.2 On map ready/style loaded, call `JumpTo` (or equivalent) to Fort Collins (~40.5853, -105.0844, zoom ~12) and verify the initial view is centered on Fort Collins without location permission prompts
- [x] 3.3 Verify pan and zoom gestures work on the Map tab on at least one platform (Windows or Android emulator/device)

## 4. Map page — center puck and selection

- [x] 4.1 Overlay a centered puck (`Grid` + `InputTransparent` center image/shape) on `MapPage` and verify the puck stays at screen center while panning and zooming
- [x] 4.2 Handle `CameraMoveStartedCommand` to set a gesturing flag and `CameraIdleCommand` to clear it and update `SelectedLatitude`/`SelectedLongitude` from `VisibleRegion.Center` (or screen-center lat/lng via controller), and verify coordinates change only after pan ends, not during continuous drag
- [x] 4.3 Optionally bind a small coordinate readout on `MapPage` for POC debugging and verify it matches the map center after camera idle

## 5. Settings tab and smoke validation

- [x] 5.1 Wire Settings tab to `SettingsPage` placeholder only (no map) and verify switching Map → Settings → Map preserves tab behavior per `mobile-navigation` spec
- [x] 5.2 Build the solution for Android, iOS, and Windows TFMs where the host OS allows, and verify zero build errors related to map or shell changes
