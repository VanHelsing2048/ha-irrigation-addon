# Irrigation Controller

Add-on Home Assistant in C# per gestire irrigazione manuale e automatica.

La prima versione usa le valvole gia disponibili in Home Assistant come `switch.*`, quindi non duplica la gestione MQTT di `MQTT_NET_COMELIT`.

L'interfaccia Ingress espone:

- stato runner;
- avvio manuale dei cicli;
- avvio/stop rapido delle singole zone;
- calibrazione guidata resa zona;
- stop globale.

Endpoint salute:

- `GET /api/health`

Vedi `DOCS.md` per configurazione e API.
