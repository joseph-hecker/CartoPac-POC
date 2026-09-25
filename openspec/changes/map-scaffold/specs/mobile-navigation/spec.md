## Purpose

Provide primary app navigation so users can switch between the map experience and a settings area reserved for future configuration.

## ADDED Requirements

### Requirement: Two-tab shell navigation

The application SHALL present a tab bar with exactly two tabs titled **Map** and **Settings**. Selecting a tab SHALL display that tab's content without requiring back navigation from the other tab.

#### Scenario: User opens the app

- **WHEN** the application launches successfully
- **THEN** the Map tab content is visible
- **AND** the tab bar shows Map and Settings

#### Scenario: User switches to Settings

- **WHEN** the user selects the Settings tab
- **THEN** the Settings tab content is displayed
- **AND** the Map tab content is not shown

#### Scenario: User returns to Map

- **WHEN** the user selects the Map tab from Settings
- **THEN** the Map tab content is displayed again

### Requirement: Settings placeholder

The Settings tab SHALL display a minimal placeholder screen suitable for future settings content. It SHALL NOT require user input to use the Map tab.

#### Scenario: Settings tab content

- **WHEN** the user views the Settings tab
- **THEN** the screen indicates that settings are not yet available or is otherwise intentionally empty
- **AND** no map is shown on the Settings tab
