# ha-irrigation-addon 0.1.8

Bugfix release dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Fix aggiunta zone: il pulsante crea una bozza nella pagina Zone e salva solo quando premi `Salva zona`.
- Fix aggiunta cicli: il pulsante crea una bozza nella pagina Cicli e richiede almeno una zona gia salvata.
- Normalizzazione degli ID inseriti nei popup, per evitare caratteri problematici nei controlli HTML.
- Fix eliminazione zone gia referenziate dai cicli: la UI rimuove automaticamente la zona dagli step prima del salvataggio.
- Menu suggerimenti per scegliere entita `switch.*` e `valve.*` gia presenti in Home Assistant quando si configurano le zone.
- Supporto runtime per entita `valve.*`.
- Versione distinta `0.1.3` per forzare Home Assistant Supervisor a proporre un aggiornamento pulito dopo cache/metadati `0.1.2`.
- Versione applicazione visibile nella UI e in `/api/health`.
- Fix apertura UI dietro Home Assistant Ingress.
- Fix chiamate API della UI dietro prefisso Ingress.
- Icona e logo Home Assistant add-on.
- Add-on Home Assistant in C#/.NET 8.
- UI Ingress guidata.
- Config generale dalla scheda Config add-on.
- Gestione zone e cicli.
- Calibrazione zone.
- Cicli automatici con bilancio idrico.
- Meteo via Home Assistant.
- MQTT Discovery.
- Diagnostica e sicurezze runtime.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
