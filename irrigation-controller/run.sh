#!/usr/bin/env sh
set -e

export HA_BASE_URL="${HA_BASE_URL:-http://supervisor/core/api}"
export HA_TOKEN="${HA_TOKEN:-$SUPERVISOR_TOKEN}"
export IRRIGATION_CONFIG_PATH="${IRRIGATION_CONFIG_PATH:-/data/irrigation.json}"
export IRRIGATION_STATE_PATH="${IRRIGATION_STATE_PATH:-/data/state.json}"

exec dotnet /app/IrrigationController.dll
