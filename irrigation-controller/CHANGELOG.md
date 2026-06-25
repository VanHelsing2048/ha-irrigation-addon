# Changelog

## [0.2.7] - 2026-06-25

### Aggiunto

- Pagina Setup visibile in modalita base con checklist di prima configurazione.
- Passi guidati per meteo, valvola master opzionale, creazione zona, primo ciclo e dry-run.
- Preset zona per prato, orto, siepe, goccia e vaso.

## [0.2.6] - 2026-06-25

### Aggiunto

- Toggle Avanzate persistente salvato nel browser.
- Pagine Diagnostica e JSON nascoste quando Avanzate e disattivo.
- Impostazioni tecniche Impianto nascoste in modalita base: parallelismo, sicurezze interne e MQTT Discovery.

### Corretto

- Le impostazioni avanzate nascoste vengono preservate quando si salva Impianto in modalita base.

## [0.2.5] - 2026-06-25

### Aggiunto

- Diagnostica meteo con entita, tipo forecast, numero record, primo/ultimo timestamp ed esito della chiamata.
- Dettagli di calcolo per zona nel piano decisionale: deficit, ET zona, pioggia utile, mm da reintegrare e minuti.
- Fascia operativa in Dashboard con stato controller, ciclo/zona attivi, bilancio e motivazione meteo.

### Modificato

- Dashboard resa piu operativa e meno simile a una pagina di form.

## [0.2.4] - 2026-06-25

### Corretto

- Migliorate le schede meteo della Dashboard combinando forecast configurato e forecast `daily` quando disponibile.
- Ogni scheda oggi/domani indica se sta usando previsioni reali di Home Assistant oppure un fallback.
- Aggiunta copertura anti-regressione per lettura forecast daily e marker UI delle previsioni.

## [0.2.3] - 2026-06-24

### Aggiunto

- Editor orari ciclo con righe `input type=time` e controlli aggiungi/rimuovi.
- Icone meteo SVG inline per sole, nuvole, pioggia, temporale, nebbia, neve, irrigazione e stati.
- Test anti-regressione sul calcolo dry-run automatico con ET e sui marker UI degli orari ciclo.

### Corretto

- Il dry-run dei cicli automatici ora proietta il bilancio ET/pioggia del giorno prima di calcolare i minuti zona, senza comandare valvole reali.
- Evitata la doppia applicazione della ET giornaliera quando il bilancio e gia aggiornato oggi.
- Sostituiti i fallback meteo `N/D` sparsi con etichette piu chiare e fallback sul meteo corrente quando il forecast non e disponibile.

## [0.2.2] - 2026-06-24

### Aggiunto

- Errori di salvataggio inline nelle pagine operative al posto dei popup bloccanti.
- Anteprima decisionale oggi/domani dentro ogni scheda ciclo.
- Riepilogo meteo operativo con entita HA, soglie, ultima ET0 e pioggia utile.
- Vista impianto con valvola master, zone collegate e stato delle sicurezze principali.
- Test condivisi sulla programmazione automatica settimanale e ogni N giorni.

### Corretto

- Il piano decisionale usa la stessa logica calendario di scheduler e overview, inclusa la cadenza ogni N giorni da una data iniziale.

## [0.2.1] - 2026-06-24

### Changed

- Rifinita la shell UI Ingress con gerarchia visiva piu forte, navigazione piu leggibile e stati focus/hover.
- Aggiunti stati vuoti guidati per dashboard, zone, cicli e registro ciclo.
- Aggiunti riepiloghi operativi nelle pagine Zone e Cicli.
- Registro ciclo piu leggibile con etichette evento compatte.
- Aggiunti test anti-regressione per shell UI e riepiloghi operativi.

## [0.2.0] - 2026-06-24

### Added

- Registro eventi dedicato per ogni ciclo nella pagina Cicli.
- Selettore entita Home Assistant `weather.*` nella configurazione meteo.
- Dashboard meteo con stato attuale, previsione oggi/domani, ET, pioggia e motivazione decisionale.
- Editor cicli strutturato con righe aggiungibili/rimuovibili, selezione zona e durata `hh:mm:ss`.
- Programmazione automatica alternativa con data iniziale e cadenza ogni N giorni.
- Valvola master opzionale a monte delle zone.
- Dry-run ciclo con log delle azioni pianificate senza comandare valvole reali.
- Test anti-regressione estesi su UI, validazione, master valve e dry-run.

## [0.1.11] - 2026-06-24

### Added

- Aggiunte notifiche esplicite per ogni salvataggio configurazione dalla UI.
- I salvataggi configurazione vengono storicizzati negli eventi diagnostici.
- Aggiunti test anti-regressione per gli handler dei pulsanti di salvataggio e gli header di audit.

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
