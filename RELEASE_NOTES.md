# ha-irrigation-addon 0.3.4

Release diagnostica meteo dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Nuovo pannello "Verifica forecast Home Assistant" nella pagina Meteo.
- Controllo in tempo reale di forecast hourly e daily.
- Mostra record totali, record disponibili per domani, pioggia prevista e probabilita.
- Nuovo endpoint API `/api/weather/forecast-check`.
- Nessuna modifica ai salvataggi, al dry-run, al setup guidato, alla calibrazione o al parser forecast operativo.
- Test locali: build senza warning e 29 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
