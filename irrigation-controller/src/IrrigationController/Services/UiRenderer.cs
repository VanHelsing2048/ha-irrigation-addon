namespace IrrigationController.Services;

public sealed class UiRenderer
{
    public string Render() => """
<!doctype html>
<html lang="it">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Irrigazione</title>
  <style>
    :root {
      color-scheme: light dark;
      font-family: "Segoe UI", system-ui, sans-serif;
      background: #f5f6f0;
      color: #20231f;
      --bg: #f5f6f0;
      --panel: #ffffff;
      --panel-2: #f0f3ea;
      --border: #d4dccb;
      --text: #20231f;
      --muted: #606b5a;
      --accent: #25734c;
      --accent-2: #0d5f7a;
      --danger: #b33b32;
      --warn: #a86d00;
      --ok: #247747;
    }
    @media (prefers-color-scheme: dark) {
      :root {
        background: #171a16;
        color: #eef2e9;
        --bg: #171a16;
        --panel: #20241e;
        --panel-2: #272d24;
        --border: #384030;
        --text: #eef2e9;
        --muted: #aeb8a7;
        --accent: #4fa873;
        --accent-2: #4aa5c0;
        --danger: #cf6259;
        --warn: #d49a3a;
        --ok: #58b77b;
      }
    }
    * { box-sizing: border-box; }
    body { margin: 0; background: var(--bg); color: var(--text); }
    button, input, select, textarea { font: inherit; }
    button {
      border: 0; border-radius: 6px; min-height: 36px; padding: 8px 12px;
      background: var(--accent); color: white; cursor: pointer; white-space: nowrap;
    }
    button.secondary { background: #5c6856; }
    button.blue { background: var(--accent-2); }
    button.danger { background: var(--danger); }
    button.ghost { background: transparent; color: var(--text); border: 1px solid var(--border); }
    button:disabled { opacity: .5; cursor: default; }
    input, select, textarea {
      width: 100%; min-height: 36px; padding: 8px 10px; border-radius: 6px;
      border: 1px solid var(--border); background: var(--panel); color: var(--text);
    }
    textarea { resize: vertical; min-height: 90px; font-family: Consolas, monospace; }
    label { display: grid; gap: 5px; color: var(--muted); font-size: 12px; }
    label span { color: var(--muted); }
    h1, h2, h3 { margin: 0; letter-spacing: 0; }
    h1 { font-size: 26px; }
    h2 { font-size: 18px; }
    h3 { font-size: 15px; }
    .app { min-height: 100vh; display: grid; grid-template-columns: 232px minmax(0, 1fr); }
    .sidebar { padding: 18px; border-right: 1px solid var(--border); background: var(--panel); position: sticky; top: 0; height: 100vh; }
    .brand { display: grid; gap: 4px; margin-bottom: 22px; }
    .brand small { color: var(--muted); }
    .nav { display: grid; gap: 6px; }
    .nav button { justify-content: flex-start; text-align: left; background: transparent; color: var(--text); border: 1px solid transparent; }
    .nav button.active { background: var(--panel-2); border-color: var(--border); }
    .main { min-width: 0; padding: 22px; }
    .topbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 18px; }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; justify-content: flex-end; }
    .grid { display: grid; gap: 12px; }
    .metrics { grid-template-columns: repeat(4, minmax(0, 1fr)); }
    .two { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .three { grid-template-columns: repeat(3, minmax(0, 1fr)); }
    .card { background: var(--panel); border: 1px solid var(--border); border-radius: 8px; padding: 14px; min-width: 0; }
    .metric strong { display: block; font-size: 20px; margin-top: 3px; overflow-wrap: anywhere; }
    .muted { color: var(--muted); }
    .section { display: grid; gap: 12px; margin-top: 18px; }
    .toolbar { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
    .row { display: grid; grid-template-columns: repeat(12, minmax(0, 1fr)); gap: 10px; align-items: end; }
    .span-2 { grid-column: span 2; }
    .span-3 { grid-column: span 3; }
    .span-4 { grid-column: span 4; }
    .span-5 { grid-column: span 5; }
    .span-6 { grid-column: span 6; }
    .span-8 { grid-column: span 8; }
    .span-12 { grid-column: span 12; }
    .list { display: grid; gap: 10px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 10px; border-bottom: 1px solid var(--border); text-align: left; vertical-align: top; }
    th { color: var(--muted); font-size: 12px; text-transform: uppercase; }
    .pill { display: inline-flex; align-items: center; min-height: 24px; padding: 3px 8px; border-radius: 999px; font-size: 12px; background: var(--panel-2); }
    .pill.on, .pill.ok { color: var(--ok); }
    .pill.warn { color: var(--warn); }
    .pill.danger { color: var(--danger); }
    .days { display: flex; gap: 6px; flex-wrap: wrap; }
    .days label { display: inline-flex; align-items: center; gap: 5px; padding: 6px 8px; border: 1px solid var(--border); border-radius: 6px; color: var(--text); }
    .days input { width: auto; min-height: auto; }
    .notice { border-left: 4px solid var(--accent); }
    .notice.warn { border-left-color: var(--warn); }
    .notice.danger { border-left-color: var(--danger); }
    .toast {
      position: fixed; right: 18px; bottom: 18px; max-width: 420px; padding: 12px 14px;
      border-radius: 8px; background: var(--panel); border: 1px solid var(--border); box-shadow: 0 10px 30px #0003;
      display: none; z-index: 5;
    }
    .toast.show { display: block; }
    .hidden { display: none !important; }
    @media (max-width: 900px) {
      .app { grid-template-columns: 1fr; }
      .sidebar { position: static; height: auto; border-right: 0; border-bottom: 1px solid var(--border); }
      .nav { grid-template-columns: repeat(3, minmax(0, 1fr)); }
      .main { padding: 14px; }
      .metrics, .two, .three { grid-template-columns: 1fr; }
      .row { grid-template-columns: 1fr; }
      .row > * { grid-column: span 1 !important; }
      .topbar { align-items: flex-start; flex-direction: column; }
      .actions { justify-content: flex-start; }
    }
  </style>
</head>
<body>
  <div class="app">
    <aside class="sidebar">
      <div class="brand">
        <h1>Irrigazione</h1>
        <small>Controller Home Assistant</small>
      </div>
      <nav class="nav" id="nav"></nav>
    </aside>
    <main class="main">
      <div class="topbar">
        <div>
          <h2 id="pageTitle">Dashboard</h2>
          <div class="muted" id="pageSubtitle"></div>
        </div>
        <div class="actions">
          <button class="ghost" onclick="reloadAll()">Aggiorna</button>
          <button class="danger" onclick="globalStop()">Stop</button>
        </div>
      </div>
      <div id="content"></div>
    </main>
  </div>
  <div class="toast" id="toast"></div>
<script>
const pages = [
  ['dashboard', 'Dashboard', 'Stato, prossime partenze e diagnostica'],
  ['zones', 'Zone', 'Valvole, resa, calibrazione e limiti'],
  ['cycles', 'Cicli', 'Sequenze manuali e automatiche'],
  ['weather', 'Meteo', 'Pioggia, ET e soglie di blocco'],
  ['plant', 'Impianto', 'Idraulica e sicurezze'],
  ['diagnostics', 'Diagnostica', 'Eventi, decisioni e ultimo meteo'],
  ['raw', 'JSON', 'Editor avanzato']
];
const dayNames = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
const dayLabels = ['Dom','Lun','Mar','Mer','Gio','Ven','Sab'];
let config = null;
let overview = null;
let currentPage = location.hash.replace('#','') || 'dashboard';

function esc(value) {
  return String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function num(value, fallback = 0) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
}
function bool(id) { return document.getElementById(id)?.checked ?? false; }
function val(id) { return document.getElementById(id)?.value ?? ''; }
function toast(message, danger = false) {
  const el = document.getElementById('toast');
  el.textContent = message;
  el.style.borderColor = danger ? 'var(--danger)' : 'var(--border)';
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 4200);
}
async function api(path, options = {}) {
  const res = await fetch(apiUrl(path), options);
  const text = await res.text();
  const body = text ? JSON.parse(text) : {};
  if (!res.ok) throw body;
  return body;
}
function apiUrl(path) {
  const normalizedPath = path.startsWith('/') ? path : '/' + path;
  const ingressBase = location.pathname.replace(/\/+ui\/?$/, '').replace(/\/$/, '');
  return ingressBase + normalizedPath;
}
async function reloadAll() {
  [config, overview] = await Promise.all([api('/api/config'), api('/api/overview')]);
  render();
}
async function saveConfig(nextConfig = config) {
  try {
    const result = await api('/api/config', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(nextConfig)
    });
    config = nextConfig;
    overview = await api('/api/overview');
    render();
    toast(result.message || 'Configurazione salvata');
  } catch (error) {
    renderValidationError(error);
    toast('Configurazione non valida', true);
  }
}
function cloneConfig() { return JSON.parse(JSON.stringify(config)); }
function setPage(page) {
  currentPage = page;
  location.hash = page;
  render();
}
window.addEventListener('hashchange', () => {
  currentPage = location.hash.replace('#','') || 'dashboard';
  render();
});
function render() {
  renderNav();
  const page = pages.find(x => x[0] === currentPage) || pages[0];
  document.getElementById('pageTitle').textContent = page[1];
  document.getElementById('pageSubtitle').textContent = page[2];
  const content = document.getElementById('content');
  if (!config || !overview) {
    content.innerHTML = '<div class="card">Caricamento...</div>';
    return;
  }
  content.innerHTML = ({
    dashboard: renderDashboard,
    zones: renderZones,
    cycles: renderCycles,
    weather: renderWeather,
    plant: renderPlant,
    diagnostics: renderDiagnostics,
    raw: renderRaw
  }[currentPage] || renderDashboard)();
}
function renderNav() {
  document.getElementById('nav').innerHTML = pages.map(([id, label]) =>
    `<button class="${id === currentPage ? 'active' : ''}" onclick="setPage('${id}')">${esc(label)}</button>`
  ).join('');
}
function renderDashboard() {
  const runner = overview.runner || {};
  const validation = overview.validation || { errors: [], warnings: [] };
  const weather = overview.diagnostics?.last_weather;
  return `
    <section class="grid metrics">
      ${metric('Runner', runner.status || 'idle')}
      ${metric('Ciclo', runner.cycle_name || '-')}
      ${metric('Zona', runner.zone_name || '-')}
      ${metric('Bilancio', overview.last_water_balance_update_date || '-')}
    </section>
    <section class="section">
      ${validationCard(validation)}
      <div class="grid two">
        <div class="card">
          <h3>Cicli</h3>
          <table><thead><tr><th>Nome</th><th>Modo</th><th>Prossima</th><th></th></tr></thead><tbody>
            ${(overview.cycles || []).map(c => `<tr><td><strong>${esc(c.name)}</strong><div class="muted">${esc(c.id)}</div></td><td>${esc(c.mode)}</td><td>${esc(c.next_run_text)}</td><td><button onclick="startCycle('${esc(c.id)}')">Start</button></td></tr>`).join('')}
          </tbody></table>
        </div>
        <div class="card">
          <h3>Meteo</h3>
          <p>${weather ? `ET0 ${weather.et0_mm?.toFixed?.(1) ?? weather.et0_mm} mm, pioggia ${weather.expected_rain_mm?.toFixed?.(1) ?? weather.expected_rain_mm} mm, utile ${weather.effective_rain_mm?.toFixed?.(1) ?? weather.effective_rain_mm} mm, probabilità ${weather.max_rain_probability}%` : '-'}</p>
          <p class="muted">${esc(overview.diagnostics?.last_decision?.message || 'Nessuna decisione registrata')}</p>
        </div>
      </div>
      <div class="card">
        <h3>Zone</h3>
        <table><thead><tr><th>Zona</th><th>Stato</th><th>Deficit</th><th>Calibrazione</th><th></th></tr></thead><tbody>
          ${(overview.zones || []).map(z => `<tr><td><strong>${esc(z.name)}</strong><div class="muted">${esc(z.entity)}</div></td><td><span class="pill ${esc(z.state_class)}">${esc(z.state)}</span></td><td>${num(z.water_balance_mm).toFixed(1)} mm</td><td>${esc(z.calibration_text)}</td><td><button onclick="startZone('${esc(z.id)}')">5 min</button></td></tr>`).join('')}
        </tbody></table>
      </div>
    </section>`;
}
function renderZones() {
  const zones = Object.entries(config.zones || {});
  return `<section class="section">
    <div class="toolbar"><h2>Zone</h2><button onclick="addZone()">Nuova zona</button></div>
    <div class="list">${zones.map(([id, z]) => zoneForm(id, z)).join('')}</div>
  </section>`;
}
function zoneForm(id, z) {
  const ov = (overview.zones || []).find(x => x.id === id);
  return `<div class="card" id="zone-${esc(id)}">
    <div class="toolbar"><h3>${esc(z.name || id)}</h3><button class="danger" onclick="deleteZone('${esc(id)}')">Elimina</button></div>
    <div class="row">
      ${field(`zone-${id}-id`, 'ID', id, 'span-2')}
      ${field(`zone-${id}-name`, 'Nome', z.name, 'span-3')}
      ${field(`zone-${id}-entity`, 'Entità switch', z.entity, 'span-4')}
      ${numberField(`zone-${id}-rate`, 'Resa mm/h', z.precipitation_rate_mm_h, 'span-3', '0.01')}
      ${numberField(`zone-${id}-crop`, 'Coeff. coltura', z.crop_coefficient, 'span-2', '0.01')}
      ${numberField(`zone-${id}-min`, 'Min minuti', z.min_minutes, 'span-2')}
      ${numberField(`zone-${id}-max`, 'Max minuti', z.max_minutes, 'span-2')}
      ${numberField(`zone-${id}-target`, 'Deficit target mm', z.target_deficit_mm, 'span-2', '0.1')}
      ${field(`zone-${id}-soil`, 'Sensore umidità', z.soil_moisture_entity || '', 'span-2')}
      ${numberField(`zone-${id}-soilskip`, 'Skip umidità sopra', z.skip_if_soil_moisture_above ?? '', 'span-2', '0.1')}
    </div>
    <div class="actions" style="margin-top:12px; justify-content:flex-start">
      <button onclick="saveZone('${esc(id)}')">Salva zona</button>
      <button class="secondary" onclick="calibrateZone('${esc(id)}')">Calibra</button>
      <button class="secondary" onclick="applyCalibration('${esc(id)}')">Applica calibrazione</button>
      <button class="blue" onclick="startZone('${esc(id)}')">Avvia 5 min</button>
      <button class="secondary" onclick="stopZone('${esc(id)}')">Stop</button>
      <span class="muted">Ultima calibrazione: ${esc(ov?.calibration_text || '-')}</span>
    </div>
  </div>`;
}
function renderCycles() {
  return `<section class="section">
    <div class="toolbar"><h2>Cicli</h2><button onclick="addCycle()">Nuovo ciclo</button></div>
    <div class="list">${Object.entries(config.cycles || {}).map(([id, c]) => cycleForm(id, c)).join('')}</div>
  </section>`;
}
function cycleForm(id, c) {
  const schedule = c.schedule || { days: [], times: [] };
  const steps = (c.steps || []).map(s => `${(s.zones || []).join(',')} | ${s.duration_minutes ?? ''}`).join('\n');
  return `<div class="card">
    <div class="toolbar"><h3>${esc(c.name || id)}</h3><button class="danger" onclick="deleteCycle('${esc(id)}')">Elimina</button></div>
    <div class="row">
      ${field(`cycle-${id}-id`, 'ID', id, 'span-2')}
      ${field(`cycle-${id}-name`, 'Nome', c.name, 'span-3')}
      <label class="span-2"><span>Modo</span><select id="cycle-${esc(id)}-mode"><option ${c.mode === 'Manual' ? 'selected' : ''}>Manual</option><option ${c.mode === 'Automatic' ? 'selected' : ''}>Automatic</option></select></label>
      <label class="span-2"><span>Abilitato</span><select id="cycle-${esc(id)}-enabled"><option value="true" ${c.enabled !== false ? 'selected' : ''}>Sì</option><option value="false" ${c.enabled === false ? 'selected' : ''}>No</option></select></label>
      ${field(`cycle-${id}-times`, 'Orari', (schedule.times || []).join(', '), 'span-3')}
      <div class="span-12">${daysControl(`cycle-${id}-days`, schedule.days || [])}</div>
      <label class="span-12"><span>Step: zone separate da virgola, poi | durata minuti. Esempio: prato,orto | 10</span><textarea id="cycle-${esc(id)}-steps">${esc(steps)}</textarea></label>
    </div>
    <div class="actions" style="margin-top:12px; justify-content:flex-start">
      <button onclick="saveCycle('${esc(id)}')">Salva ciclo</button>
      <button class="blue" onclick="startCycle('${esc(id)}')">Avvia</button>
    </div>
  </div>`;
}
function renderWeather() {
  const w = config.weather || {};
  return `<section class="section">
  <div class="card notice"><strong>Configurazione Home Assistant</strong><p class="muted">In uso reale queste opzioni generali sono pensate per la scheda Config dell'add-on. Questa vista resta utile per sviluppo e modifiche avanzate.</p></div>
  <div class="card"><div class="row">
    ${field('weather-entity', 'Entità meteo', w.entity, 'span-4')}
    ${field('weather-type', 'Tipo forecast', w.forecast_type || 'hourly', 'span-2')}
    ${numberField('weather-lookahead', 'Ore previsione', w.rain_lookahead_hours, 'span-2')}
    ${numberField('weather-efficiency', 'Efficienza pioggia', w.rain_efficiency, 'span-2', '0.01')}
    ${numberField('weather-skipmm', 'Skip sopra mm', w.skip_if_expected_rain_mm_above, 'span-2', '0.1')}
    ${numberField('weather-skipprob', 'Skip probabilità %', w.skip_if_rain_probability_above, 'span-2')}
    ${field('weather-et0', 'Sensore ET0 opzionale', w.external_et0_sensor_entity || '', 'span-4')}
  </div><div class="actions" style="margin-top:12px; justify-content:flex-start"><button onclick="saveWeather()">Salva meteo</button></div></div></section>`;
}
function renderPlant() {
  const h = config.hydraulic || {};
  const s = config.safety || {};
  const m = config.mqtt_discovery || {};
  return `<section class="section">
    <div class="card notice"><strong>Configurazione Home Assistant</strong><p class="muted">Idraulica, sicurezze e MQTT Discovery possono essere gestite dalla scheda Config dell'add-on. Il collegamento laterale usa Ingress con panel_title/panel_icon.</p></div>
    <div class="card"><h3>Idraulica</h3><div class="row">
      <label class="span-3"><span>Zone parallele</span><select id="hyd-parallel"><option value="false" ${!h.allow_parallel_zones ? 'selected' : ''}>No</option><option value="true" ${h.allow_parallel_zones ? 'selected' : ''}>Sì</option></select></label>
      ${numberField('hyd-max', 'Max zone insieme', h.max_parallel_zones, 'span-3')}
      ${numberField('hyd-pause', 'Pausa tra zone sec.', h.pause_between_zones_seconds, 'span-3')}
    </div></div>
    <div class="card"><h3>Sicurezze</h3><div class="row">
      ${selectBool('safe-startup', 'Spegni all’avvio', s.turn_off_all_zones_on_startup, 'span-3')}
      ${selectBool('safe-error', 'Spegni su errore', s.stop_all_known_zones_on_error, 'span-3')}
      ${selectBool('safe-verify', 'Verifica stato valvole', s.verify_zone_state_after_switch, 'span-3')}
      ${selectBool('safe-manualweather', 'Manuale ignora meteo', s.manual_runs_ignore_weather, 'span-3')}
      ${numberField('safe-retry', 'Retry comandi', s.switch_retry_count, 'span-3')}
      ${numberField('safe-delay', 'Delay retry ms', s.switch_retry_delay_ms, 'span-3')}
      ${numberField('safe-maxminutes', 'Max minuti zona', s.max_zone_minutes, 'span-3')}
    </div></div>
    <div class="card"><h3>MQTT Discovery</h3><div class="row">
      ${selectBool('mqtt-enabled', 'Abilitato', m.enabled, 'span-3')}
      ${field('mqtt-prefix', 'Discovery prefix', m.discovery_prefix, 'span-3')}
      ${field('mqtt-topic', 'Base topic', m.base_topic, 'span-3')}
      ${numberField('mqtt-interval', 'Intervallo sec.', m.publish_interval_seconds, 'span-3')}
    </div></div>
    <div class="actions" style="justify-content:flex-start"><button onclick="savePlant()">Salva impianto</button></div>
  </section>`;
}
function renderDiagnostics() {
  const events = overview.recent_events || [];
  const d = overview.diagnostics || {};
  return `<section class="section">
    <div class="grid three">
      <div class="card"><h3>Ultimo meteo</h3><pre>${esc(JSON.stringify(d.last_weather || {}, null, 2))}</pre></div>
      <div class="card"><h3>Ultima decisione</h3><pre>${esc(JSON.stringify(d.last_decision || {}, null, 2))}</pre></div>
      <div class="card"><h3>Ultimo errore</h3><pre>${esc(JSON.stringify(d.last_error || {}, null, 2))}</pre></div>
    </div>
    <div class="card"><h3>Eventi recenti</h3><table><thead><tr><th>Quando</th><th>Tipo</th><th>Zona</th><th>Messaggio</th></tr></thead><tbody>${events.map(e => `<tr><td>${esc(new Date(e.timestamp).toLocaleString())}</td><td>${esc(e.type)}</td><td>${esc(e.zone_id || '-')}</td><td>${esc(e.message)}</td></tr>`).join('')}</tbody></table></div>
  </section>`;
}
function renderRaw() {
  return `<section class="section"><div class="card">
    <textarea id="raw-json" style="min-height:65vh">${esc(JSON.stringify(config, null, 2))}</textarea>
    <div class="actions" style="margin-top:12px; justify-content:flex-start"><button onclick="saveRaw()">Salva JSON</button></div>
  </div></section>`;
}
function metric(label, value) { return `<div class="card metric"><span class="muted">${esc(label)}</span><strong>${esc(value)}</strong></div>`; }
function validationCard(validation) {
  const errors = validation.errors || [];
  const warnings = validation.warnings || [];
  const issues = [...errors.map(x => ['danger', x]), ...warnings.map(x => ['warn', x])];
  if (!issues.length) return '<div class="card notice"><strong>Configurazione valida</strong></div>';
  return `<div class="card notice ${errors.length ? 'danger' : 'warn'}"><strong>Configurazione da verificare</strong><ul>${issues.map(([cls, x]) => `<li class="${cls}"><strong>${esc(x.path)}</strong> ${esc(x.message)}</li>`).join('')}</ul></div>`;
}
function field(id, label, value, cls = '') { return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" value="${esc(value ?? '')}"></label>`; }
function numberField(id, label, value, cls = '', step = '1') { return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" type="number" step="${esc(step)}" value="${esc(value ?? '')}"></label>`; }
function selectBool(id, label, value, cls = '') { return `<label class="${cls}"><span>${esc(label)}</span><select id="${esc(id)}"><option value="true" ${value ? 'selected' : ''}>Sì</option><option value="false" ${!value ? 'selected' : ''}>No</option></select></label>`; }
function daysControl(id, values) {
  return `<div class="days">${dayNames.map((d, i) => `<label><input type="checkbox" data-days="${esc(id)}" value="${d}" ${values.includes(d) ? 'checked' : ''}>${dayLabels[i]}</label>`).join('')}</div>`;
}
function getDays(id) { return [...document.querySelectorAll(`[data-days="${id}"]:checked`)].map(x => x.value); }
function parseSteps(text) {
  return text.split('\n').map(line => line.trim()).filter(Boolean).map(line => {
    const [zonesPart, durationPart] = line.split('|').map(x => (x || '').trim());
    const duration = durationPart ? Number(durationPart) : null;
    return { zones: zonesPart.split(',').map(x => x.trim()).filter(Boolean), duration_minutes: Number.isFinite(duration) ? duration : null };
  });
}
async function startCycle(id) { try { toast((await api('/api/cycles/' + id + '/start', { method: 'POST' })).message); } catch(e) { toast(e.message || 'Errore avvio ciclo', true); } }
async function startZone(id) { try { toast((await api('/api/zones/' + id + '/start?minutes=5', { method: 'POST' })).message); } catch(e) { toast(e.message || 'Errore avvio zona', true); } }
async function stopZone(id) { try { await api('/api/zones/' + id + '/stop', { method: 'POST' }); toast('Zona fermata'); } catch(e) { toast(e.message || 'Errore stop zona', true); } }
async function globalStop() { try { await api('/api/stop', { method: 'POST' }); toast('Stop richiesto'); } catch(e) { toast(e.message || 'Errore stop', true); } }
async function applyCalibration(id) { try { toast((await api('/api/calibration/zones/' + id + '/apply', { method: 'POST' })).message); await reloadAll(); } catch(e) { toast(e.message || 'Nessuna calibrazione da applicare', true); } }
async function calibrateZone(id) {
  const minutes = prompt('Minuti test calibrazione', '10');
  if (!minutes) return;
  try { await api('/api/calibration/zones/' + id + '/start?minutes=' + encodeURIComponent(minutes), { method: 'POST' }); }
  catch (e) { toast(e.message || 'Errore avvio calibrazione', true); return; }
  const values = prompt('Misure mm separate da virgola, es. 1.8,2.1,1.6');
  if (!values) return;
  const measurements = values.split(',').map(x => Number(x.trim())).filter(x => !Number.isNaN(x));
  try {
    const result = await api('/api/calibration/zones/' + id + '/complete', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ minutes: Number(minutes), measurements_mm: measurements }) });
    toast(result.recommendation || 'Calibrazione salvata');
    await reloadAll();
  } catch (e) { toast(e.message || 'Misure non valide', true); }
}
async function saveZone(id) {
  const next = cloneConfig();
  const newId = val(`zone-${id}-id`).trim();
  if (!newId) return toast('ID zona obbligatorio', true);
  const z = {
    name: val(`zone-${id}-name`), entity: val(`zone-${id}-entity`),
    precipitation_rate_mm_h: num(val(`zone-${id}-rate`), 10),
    crop_coefficient: num(val(`zone-${id}-crop`), 1),
    min_minutes: num(val(`zone-${id}-min`), 3),
    max_minutes: num(val(`zone-${id}-max`), 30),
    target_deficit_mm: num(val(`zone-${id}-target`), 0),
    soil_moisture_entity: val(`zone-${id}-soil`) || null,
    skip_if_soil_moisture_above: val(`zone-${id}-soilskip`) === '' ? null : num(val(`zone-${id}-soilskip`))
  };
  delete next.zones[id];
  next.zones[newId] = z;
  await saveConfig(next);
}
async function addZone() {
  const id = prompt('ID nuova zona, es. prato_nord');
  if (!id) return;
  const next = cloneConfig();
  next.zones ||= {};
  next.zones[id] = { name: id, entity: 'switch.', precipitation_rate_mm_h: 10, crop_coefficient: 1, min_minutes: 3, max_minutes: 30, target_deficit_mm: 0, soil_moisture_entity: null, skip_if_soil_moisture_above: null };
  await saveConfig(next);
  setPage('zones');
}
async function deleteZone(id) {
  if (!confirm('Eliminare la zona ' + id + '?')) return;
  const next = cloneConfig();
  delete next.zones[id];
  await saveConfig(next);
}
async function saveCycle(id) {
  const next = cloneConfig();
  const newId = val(`cycle-${id}-id`).trim();
  if (!newId) return toast('ID ciclo obbligatorio', true);
  const mode = val(`cycle-${id}-mode`);
  const times = val(`cycle-${id}-times`).split(',').map(x => x.trim()).filter(Boolean);
  const c = {
    name: val(`cycle-${id}-name`),
    enabled: val(`cycle-${id}-enabled`) === 'true',
    mode,
    schedule: mode === 'Automatic' ? { days: getDays(`cycle-${id}-days`), times } : null,
    steps: parseSteps(val(`cycle-${id}-steps`))
  };
  delete next.cycles[id];
  next.cycles[newId] = c;
  await saveConfig(next);
}
async function addCycle() {
  const id = prompt('ID nuovo ciclo, es. mattina_prato');
  if (!id) return;
  const firstZone = Object.keys(config.zones || {})[0] || '';
  const next = cloneConfig();
  next.cycles ||= {};
  next.cycles[id] = { name: id, enabled: true, mode: 'Manual', schedule: null, steps: [{ zones: firstZone ? [firstZone] : [], duration_minutes: 10 }] };
  await saveConfig(next);
  setPage('cycles');
}
async function deleteCycle(id) {
  if (!confirm('Eliminare il ciclo ' + id + '?')) return;
  const next = cloneConfig();
  delete next.cycles[id];
  await saveConfig(next);
}
async function saveWeather() {
  const next = cloneConfig();
  next.weather = {
    entity: val('weather-entity'), forecast_type: val('weather-type'),
    rain_lookahead_hours: num(val('weather-lookahead'), 24),
    rain_efficiency: num(val('weather-efficiency'), .75),
    skip_if_expected_rain_mm_above: num(val('weather-skipmm'), 4),
    skip_if_rain_probability_above: num(val('weather-skipprob'), 70),
    external_et0_sensor_entity: val('weather-et0') || null
  };
  await saveConfig(next);
}
async function savePlant() {
  const next = cloneConfig();
  next.hydraulic = { allow_parallel_zones: val('hyd-parallel') === 'true', max_parallel_zones: num(val('hyd-max'), 1), pause_between_zones_seconds: num(val('hyd-pause'), 0) };
  next.safety = {
    ...(next.safety || {}),
    turn_off_all_zones_on_startup: val('safe-startup') === 'true',
    stop_all_known_zones_on_error: val('safe-error') === 'true',
    verify_zone_state_after_switch: val('safe-verify') === 'true',
    manual_runs_ignore_weather: val('safe-manualweather') === 'true',
    switch_retry_count: num(val('safe-retry'), 2),
    switch_retry_delay_ms: num(val('safe-delay'), 750),
    max_zone_minutes: num(val('safe-maxminutes'), 60)
  };
  next.mqtt_discovery = { enabled: val('mqtt-enabled') === 'true', discovery_prefix: val('mqtt-prefix'), base_topic: val('mqtt-topic'), publish_interval_seconds: num(val('mqtt-interval'), 30) };
  await saveConfig(next);
}
async function saveRaw() {
  try { await saveConfig(JSON.parse(val('raw-json'))); }
  catch (e) { toast('JSON non valido: ' + e.message, true); }
}
function renderValidationError(error) {
  const details = [...(error.errors || []), ...(error.warnings || [])].map(x => `${x.path}: ${x.message}`).join('\n');
  if (details) alert(details);
}
reloadAll().catch(error => {
  document.getElementById('content').innerHTML = `<div class="card notice danger"><strong>Errore caricamento</strong><p>${esc(error.message || JSON.stringify(error))}</p></div>`;
});
</script>
</body>
</html>
""";
}
