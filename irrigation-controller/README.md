# Irrigation Controller

Versione add-on: `0.2.0`

Add-on Home Assistant in C# per gestire irrigazione manuale e automatica.

La prima versione usa le valvole gia disponibili in Home Assistant come `switch.*` o `valve.*`, quindi non duplica la gestione MQTT di `MQTT_NET_COMELIT`.

L'interfaccia Ingress espone:

- dashboard stato runner, zone e cicli;
- gestione guidata zone;
- suggerimento automatico delle entita `switch.*` e `valve.*` disponibili in Home Assistant;
- gestione guidata cicli;
- editor cicli con step strutturati, durata `hh:mm:ss`, aggiunta e rimozione step;
- piano oggi/domani in Dashboard con meteo attuale, ET, pioggia prevista e decisioni irrigazione;
- configurazione meteo con selezione entita `weather.*` da Home Assistant;
- configurazione impianto, valvola master, sicurezze e MQTT Discovery;
- registro eventi per singolo ciclo e diagnostica generale;
- editor JSON avanzato con backup automatico;
- avvio/stop manuale, dry-run cicli e calibrazione zone.

La scheda Config dell'add-on in Home Assistant gestisce le impostazioni generali. La UI Ingress resta la pagina operativa e guidata per usare il controller.

Endpoint salute:

- `GET /api/health`

Vedi `DOCS.md` per configurazione e API.

## Release

La versione dell'add-on e definita in `config.yaml`. Le release del repository generano anche uno zip scaricabile del repository Home Assistant add-on.
