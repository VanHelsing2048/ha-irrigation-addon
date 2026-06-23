# Changelog

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
