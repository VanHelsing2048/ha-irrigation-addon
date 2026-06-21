# Irrigation Controller

Questo add-on orchestra valvole gia esposte in Home Assistant come entita `switch.*`.

## Configurazione

Alla prima esecuzione viene creato `/data/irrigation.json` con un esempio. Le valvole devono essere sostituite con le entita reali create da `MQTT_NET_COMELIT`.

Il file `irrigation.example.json` contiene un esempio completo da usare come riferimento.

La UI Ingress espone viste guidate per modificare zone, cicli, meteo, impianto, sicurezze e MQTT Discovery. La vista JSON rimane disponibile per modifiche avanzate.

```json
{
  "weather": {
    "entity": "weather.home",
    "forecast_type": "hourly",
    "rain_lookahead_hours": 24,
    "rain_efficiency": 0.75,
    "skip_if_expected_rain_mm_above": 4,
    "skip_if_rain_probability_above": 70
  },
  "mqtt_discovery": {
    "enabled": true,
    "discovery_prefix": "homeassistant",
    "base_topic": "irrigation_controller",
    "publish_interval_seconds": 30
  },
  "safety": {
    "turn_off_all_zones_on_startup": true,
    "stop_all_known_zones_on_error": true,
    "verify_zone_state_after_switch": true,
    "switch_retry_count": 2,
    "switch_retry_delay_ms": 750,
    "manual_runs_ignore_weather": true,
    "max_zone_minutes": 60
  },
  "hydraulic": {
    "allow_parallel_zones": false,
    "max_parallel_zones": 1,
    "pause_between_zones_seconds": 0
  },
  "zones": {
    "prato": {
      "name": "Prato",
      "entity": "switch.valvola_prato",
      "precipitation_rate_mm_h": 12,
      "crop_coefficient": 0.8,
      "min_minutes": 4,
      "max_minutes": 25
    }
  },
  "cycles": {
    "manuale_giardino": {
      "name": "Manuale giardino",
      "mode": "Manual",
      "steps": [
        { "zones": ["prato"], "duration_minutes": 15 }
      ]
    },
    "automatico_mattina": {
      "name": "Automatico mattina",
      "enabled": true,
      "mode": "Automatic",
      "schedule": {
        "days": ["Monday", "Wednesday", "Friday"],
        "times": ["06:30"]
      },
      "steps": [
        { "zones": ["prato"] }
      ]
    }
  }
}
```

## API

- `GET /api/health`
- `GET /api/status`
- `GET /api/overview`
- `GET /api/diagnostics`
- `GET /api/config`
- `PUT /api/config`
- `GET /api/config/validate`
- `POST /api/config/reload`
- `POST /api/cycles/{cycleId}/start`
- `POST /api/cycles/{cycleId}/stop`
- `POST /api/zones/{zoneId}/start?minutes=5`
- `POST /api/zones/{zoneId}/stop`
- `POST /api/stop`
- `POST /api/calibration/zones/{zoneId}/start?minutes=10`
- `POST /api/calibration/zones/{zoneId}/complete`
- `POST /api/calibration/zones/{zoneId}/apply`

## Calibrazione zone

Per misurare `precipitation_rate_mm_h`:

1. Disponi alcuni contenitori nella zona.
2. Avvia il test:

```http
POST /api/calibration/zones/prato/start?minutes=10
```

3. Misura i mm raccolti e completa:

```json
{
  "minutes": 10,
  "measurements_mm": [1.8, 2.1, 1.6, 2.0]
}
```

Il controller restituisce media, resa `mm/h`, minimo, massimo e uniformita. Usa `precipitation_rate_mm_h` proposto nella configurazione della zona.

Per applicare l'ultima calibrazione alla configurazione della zona:

```http
POST /api/calibration/zones/prato/apply
```

Prima di salvare, il controller crea un backup timestamped di `/data/irrigation.json`.

## Modalita manuale

I cicli manuali usano i minuti configurati negli step. Di default ignorano il meteo.

## Modalita automatica

Il bilancio idrico viene aggiornato una volta al giorno usando il meteo Home Assistant e `weather.get_forecasts`.

Per ogni zona:

```text
deficit = deficit_precedente + ET0 * crop_coefficient - pioggia_effettiva
```

Quando parte un ciclo automatico, la durata viene calcolata dal deficit gia persistito:

```text
durata = (deficit - target_deficit_mm) / precipitation_rate_mm_h * 60
```

Ogni irrigazione riduce il deficit in base ai mm stimati applicati dalla zona.

Per una stima migliore puoi configurare `external_et0_sensor_entity` e far leggere all'add-on un sensore ET0 dedicato.

## Storico

Lo stato persistente in `/data/state.json` contiene:

- deficit idrico per zona;
- data ultimo aggiornamento del bilancio;
- ultime esecuzioni schedulate;
- ultimi eventi di bilancio e irrigazione.
- diagnostica di ultimo meteo letto, ultima decisione e ultimo errore.

## Sicurezze

Il controller puo:

- spegnere tutte le zone note all'avvio;
- riprovare i comandi alle valvole;
- verificare lo stato reale dopo `turn_on` e `turn_off`;
- spegnere tutte le zone note quando un ciclo termina o va in errore;
- applicare un limite massimo di minuti per zona.

## Policy idraulica

Per impostazione predefinita le zone vengono eseguite una alla volta.

```json
{
  "hydraulic": {
    "allow_parallel_zones": false,
    "max_parallel_zones": 1,
    "pause_between_zones_seconds": 0
  }
}
```

Se l'impianto supporta abbastanza portata e pressione, puoi abilitare zone parallele:

```json
{
  "hydraulic": {
    "allow_parallel_zones": true,
    "max_parallel_zones": 2,
    "pause_between_zones_seconds": 5
  }
}
```

Gli step con piu zone vengono eseguiti in batch rispettando `max_parallel_zones`.

## MQTT Discovery

Se Home Assistant ha l'integrazione MQTT, l'add-on pubblica discovery tramite il servizio `mqtt.publish`.

Le entita create descrivono solo il controller:

- ciclo attivo;
- zona attiva;
- tempo residuo;
- prossimo ciclo;
- prossima esecuzione;
- stato running;
- conteggio errori/avvisi configurazione.

Le valvole restano le entita `switch.*` gia create da `MQTT_NET_COMELIT`.
