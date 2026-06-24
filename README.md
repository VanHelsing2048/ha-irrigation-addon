# Home Assistant Irrigation Add-on

Versione corrente: `0.2.1`

Repository add-on per Home Assistant con controller irrigazione scritto in C#.

## Add-on

- `irrigation-controller`: orchestra valvole gia presenti in Home Assistant come entita `switch.*` o `valve.*`.

## Concetti

- Le valvole sono gestite da Home Assistant, ad esempio tramite `MQTT_NET_COMELIT`.
- La UI propone le entita `switch.*` e `valve.*` gia presenti in Home Assistant quando configuri le zone.
- I cicli manuali usano tempi statici per zona.
- I cicli automatici calcolano i tempi usando deficit idrico, ET stimata e pioggia prevista.
- L'add-on usa le API Home Assistant per chiamare `switch.turn_on`, `switch.turn_off` e `weather.get_forecasts`.
- L'add-on puo pubblicare entita MQTT Discovery proprie del controller tramite `mqtt.publish`.
- La configurazione puo essere modificata dalla UI Ingress con salvataggio e backup automatico.
- La policy idraulica controlla zone sequenziali, parallelismo e pausa tra zone.
- La UI Ingress include viste guidate per dashboard, zone, cicli, meteo, impianto, diagnostica e JSON avanzato.
- La Dashboard mostra oggi/domani con badge meteo, tooltip, decisione irrigazione e cicli automatici previsti.
- La Dashboard mostra meteo attuale, previsioni oggi/domani, ET, pioggia prevista e motivo della decisione.
- Ogni ciclo ha un registro eventi dedicato, oltre alla diagnostica generale.
- I cicli si configurano con step strutturati: zona, durata `hh:mm:ss`, aggiunta e rimozione righe.
- La programmazione automatica supporta giorni settimana oppure cadenza ogni N giorni da una data iniziale.
- L'impianto puo usare una valvola master a monte delle zone.
- I cicli possono essere simulati in dry-run senza comandare valvole reali.
- La UI usa una shell piu curata con navigazione codificata, riepiloghi operativi, stati vuoti e tabelle piu leggibili.
- Le impostazioni generali dell'add-on sono disponibili nella scheda Config di Home Assistant e vengono lette da `/data/options.json`.

## Sviluppo locale

```powershell
dotnet build irrigation-controller\src\IrrigationController\IrrigationController.csproj
dotnet run --project irrigation-controller\src\IrrigationController\IrrigationController.csproj
dotnet run --project irrigation-controller\tests\IrrigationController.Tests\IrrigationController.Tests.csproj
```

In Home Assistant, aggiungi questo repository come add-on repository e installa `Irrigation Controller`.

## Installazione Home Assistant

Metodo consigliato:

1. Apri Home Assistant.
2. Vai in Settings -> Add-ons -> Add-on Store.
3. Aggiungi questo repository tra i repository add-on:
   ```text
   https://github.com/VanHelsing2048/ha-irrigation-addon
   ```
4. Installa `Irrigation Controller`.
5. Avvia l'add-on e apri la UI Ingress.
6. Configura le impostazioni generali dalla scheda Config dell'add-on.
7. Usa la UI Ingress per zone, cicli, calibrazione e diagnostica.

## Pacchetto scaricabile

Ogni release GitHub pubblica anche uno zip del repository add-on:

```text
ha-irrigation-addon-<version>.zip
```

Home Assistant normalmente usa direttamente l'URL del repository GitHub. Lo zip serve come artefatto scaricabile/versionato per backup, audit o distribuzioni manuali.

## Versioning

- `irrigation-controller/config.yaml` contiene la versione add-on.
- `CHANGELOG.md` contiene le note di rilascio.
- I tag seguono il formato `vX.Y.Z`, per esempio `v0.1.0`.

## Stato repository

Flusso previsto:

1. sviluppo su branch dedicati;
2. merge su `main`;
3. branch `release/vX.Y.Z` per stabilizzazione;
4. tag `vX.Y.Z`;
5. GitHub Release con pacchetto zip allegato.
