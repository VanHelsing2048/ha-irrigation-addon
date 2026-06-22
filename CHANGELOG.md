# Changelog

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
