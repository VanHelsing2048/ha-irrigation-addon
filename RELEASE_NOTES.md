# ha-irrigation-addon 0.3.6

Release spiegazione formule dry-run dell'add-on Home Assistant `Irrigation Controller`.

## Installazione

In Home Assistant aggiungi il repository:

```text
https://github.com/VanHelsing2048/ha-irrigation-addon
```

Poi installa l'add-on `Irrigation Controller`.

## Contenuto

- Nella pagina Simulazione ogni zona mostra una formula testuale del tempo calcolato.
- I cicli automatici spiegano deficit attuale, ET zona, pioggia utile, target, portata e limiti min/max.
- I cicli manuali spiegano durata impostata nello step e limite massimo di sicurezza.
- La formula segue la logica reale anche quando il bilancio acqua odierno e gia stato aggiornato.
- Test locali: build senza warning e 29 controlli anti-regressione superati.

## Pacchetto

Lo zip allegato contiene il repository add-on completo. Home Assistant normalmente usa l'URL del repository; lo zip e fornito come artefatto scaricabile/versionato.
