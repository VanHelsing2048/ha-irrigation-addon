# ha-irrigation-addon 0.3.1

Release rifinitura UI dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Registro eventi dentro ogni ciclo richiudibile di default, cosi non appesantisce la configurazione.
- Metriche ET, pioggia, probabilita, deficit e acqua richiesta rese piu compatte con icone e tooltip.
- Icone con colori distinti per sole, nuvole, pioggia, temporale, ET, OK, skip e informazioni.
- Meno testo ripetitivo nei pannelli previsione e piano.
- Nessuna modifica al contratto API, ai salvataggi, al dry-run, al setup guidato o alla calibrazione.
- Test locali: build senza warning e 28 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
