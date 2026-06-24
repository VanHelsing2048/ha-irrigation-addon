# ha-irrigation-addon 0.2.0

Feature release dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Dashboard meteo riorganizzata con stato attuale, oggi/domani, ET, pioggia prevista, probabilita e motivazione decisionale.
- Registro eventi dedicato per ogni ciclo, oltre alla diagnostica generale.
- Selettore entita meteo `weather.*` letto da Home Assistant.
- Editor cicli strutturato con step aggiungibili/rimuovibili, selezione zona e durata `hh:mm:ss`.
- Programmazione automatica con scelta tra giorni settimana oppure ogni N giorni da data iniziale.
- Valvola master opzionale a monte di tutte le zone.
- Dry-run ciclo con registro di cosa la logica farebbe senza comandare valvole reali.
- Test anti-regressione estesi per UI, validazione schedule, master valve e dry-run.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
