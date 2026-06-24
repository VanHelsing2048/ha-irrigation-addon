# ha-irrigation-addon 0.2.2

Release UI e affidabilita calendario dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Errori di salvataggio mostrati inline nelle pagine operative, senza popup bloccanti.
- Ogni ciclo mostra una anteprima decisionale oggi/domani, con decisione e zone coinvolte.
- Pagina Meteo piu leggibile con entita HA, forecast, soglie, ultima ET0 e pioggia utile.
- Pagina Impianto con schema master valve -> zone e stato delle sicurezze principali.
- Logica calendario condivisa tra scheduler, overview e piano decisionale.
- Correzione del piano decisionale per i cicli automatici con data iniziale + ogni N giorni.
- Test locali estesi a 25 controlli anti-regressione.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
