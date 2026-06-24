# ha-irrigation-addon 0.2.3

Release correttiva per dry-run automatico, orari ciclo e meteo dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Corretto il dry-run dei cicli automatici: ora calcola i minuti zona proiettando ET/pioggia del giorno.
- Il dry-run non modifica lo stato reale e non applica due volte la ET se il bilancio e gia aggiornato.
- Gli orari di partenza dei cicli usano input orario guidato con aggiungi/rimuovi, non piu un campo testuale con virgole.
- Dashboard e piano meteo usano icone SVG per sole, nuvole, pioggia, temporale e altri stati.
- Migliorati i fallback meteo: meno `N/D`, piu etichette leggibili quando Home Assistant non fornisce forecast.
- Test locali estesi a 27 controlli anti-regressione.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
