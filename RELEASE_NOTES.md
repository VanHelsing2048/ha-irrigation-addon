# ha-irrigation-addon 0.3.2

Release forecast e mobile dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Correzione parser `weather.get_forecasts`: ora legge il wrapper `service_response` restituito da Home Assistant.
- La dashboard oggi/domani e il calcolo ET/pioggia usano finalmente i forecast reali quando disponibili.
- Layout mobile migliorato con navigazione compatta in basso, header sticky, tabelle impilate e pulsanti piu comodi.
- Nessuna modifica ai salvataggi, al dry-run, al setup guidato o alla calibrazione.
- Verificato contro `weather.forecast_home`: 48 record hourly e 6 daily ricevuti.
- Test locali: build senza warning e 29 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
