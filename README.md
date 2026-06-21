# Home Assistant Irrigation Add-on

Repository add-on per Home Assistant con controller irrigazione scritto in C#.

## Add-on

- `irrigation-controller`: orchestra valvole gia presenti in Home Assistant come entita `switch.*`.

## Concetti

- Le valvole sono gestite da Home Assistant, ad esempio tramite `MQTT_NET_COMELIT`.
- I cicli manuali usano tempi statici per zona.
- I cicli automatici calcolano i tempi usando deficit idrico, ET stimata e pioggia prevista.
- L'add-on usa le API Home Assistant per chiamare `switch.turn_on`, `switch.turn_off` e `weather.get_forecasts`.
- L'add-on puo pubblicare entita MQTT Discovery proprie del controller tramite `mqtt.publish`.
- La configurazione puo essere modificata dalla UI Ingress con salvataggio e backup automatico.

## Sviluppo locale

```powershell
dotnet build irrigation-controller\src\IrrigationController\IrrigationController.csproj
dotnet run --project irrigation-controller\src\IrrigationController\IrrigationController.csproj
dotnet run --project irrigation-controller\tests\IrrigationController.Tests\IrrigationController.Tests.csproj
```

In Home Assistant, aggiungi questo repository come add-on repository e installa `Irrigation Controller`.

## Installazione Home Assistant

Quando il repository sara pubblicato:

1. Apri Home Assistant.
2. Vai in Settings -> Add-ons -> Add-on Store.
3. Aggiungi questo repository tra i repository add-on.
4. Installa `Irrigation Controller`.
5. Avvia l'add-on e apri la UI Ingress.
6. Modifica `/data/irrigation.json` usando `irrigation-controller/irrigation.example.json` come riferimento.

## Stato repository

La branch principale locale e `main`. Ogni blocco funzionale viene salvato con un commit separato per rendere semplice una futura pubblicazione su GitHub.
