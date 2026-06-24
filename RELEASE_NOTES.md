# ha-irrigation-addon 0.2.4

Release correttiva per la dashboard meteo dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- La Dashboard ora combina forecast configurato e forecast `daily` per popolare meglio oggi/domani.
- Le schede oggi/domani indicano se i dati arrivano davvero da Home Assistant oppure se e stato usato un fallback.
- Domani non resta piu una scheda muta quando il forecast configurato restituisce solo dati orari di oggi.
- Test locali estesi a 28 controlli anti-regressione.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
