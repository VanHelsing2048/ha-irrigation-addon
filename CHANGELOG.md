# Changelog

## [0.2.8] - 2026-06-25

### Added

- Add guided zone calibration panel with test start, measurement entry, calculated result and apply action.
- Add Setup step for zone calibration.
- Add calibrated / needs calibration badges on zone cards and setup checklist.

## [0.2.7] - 2026-06-25

### Added

- Add a non-advanced Setup page with first-configuration checklist.
- Add guided setup steps for weather entity, optional master valve, zone creation, first cycle creation and dry-run.
- Add zone presets for prato, orto, siepe, goccia and vaso.

## [0.2.6] - 2026-06-25

### Added

- Add a persistent Advanced mode toggle saved in browser local storage.
- Hide Diagnostics and JSON pages while Advanced mode is disabled.
- Hide advanced plant settings such as parallelism, safety internals and MQTT Discovery in basic mode.

### Fixed

- Preserve hidden advanced plant settings when saving the plant page in basic mode.

## [0.2.5] - 2026-06-25

### Added

- Add weather data diagnostics with entity, forecast type, record count, first/last forecast timestamps and fetch status.
- Add per-zone irrigation calculation details in the decision plan: deficit, crop ET, effective rain, irrigation deficit and minutes.
- Add a polished dashboard operating header with controller status, active cycle/zone, water balance and weather decision reason.

### Changed

- Make dashboard cycle and weather sections more operational and less form-like.

## [0.2.4] - 2026-06-25

### Fixed

- Improve dashboard weather forecast cards by combining configured forecast data with `daily` forecasts when available.
- Show whether each today/tomorrow card is using real Home Assistant forecasts or a fallback.
- Add regression coverage for daily forecast reads and forecast UI markers.

## [0.2.3] - 2026-06-24

### Added

- Add a structured cycle start-time editor using `input type=time` rows with add/remove controls.
- Add inline SVG weather icons for sun, cloud, rain, storm, fog, snow, irrigation and status badges.
- Add regression tests for automatic dry-run ET projection and cycle time editor markers.

### Fixed

- Fix automatic cycle dry-run durations: simulations now project today's ET/rain balance before calculating zone minutes, without mutating real valve state.
- Avoid adding daily ET twice when the water balance has already been updated today.
- Replace scattered `N/D` weather fallbacks with clearer labels and current-weather fallback when forecasts are unavailable.

## [0.2.2] - 2026-06-24

### Added

- Add inline validation feedback in the operative UI pages, replacing blocking validation popups.
- Add today/tomorrow decision preview inside each cycle card.
- Add a richer weather settings overview with HA entity, thresholds, latest ET0 and useful rain.
- Add a plant overview showing master valve, zone branches and main safety settings.
- Add shared schedule calculation tests for weekly and every-N-days automatic schedules.

### Fixed

- Make the decision plan use the same schedule calculation as the scheduler and overview, including start date plus every-N-days schedules.

## [0.2.1] - 2026-06-24

### Changed

- Polish the Ingress UI shell with stronger visual hierarchy, improved sidebar navigation and clearer focus/hover states.
- Add guided empty states for dashboard, zones, cycles and per-cycle event registers.
- Add operation summaries to Zones and Cycles pages.
- Render per-cycle event types with compact user-facing labels.
- Add regression tests for the polished UI shell and operation summaries.

## [0.2.0] - 2026-06-24

### Added

- Add per-cycle event register in the Cycles page.
- Add Home Assistant `weather.*` entity picker for weather configuration.
- Add dashboard weather summary with current weather state, today/tomorrow forecast, ET, rain and decision reason.
- Add structured cycle step editor with add/remove rows, zone selector and `hh:mm:ss` duration input.
- Add automatic schedule interval mode using start date plus every N days.
- Add optional master valve support for upstream irrigation valves.
- Add cycle dry-run simulation that logs planned actions without switching real valves.
- Add regression tests for cycle register, weather picker, dashboard weather summary, cycle step editor, master valve and dry-run UI.

## [0.1.11] - 2026-06-24

### Added

- Add explicit UI notifications for every configuration save action.
- Record configuration saves in the diagnostics event log.
- Add regression tests for escaped UI save handlers and save audit headers.

## [0.1.10] - 2026-06-24

### Fixed

- Fix save buttons for draft zones and cycles by generating escaped JavaScript handlers.
- Add explicit save notifications for zones and cycles.
- Show saved status badges after zone and cycle drafts are persisted.

## [0.1.9] - 2026-06-24

### Fixed

- Read the Ingress UI version from the .NET assembly instead of a hardcoded sidebar string.

## [0.1.8] - 2026-06-24

### Fixed

- Fix zone creation from the web UI by creating an editable draft instead of immediately saving a new zone.
- Fix cycle creation from the web UI by creating an editable draft and blocking cycle creation until at least one zone is saved.
- Normalize IDs entered in creation prompts to avoid problematic characters in generated HTML controls.

## [0.1.7] - 2026-06-23

### Changed

- Move the decision plan into the Dashboard instead of a separate page.
- Replace fragile unicode weather icons with stable text badges and hover tooltips.
- Add an add-on local changelog file for Home Assistant update notes.

## [0.1.6] - 2026-06-23

### Added

- Add decision plan API for today and tomorrow.
- Add icon-first `Piano` UI page with weather icon, irrigation decision, compact weather metrics, scheduled automatic cycles and today's events.

## [0.1.5] - 2026-06-23

### Fixed

- Remove deleted zones from cycle steps before saving the configuration.
- Drop empty cycle steps created by deleting the last zone in a step.

## [0.1.4] - 2026-06-23

### Added

- Add Home Assistant entity discovery endpoint for irrigation zone entities.
- Suggest available `switch.*` and `valve.*` entities in the zone entity field.
- Support `valve.*` entities with `valve.open_valve` and `valve.close_valve`.

## [0.1.3] - 2026-06-23

### Changed

- Bump add-on version to `0.1.3` to force Home Assistant Supervisor to see a distinct update after cached `0.1.2` metadata.
- Add application version to `/api/health`.
- Show the application version in the Ingress UI sidebar.

## [0.1.2] - 2026-06-23

### Fixed

- Fix Ingress entry path to avoid URLs with a double slash before `ui`.
- Resolve UI API calls relative to the current Ingress base path instead of calling Home Assistant root `/api`.
- Use relative redirects for `/` and `/config` so the browser stays inside the Ingress prefix.

## [0.1.1] - 2026-06-22

### Added

- Add Home Assistant add-on `icon.png` at 128x128.
- Add Home Assistant add-on `logo.png` around 250x100.

### Changed

- Bump add-on version to `0.1.1`.

## [0.1.0] - 2026-06-22

Prima release pubblicabile dell'add-on.

### Added

- Add-on Home Assistant `Irrigation Controller` in C#/.NET 8.
- Configurazione generale dalla scheda Config dell'add-on tramite `/data/options.json`.
- UI Ingress guidata per dashboard, zone, cicli, meteo, impianto, diagnostica e JSON avanzato.
- Supporto valvole gia esposte in Home Assistant come entita `switch.*`.
- Cicli manuali con tempi statici per zona.
- Cicli automatici basati su bilancio idrico giornaliero, ET stimata e pioggia prevista.
- Integrazione meteo via `weather.get_forecasts`.
- Calibrazione guidata zone con calcolo `precipitation_rate_mm_h`.
- Applicazione calibrazione alla configurazione con backup automatico.
- MQTT Discovery per entita di stato del controller.
- Policy idraulica per zone sequenziali/parallele e pausa tra zone.
- Sicurezze runtime: retry comandi, verifica stato valvole, spegnimento zone all'avvio e su errore.
- Diagnostica runtime con ultimo meteo, ultima decisione e ultimo errore.
- Test runner locale senza dipendenze esterne.
- Workflow release per produrre pacchetto zip scaricabile.

### Notes

- Home Assistant usa normalmente l'URL del repository come add-on repository.
- Lo zip di release e un artefatto versionato comodo per backup o distribuzione manuale.
