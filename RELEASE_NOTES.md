# ha-irrigation-addon 0.3.3

Release pulizia dashboard dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Rimossi i doppioni tra riepilogo meteo e piano oggi/domani.
- La dashboard ora ha una sola area previsionale: il piano con oggi/domani, decisione, meteo e cicli.
- Meteo attuale e motivo decisionale restano in alto come card compatta operativa.
- Nessuna modifica ai salvataggi, al dry-run, al setup guidato, alla calibrazione o al parser forecast.
- Test locali: build senza warning e 29 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
