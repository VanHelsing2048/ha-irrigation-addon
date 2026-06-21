# Irrigation Controller

Add-on Home Assistant in C# per gestire irrigazione manuale e automatica.

La prima versione usa le valvole gia disponibili in Home Assistant come `switch.*`, quindi non duplica la gestione MQTT di `MQTT_NET_COMELIT`.

L'interfaccia Ingress espone:

- dashboard stato runner, zone e cicli;
- gestione guidata zone;
- gestione guidata cicli;
- configurazione meteo;
- configurazione impianto, sicurezze e MQTT Discovery;
- diagnostica;
- editor JSON avanzato con backup automatico;
- avvio/stop manuale e calibrazione zone.

Endpoint salute:

- `GET /api/health`

Vedi `DOCS.md` per configurazione e API.
