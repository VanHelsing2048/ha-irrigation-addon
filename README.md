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

## Sviluppo locale

```powershell
dotnet build irrigation-controller\src\IrrigationController\IrrigationController.csproj
dotnet run --project irrigation-controller\src\IrrigationController\IrrigationController.csproj
```

In Home Assistant, aggiungi questo repository come add-on repository e installa `Irrigation Controller`.
