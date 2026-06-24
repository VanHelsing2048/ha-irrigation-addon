# Changelog

## [0.1.10] - 2026-06-24

### Fixed

- Corretto il salvataggio di zone e cicli creati come bozza: i pulsanti ora eseguono correttamente l'azione JavaScript.
- Aggiunte notifiche esplicite `Zona salvata` e `Ciclo salvato`.
- Le schede mostrano `bozza` prima del salvataggio e `salvata` / `salvato` dopo il salvataggio riuscito.

## [0.1.9] - 2026-06-24

### Fixed

- La versione mostrata nella UI Ingress ora viene letta dall'assembly .NET invece di essere hardcoded nella sidebar.

## [0.1.8] - 2026-06-24

### Fixed

- Corretta l'aggiunta di zone dalla UI: il pulsante crea una bozza modificabile invece di salvare subito.
- Corretta l'aggiunta di cicli dalla UI: il pulsante crea una bozza modificabile e richiede almeno una zona gia salvata.
- Normalizzati gli ID inseriti nei popup per evitare caratteri problematici nei controlli HTML.

## [0.1.7] - 2026-06-23

### Changed

- Piano decisionale spostato direttamente in Dashboard.
- Badge meteo testuali con tooltip al posto di icone unicode fragili.
- Aggiunto changelog locale dell'add-on per Home Assistant.

## [0.1.6] - 2026-06-23

### Added

- Endpoint decisionale oggi/domani.
- Vista decisionale con meteo, cicli automatici e stima zone.

## [0.1.5] - 2026-06-23

### Fixed

- Eliminazione zone con pulizia automatica dei riferimenti nei cicli.
- Rinomina ID zona con aggiornamento dei riferimenti nei cicli.

## [0.1.4] - 2026-06-23

### Added

- Suggerimenti entita Home Assistant `switch.*` e `valve.*` nella configurazione zone.
- Supporto runtime per entita `valve.*`.

## [0.1.3] - 2026-06-23

### Changed

- Versione runtime visibile in UI e in `/api/health`.

## [0.1.2] - 2026-06-23

### Fixed

- Fix percorsi Ingress e chiamate API relative al prefisso Ingress.

## [0.1.1] - 2026-06-22

### Added

- Icona e logo add-on.

## [0.1.0] - 2026-06-22

### Added

- Prima release pubblicabile.
