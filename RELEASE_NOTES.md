# ha-irrigation-addon 0.3.5

Release simulazione dry-run dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Nuova pagina Simulazione per avviare dry-run visuali.
- Timeline grafica con master valve, zone, pause e step saltati.
- Riepilogo con durata totale, zone previste/saltate, acqua stimata e uso meteo.
- Dettaglio per zona con deficit, ET, pioggia utile, acqua stimata e motivo.
- Nuovo endpoint `/api/simulation/{cycleId}` che non comanda valvole reali.
- Test locali: build senza warning e 29 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
