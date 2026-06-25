# ha-irrigation-addon 0.3.0

Release restyling UI dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Shell Ingress rivista con sidebar persistente, brand piu chiaro e toggle Avanzate spostato nella navigazione.
- Dashboard piu ordinata: stato controller, meteo attuale, motivazione decisionale, previsioni e piano hanno una gerarchia piu leggibile.
- Icone meteo/decisione piu coerenti, con fallback comprensibili quando Home Assistant non fornisce forecast completi.
- Zone e cicli hanno intestazioni strutturate, badge di stato e form meno rumorosi.
- Nessuna modifica al contratto API, ai salvataggi, al dry-run, al setup guidato o alla calibrazione.
- Test locali: build senza warning e 28 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
