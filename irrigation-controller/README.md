# Irrigation Controller

Versione add-on: `0.3.3`

Add-on Home Assistant in C# per gestire irrigazione manuale e automatica.

La prima versione usa le valvole gia disponibili in Home Assistant come `switch.*` o `valve.*`, quindi non duplica la gestione MQTT di `MQTT_NET_COMELIT`.

L'interfaccia Ingress espone:

- dashboard stato runner, zone e cicli;
- gestione guidata zone;
- suggerimento automatico delle entita `switch.*` e `valve.*` disponibili in Home Assistant;
- gestione guidata cicli;
- editor cicli con step strutturati, durata `hh:mm:ss`, aggiunta e rimozione step;
- editor orari ciclo con input `time` e righe aggiungibili/rimuovibili;
- piano oggi/domani in Dashboard con meteo attuale, ET, pioggia prevista e decisioni irrigazione;
- dashboard senza doppio blocco previsionale: stato meteo compatto sopra e piano oggi/domani come unica vista forecast;
- errori di salvataggio mostrati inline nelle pagine operative;
- configurazione meteo con selezione entita `weather.*` da Home Assistant;
- riepilogo meteo operativo con soglie, ultima ET0 e pioggia utile;
- icone meteo SVG e fallback testuali piu chiari quando il forecast non e disponibile;
- forecast dashboard oggi/domani basato su dati `daily` piu il tipo configurato, con indicazione se i dati arrivano davvero da Home Assistant;
- lettura forecast compatibile con il wrapper `service_response` restituito da Home Assistant;
- diagnostica meteo con record ricevuti, tipo forecast, primo/ultimo timestamp ed esito della chiamata;
- dettagli di calcolo per zona: deficit, ET, pioggia utile, mm da reintegrare e minuti;
- dashboard operativa con stato controller, ciclo/zona attivi e motivazione meteo in evidenza;
- shell UI rivista con sidebar persistente, navigazione piu ordinata e toggle Avanzate nella barra laterale;
- toggle Avanzate persistente per nascondere pagine diagnostiche, JSON e impostazioni tecniche;
- pagina Setup con checklist guidata, meteo, master valve, creazione zona con preset, primo ciclo e dry-run;
- calibrazione guidata per zona con test, misure, resa mm/h, uniformita e applicazione alla configurazione;
- configurazione impianto, schema master valve -> zone, sicurezze e MQTT Discovery;
- anteprima decisionale oggi/domani dentro ogni ciclo;
- registro eventi per singolo ciclo richiudibile e diagnostica generale;
- layout mobile con navigazione compatta in basso e tabelle impilate;
- editor JSON avanzato con backup automatico;
- avvio/stop manuale, dry-run cicli e calibrazione zone.
- shell UI rifinita con riepiloghi operativi, navigazione piu leggibile e stati vuoti guidati.

La scheda Config dell'add-on in Home Assistant gestisce le impostazioni generali. La UI Ingress resta la pagina operativa e guidata per usare il controller.

Endpoint salute:

- `GET /api/health`

Vedi `DOCS.md` per configurazione e API.

## Release

La versione dell'add-on e definita in `config.yaml`. Le release del repository generano anche uno zip scaricabile del repository Home Assistant add-on.
