namespace IrrigationController.Services;

public sealed class UiRenderer
{
    public string Render()
    {
        var versionAttribute = System.Reflection.Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault();
        var version = versionAttribute?.InformationalVersion ?? "unknown";

        return HtmlTemplate.Replace("{{APP_VERSION}}", version, StringComparison.Ordinal);
    }

    private const string HtmlTemplate = """
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
      background: #eef3ee;
      color: #20231f;
      --bg: #eef3ee;
      --panel: #ffffff;
      --panel-2: #f6f8f3;
      --panel-3: #e6eee4;
      --border: #cfdbcf;
      --text: #20231f;
      --muted: #606b5a;
      --accent: #216e48;
      --accent-2: #0c6478;
      --danger: #b33b32;
      --warn: #a86d00;
      --ok: #247747;
      --shadow: 0 10px 30px rgba(34, 49, 34, .08);
    }
    @media (prefers-color-scheme: dark) {
      :root {
        background: #171a16;
        color: #eef2e9;
        --bg: #171a16;
        --panel: #20241e;
        --panel-2: #272d24;
        --panel-3: #30382d;
        --border: #3b4638;
        --text: #eef2e9;
        --muted: #aeb8a7;
        --accent: #4fa873;
        --accent-2: #4aa5c0;
        --danger: #cf6259;
        --warn: #d49a3a;
        --ok: #58b77b;
        --shadow: 0 12px 34px rgba(0, 0, 0, .28);
      }
    }
    * { box-sizing: border-box; }
    body { margin: 0; background: var(--bg); color: var(--text); }
    button, input, select, textarea { font: inherit; }
    button {
      border: 0; border-radius: 7px; min-height: 36px; padding: 8px 12px;
      background: var(--accent); color: white; cursor: pointer; white-space: nowrap;
      display: inline-flex; align-items: center; justify-content: center; gap: 6px;
      transition: transform .12s ease, filter .12s ease, border-color .12s ease, background .12s ease;
    }
    button:hover { filter: brightness(1.05); }
    button:active { transform: translateY(1px); }
    button:focus-visible, input:focus-visible, select:focus-visible, textarea:focus-visible {
      outline: 2px solid color-mix(in srgb, var(--accent) 55%, transparent);
      outline-offset: 2px;
    }
    button.secondary { background: #5c6856; }
    button.blue { background: var(--accent-2); }
    button.danger { background: var(--danger); }
    button.ghost { background: transparent; color: var(--text); border: 1px solid var(--border); }
    button:disabled { opacity: .5; cursor: default; }
    input, select, textarea {
      width: 100%; min-height: 36px; padding: 8px 10px; border-radius: 7px;
      border: 1px solid var(--border); background: var(--panel); color: var(--text);
    }
    textarea { resize: vertical; min-height: 90px; font-family: Consolas, monospace; }
    label { display: grid; gap: 5px; color: var(--muted); font-size: 12px; }
    label span { color: var(--muted); }
    h1, h2, h3 { margin: 0; letter-spacing: 0; }
    h1 { font-size: 26px; }
    h2 { font-size: 18px; }
    h3 { font-size: 15px; }
    .app { min-height: 100vh; display: grid; grid-template-columns: 252px minmax(0, 1fr); }
    .sidebar { padding: 18px; border-right: 1px solid var(--border); background: var(--panel); position: sticky; top: 0; height: 100vh; }
    .brand { display: grid; gap: 6px; margin-bottom: 22px; padding: 12px; border-radius: 8px; background: var(--panel-2); border: 1px solid var(--border); }
    .brand small { color: var(--muted); }
    .nav { display: grid; gap: 6px; }
    .nav button { justify-content: flex-start; text-align: left; background: transparent; color: var(--text); border: 1px solid transparent; min-height: 40px; }
    .nav button.active { background: var(--panel-2); border-color: var(--border); box-shadow: inset 3px 0 0 var(--accent); }
    .nav-code { display: inline-grid; place-items: center; width: 32px; height: 26px; border-radius: 7px; background: var(--panel-3); font-size: 11px; font-weight: 800; color: var(--accent); }
    .main { min-width: 0; padding: 22px; }
    .topbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 18px; padding-bottom: 12px; border-bottom: 1px solid var(--border); }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; justify-content: flex-end; }
    .grid { display: grid; gap: 12px; }
    .metrics { grid-template-columns: repeat(4, minmax(0, 1fr)); }
    .summary { grid-template-columns: repeat(3, minmax(0, 1fr)); }
    .two { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .three { grid-template-columns: repeat(3, minmax(0, 1fr)); }
    .card { background: var(--panel); border: 1px solid var(--border); border-radius: 8px; padding: 14px; min-width: 0; box-shadow: var(--shadow); }
    .metric { display: grid; gap: 4px; }
    .metric strong { display: block; font-size: 20px; margin-top: 3px; overflow-wrap: anywhere; }
    .metric span:first-child { text-transform: uppercase; letter-spacing: .02em; font-size: 11px; }
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
    tbody tr:hover { background: var(--panel-2); }
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
    .validation-panel { display: grid; gap: 8px; }
    .validation-panel ul { margin: 0; padding-left: 18px; }
    .validation-panel li { margin: 4px 0; }
    .validation-panel .danger { color: var(--danger); }
    .validation-panel .warn { color: var(--warn); }
    .plan { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
    .plan-day { display: grid; gap: 12px; }
    .plan-head { display: grid; grid-template-columns: 68px minmax(0, 1fr); gap: 12px; align-items: center; }
    .weather-icon { width: 64px; height: 64px; }
    .icon-badge {
      display: inline-grid; place-items: center; border-radius: 8px; background: var(--panel-2);
      border: 1px solid var(--border); color: var(--accent); min-width: 32px; min-height: 32px; padding: 4px; line-height: 1;
    }
    .icon-badge svg { width: 22px; height: 22px; stroke: currentColor; fill: none; stroke-width: 2; stroke-linecap: round; stroke-linejoin: round; }
    .icon-badge.big { width: 64px; height: 64px; }
    .icon-badge.big svg { width: 42px; height: 42px; }
    .decision { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; margin-top: 5px; }
    .mini { display: flex; gap: 8px; flex-wrap: wrap; }
    .weather-summary { display: grid; grid-template-columns: minmax(220px, 0.8fr) repeat(2, minmax(0, 1fr)); gap: 12px; align-items: stretch; }
    .weather-kpi { display: grid; gap: 6px; }
    .weather-kpi strong { font-size: 22px; }
    .forecast-card { display: grid; gap: 8px; }
    .forecast-line { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
    .forecast-meta { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; color: var(--muted); font-size: 12px; }
    .cycle-preview { display: grid; gap: 8px; margin-top: 12px; }
    .cycle-preview-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px; }
    .cycle-preview-day { display: grid; gap: 8px; background: var(--panel-2); border: 1px solid var(--border); border-radius: 8px; padding: 10px; }
    .setting-board { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; }
    .setting-tile { display: grid; gap: 5px; background: var(--panel-2); border: 1px solid var(--border); border-radius: 8px; padding: 10px; }
    .setting-tile span:first-child { color: var(--muted); font-size: 12px; }
    .plant-flow { display: grid; grid-template-columns: minmax(180px, .55fr) minmax(0, 1fr); gap: 12px; align-items: stretch; }
    .flow-node { display: grid; gap: 8px; background: var(--panel-2); border: 1px solid var(--border); border-radius: 8px; padding: 12px; }
    .flow-zones { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 8px; }
    .step-row, .time-row { display: grid; grid-template-columns: minmax(180px, 1fr) 130px auto; gap: 8px; align-items: end; }
    .cycle-chip, .zone-chip, .event-chip { display: flex; align-items: center; gap: 8px; min-width: 0; }
    .zone-chip { padding: 6px 8px; border: 1px solid var(--border); border-radius: 6px; }
    .event-chip { color: var(--muted); }
    .toast {
      position: fixed; right: 18px; bottom: 18px; max-width: 420px; padding: 12px 14px;
      border-radius: 8px; background: var(--panel); border: 1px solid var(--border); box-shadow: 0 10px 30px #0003;
      display: none; z-index: 5;
    }
    .toast.show { display: block; }
    .empty { display: grid; gap: 6px; padding: 18px; border: 1px dashed var(--border); border-radius: 8px; background: var(--panel-2); color: var(--muted); }
    .empty strong { color: var(--text); }
    .hidden { display: none !important; }
    @media (max-width: 900px) {
      .app { grid-template-columns: 1fr; }
      .sidebar { position: static; height: auto; border-right: 0; border-bottom: 1px solid var(--border); }
      .nav { grid-template-columns: repeat(3, minmax(0, 1fr)); }
      .main { padding: 14px; }
      .metrics, .summary, .two, .three, .plan, .weather-summary { grid-template-columns: 1fr; }
      .row { grid-template-columns: 1fr; }
      .step-row, .time-row, .cycle-preview-grid, .setting-board, .plant-flow { grid-template-columns: 1fr; }
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
        <small>Controller Home Assistant - v{{APP_VERSION}}</small>
        <span class="pill ok">Ingress UI</span>
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
let decisionPlan = null;
let irrigationEntities = [];
let weatherEntities = [];
let draftZones = {};
let draftCycles = {};
let lastValidation = null;
let currentPage = location.hash.replace('#','') || 'dashboard';

function esc(value) {
  return String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function js(value) { return JSON.stringify(String(value ?? '')); }
function action(name, value) { return `${name}(${js(value)})`; }
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
  [config, overview, decisionPlan, irrigationEntities, weatherEntities] = await Promise.all([
    api('/api/config'),
    api('/api/overview'),
    api('/api/decision-plan').catch(() => null),
    api('/api/entities/irrigation').catch(() => []),
    api('/api/entities/weather').catch(() => [])
  ]);
  lastValidation = null;
  render();
}
async function saveConfig(nextConfig = config, successMessage = 'Configurazione salvata', renderAfterSave = true, action = 'config_saved', zoneId = '', cycleId = '') {
  try {
    const result = await api('/api/config', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'X-Irrigation-Action': action,
        'X-Irrigation-Message': successMessage,
        'X-Irrigation-Cycle': cycleId,
        'X-Irrigation-Zone': zoneId
      },
      body: JSON.stringify(nextConfig)
    });
    config = nextConfig;
    overview = await api('/api/overview');
    lastValidation = null;
    if (renderAfterSave) render();
    toast(successMessage);
    return true;
  } catch (error) {
    lastValidation = error;
    renderValidationError(error);
    toast('Configurazione non valida', true);
    return false;
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
    `<button class="${id === currentPage ? 'active' : ''}" onclick="setPage('${id}')"><span class="nav-code">${esc(navCode(id))}</span>${esc(label)}</button>`
  ).join('');
}
function navCode(id) {
  return ({ dashboard: 'DB', zones: 'ZN', cycles: 'CY', weather: 'MT', plant: 'IM', diagnostics: 'LG', raw: 'JS' })[id] || 'UI';
}
function normalizeId(raw) {
  return String(raw ?? '').trim().toLowerCase()
    .replace(/[^a-z0-9_]+/g, '_')
    .replace(/^_+|_+$/g, '')
    .replace(/_+/g, '_');
}
function renderPlan() {
  if (!decisionPlan) {
    return '<section class="section"><div class="card notice warn"><strong>Piano non disponibile</strong></div></section>';
  }

  return `<section class="section">${renderPlanPanel()}</section>`;
}
function renderPlanPanel() {
  if (!decisionPlan) return '<div class="card notice warn"><strong>Piano non disponibile</strong></div>';
  return `
    <div class="plan">
      ${dayPlan(decisionPlan.today)}
      ${dayPlan(decisionPlan.tomorrow)}
    </div>`;
}
function dayPlan(day) {
  const cycles = day.cycles || [];
  const events = day.events || [];
  const forecastText = day.has_forecast ? `${day.forecast_count} previsioni ricevute` : 'Nessuna previsione ricevuta da Home Assistant';
  return `<div class="card plan-day">
    <div class="plan-head">
      <div class="weather-icon">${iconBadge(day.icon, true)}</div>
      <div>
        <h2>${esc(day.label)}</h2>
        <div class="decision"><strong>${esc(day.decision)}</strong><span class="pill ${esc(day.decision_class)}">${esc(day.weather_label)}</span></div>
        <div class="forecast-meta">${iconBadge(day.has_forecast ? 'OK' : 'INFO')} ${esc(forecastText)}</div>
        <div class="mini muted">
          <span title="Pioggia prevista">${iconBadge('RAIN')} ${num(day.expected_rain_mm).toFixed(1)} mm</span>
          <span title="Probabilita di pioggia">${iconBadge('PCT')} ${num(day.rain_probability)}%</span>
          <span title="Evapotraspirazione stimata">${iconBadge('ET')} ${num(day.et0_mm).toFixed(1)} mm</span>
        </div>
      </div>
    </div>
    <div class="list">
      ${cycles.length ? cycles.map(cyclePlan).join('') : emptyState('Nessun ciclo automatico', 'Aggiungi un ciclo automatico per vedere qui le prossime valutazioni meteo.')}
    </div>
    ${events.length ? `<div class="list">${events.map(eventPlan).join('')}</div>` : ''}
  </div>`;
}
function cyclePlan(cycle) {
  const zones = cycle.zones || [];
  return `<div class="card">
    <div class="toolbar">
      <div class="cycle-chip">${iconBadge(cycle.icon)}<strong>${esc(cycle.time)} ${esc(cycle.name)}</strong></div>
      <span class="pill ${esc(cycle.decision_class)}">${esc(cycle.decision)}</span>
    </div>
    ${zones.length ? `<div class="mini">${zones.map(zone => `<span class="zone-chip">${iconBadge(zone.icon)}<strong>${esc(zone.name)}</strong><span class="muted">${esc(zone.text)}</span></span>`).join('')}</div>` : ''}
  </div>`;
}
function eventPlan(event) {
  return `<div class="event-chip">${iconBadge(event.icon)}<strong>${esc(event.time)}</strong><span>${esc(event.text)}</span></div>`;
}
function iconBadge(code, big = false) {
  const meta = {
    SUN: ['sun', 'Soleggiato'],
    PARTLY: ['partly', 'Variabile o parzialmente nuvoloso'],
    CLOUD: ['cloud', 'Nuvoloso'],
    RAIN: ['rain', 'Pioggia prevista'],
    STORM: ['storm', 'Temporale'],
    FOG: ['fog', 'Nebbia'],
    SNOW: ['snow', 'Neve'],
    DROP: ['drop', 'Irrigazione prevista'],
    SKIP: ['skip', 'Ciclo saltato'],
    OK: ['ok', 'Nessun intervento necessario'],
    BAL: ['balance', 'Bilancio idrico'],
    PCT: ['percent', 'Probabilita di pioggia'],
    ET: ['et', 'Evapotraspirazione stimata'],
    INFO: ['info', 'Informazione'],
    NA: ['info', 'Dato non disponibile']
  }[code] || ['info', code || 'Dato non disponibile'];
  return `<span class="icon-badge ${big ? 'big' : ''}" title="${esc(meta[1])}" aria-label="${esc(meta[1])}">${iconSvg(meta[0])}</span>`;
}
function iconSvg(name) {
  const icons = {
    sun: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"></circle><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"></path></svg>',
    partly: '<svg viewBox="0 0 24 24"><circle cx="8" cy="8" r="3"></circle><path d="M8 1v2M8 13v2M1 8h2M13 8h2M4 4l1.2 1.2M10.8 10.8 12 12"></path><path d="M9 18h9a4 4 0 0 0 0-8 5.5 5.5 0 0 0-10.5 2"></path></svg>',
    cloud: '<svg viewBox="0 0 24 24"><path d="M5 18h13a4 4 0 0 0 0-8 5.5 5.5 0 0 0-10.5 2A3.5 3.5 0 0 0 5 18Z"></path></svg>',
    rain: '<svg viewBox="0 0 24 24"><path d="M5 15h13a4 4 0 0 0 0-8 5.5 5.5 0 0 0-10.5 2A3.5 3.5 0 0 0 5 15Z"></path><path d="M8 19v2M12 18v2M16 19v2"></path></svg>',
    storm: '<svg viewBox="0 0 24 24"><path d="M5 14h13a4 4 0 0 0 0-8 5.5 5.5 0 0 0-10.5 2A3.5 3.5 0 0 0 5 14Z"></path><path d="m13 14-3 5h4l-2 4"></path></svg>',
    fog: '<svg viewBox="0 0 24 24"><path d="M5 13h13a4 4 0 0 0 0-8 5.5 5.5 0 0 0-10.5 2A3.5 3.5 0 0 0 5 13Z"></path><path d="M4 17h16M6 21h12"></path></svg>',
    snow: '<svg viewBox="0 0 24 24"><path d="M12 3v18M5 7l14 10M19 7 5 17M7 3l5 4 5-4M7 21l5-4 5 4"></path></svg>',
    drop: '<svg viewBox="0 0 24 24"><path d="M12 3s6 7 6 12a6 6 0 0 1-12 0c0-5 6-12 6-12Z"></path></svg>',
    skip: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="9"></circle><path d="m8 8 8 8"></path></svg>',
    ok: '<svg viewBox="0 0 24 24"><path d="m5 12 4 4L19 6"></path></svg>',
    balance: '<svg viewBox="0 0 24 24"><path d="M12 3v18M5 7h14M7 7l-4 7h8L7 7ZM17 7l-4 7h8l-4-7Z"></path></svg>',
    percent: '<svg viewBox="0 0 24 24"><path d="m19 5-14 14"></path><circle cx="7" cy="7" r="2"></circle><circle cx="17" cy="17" r="2"></circle></svg>',
    et: '<svg viewBox="0 0 24 24"><path d="M12 3s5 6 5 10a5 5 0 0 1-10 0c0-4 5-10 5-10Z"></path><path d="M8 21h8"></path></svg>',
    info: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="9"></circle><path d="M12 11v6M12 7h.01"></path></svg>'
  };
  return icons[name] || icons.info;
}
function renderDashboard() {
  const runner = overview.runner || {};
  const validation = overview.validation || { errors: [], warnings: [] };
  return `
    ${renderPlanPanel()}
    ${renderWeatherSummaryPanel()}
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
            ${(overview.cycles || []).length ? (overview.cycles || []).map(c => `<tr><td><strong>${esc(c.name)}</strong><div class="muted">${esc(c.id)}</div></td><td>${esc(c.mode)}</td><td>${esc(c.next_run_text)}</td><td><button onclick="startCycle('${esc(c.id)}')">Start</button></td></tr>`).join('') : `<tr><td colspan="4">${emptyState('Nessun ciclo configurato', 'Crea un ciclo dalla pagina Cicli per iniziare a programmare irrigazioni.')}</td></tr>`}
          </tbody></table>
        </div>
        ${renderWeatherReasonCard()}
      </div>
      <div class="card">
        <h3>Zone</h3>
        <table><thead><tr><th>Zona</th><th>Stato</th><th>Deficit</th><th>Calibrazione</th><th></th></tr></thead><tbody>
          ${(overview.zones || []).length ? (overview.zones || []).map(z => `<tr><td><strong>${esc(z.name)}</strong><div class="muted">${esc(z.entity)}</div></td><td><span class="pill ${esc(z.state_class)}">${esc(z.state)}</span></td><td>${num(z.water_balance_mm).toFixed(1)} mm</td><td>${esc(z.calibration_text)}</td><td><button onclick="startZone('${esc(z.id)}')">5 min</button></td></tr>`).join('') : `<tr><td colspan="5">${emptyState('Nessuna zona configurata', 'Aggiungi una zona e associa una valvola Home Assistant.')}</td></tr>`}
        </tbody></table>
      </div>
    </section>`;
}
function renderWeatherSummaryPanel() {
  const weather = overview.weather || {};
  const last = overview.diagnostics?.last_weather;
  return `<section class="section">
    <div class="weather-summary">
      <div class="card weather-kpi">
        <span class="muted">Meteo attuale</span>
        <strong>${esc(formatWeatherState(weather.state))}</strong>
        <span class="muted">${esc(weather.entity || '-')} · forecast ${esc(weather.forecast_type || '-')}</span>
        <div class="mini">
          <span title="Evapotraspirazione stimata">${iconBadge('ET')} ${last ? num(last.et0_mm).toFixed(1) : '-'} mm</span>
          <span title="Pioggia utile stimata">${iconBadge('RAIN')} ${last ? num(last.effective_rain_mm).toFixed(1) : '-'} mm</span>
        </div>
      </div>
      ${forecastCard(decisionPlan?.today, 'Oggi')}
      ${forecastCard(decisionPlan?.tomorrow, 'Domani')}
    </div>
  </section>`;
}
function forecastCard(day, fallbackLabel) {
  if (!day) return `<div class="card forecast-card"><h3>${esc(fallbackLabel)}</h3><p class="muted">Previsione non disponibile</p></div>`;
  const forecastText = day.has_forecast ? `${day.forecast_count} previsioni HA` : 'Fallback: forecast HA non disponibile';
  return `<div class="card forecast-card">
    <div class="forecast-line">${iconBadge(day.icon)}<h3>${esc(day.label || fallbackLabel)}</h3><span class="pill ${esc(day.decision_class)}">${esc(day.decision)}</span></div>
    <strong>${esc(day.weather_label || '-')}</strong>
    <div class="forecast-meta">${iconBadge(day.has_forecast ? 'OK' : 'INFO')} ${esc(forecastText)}</div>
    <div class="mini">
      <span title="Pioggia prevista">${iconBadge('RAIN')} ${num(day.expected_rain_mm).toFixed(1)} mm</span>
      <span title="Probabilita di pioggia">${iconBadge('PCT')} ${num(day.rain_probability)}%</span>
      <span title="Evapotraspirazione">${iconBadge('ET')} ${num(day.et0_mm).toFixed(1)} mm</span>
    </div>
  </div>`;
}
function renderWeatherReasonCard() {
  const decision = overview.diagnostics?.last_decision;
  const last = overview.diagnostics?.last_weather;
  return `<div class="card">
    <h3>Perche questa decisione</h3>
    <p>${esc(decision?.message || 'Nessuna decisione registrata')}</p>
    <p class="muted">${last ? `ET0 ${num(last.et0_mm).toFixed(1)} mm, pioggia prevista ${num(last.expected_rain_mm).toFixed(1)} mm, pioggia utile ${num(last.effective_rain_mm).toFixed(1)} mm, probabilita ${num(last.max_rain_probability)}%` : 'Nessun calcolo meteo registrato'}</p>
  </div>`;
}
function formatWeatherState(state) {
  const states = {
    clear: 'Sereno',
    sunny: 'Soleggiato',
    partlycloudy: 'Parzialmente nuvoloso',
    cloudy: 'Nuvoloso',
    rainy: 'Pioggia',
    pouring: 'Pioggia intensa',
    lightning: 'Temporale',
    snowy: 'Neve',
    fog: 'Nebbia',
    unknown: 'Sconosciuto',
    unavailable: 'Non disponibile'
  };
  return states[String(state || '').toLowerCase()] || state || '-';
}
function renderZones() {
  const zones = Object.entries({ ...draftZones, ...(config.zones || {}) });
  const savedCount = Object.keys(config.zones || {}).length;
  const calibratedCount = (overview.zones || []).filter(zone => zone.calibrated_precipitation_rate_mm_h).length;
  return `<section class="section">
    <div class="toolbar"><h2>Zone</h2><button onclick="addZone()">Nuova zona</button></div>
    <div class="grid summary">
      ${metric('Zone salvate', savedCount)}
      ${metric('In bozza', Object.keys(draftZones).length)}
      ${metric('Calibrate', calibratedCount)}
    </div>
    ${validationPanel('zones')}
    ${entityOptionsList()}
    <div class="list">${zones.length ? zones.map(([id, z]) => zoneForm(id, z)).join('') : emptyState('Nessuna zona', 'Crea una zona per associare una valvola e impostare resa, limiti e calibrazione.')}</div>
  </section>`;
}
function zoneForm(id, z) {
  const ov = (overview.zones || []).find(x => x.id === id);
  const isDraft = Object.prototype.hasOwnProperty.call(draftZones, id);
  const status = isDraft ? '<span class="pill warn">bozza</span>' : '<span class="pill ok">salvata</span>';
  return `<div class="card" id="zone-${esc(id)}">
    <div class="toolbar"><h3>${esc(z.name || id)} ${status}</h3><button class="danger" onclick="${esc(action('deleteZone', id))}">Elimina</button></div>
    <div class="row">
      ${field(`zone-${id}-id`, 'ID', id, 'span-2')}
      ${field(`zone-${id}-name`, 'Nome', z.name, 'span-3')}
      ${entityField(`zone-${id}-entity`, 'Entita valvola/switch', z.entity, 'span-4')}
      ${numberField(`zone-${id}-rate`, 'Resa mm/h', z.precipitation_rate_mm_h, 'span-3', '0.01')}
      ${numberField(`zone-${id}-crop`, 'Coeff. coltura', z.crop_coefficient, 'span-2', '0.01')}
      ${numberField(`zone-${id}-min`, 'Min minuti', z.min_minutes, 'span-2')}
      ${numberField(`zone-${id}-max`, 'Max minuti', z.max_minutes, 'span-2')}
      ${numberField(`zone-${id}-target`, 'Deficit target mm', z.target_deficit_mm, 'span-2', '0.1')}
      ${field(`zone-${id}-soil`, 'Sensore umidita', z.soil_moisture_entity || '', 'span-2')}
      ${numberField(`zone-${id}-soilskip`, 'Skip umidita sopra', z.skip_if_soil_moisture_above ?? '', 'span-2', '0.1')}
    </div>
    <div class="actions" style="margin-top:12px; justify-content:flex-start">
      <button onclick="${esc(action('saveZone', id))}">Salva zona</button>
      <button class="secondary" onclick="${esc(action('calibrateZone', id))}">Calibra</button>
      <button class="secondary" onclick="${esc(action('applyCalibration', id))}">Applica calibrazione</button>
      <button class="blue" onclick="${esc(action('startZone', id))}">Avvia 5 min</button>
      <button class="secondary" onclick="${esc(action('stopZone', id))}">Stop</button>
      <span class="muted">Ultima calibrazione: ${esc(ov?.calibration_text || '-')}</span>
    </div>
  </div>`;
}
function renderCycles() {
  const cycles = Object.entries({ ...draftCycles, ...(config.cycles || {}) });
  const savedCycles = Object.values(config.cycles || {});
  const automaticCount = savedCycles.filter(cycle => cycle.mode === 'Automatic').length;
  const enabledCount = savedCycles.filter(cycle => cycle.enabled !== false).length;
  return `<section class="section">
    <div class="toolbar"><h2>Cicli</h2><button onclick="addCycle()">Nuovo ciclo</button></div>
    <div class="grid summary">
      ${metric('Cicli abilitati', enabledCount)}
      ${metric('Automatici', automaticCount)}
      ${metric('In bozza', Object.keys(draftCycles).length)}
    </div>
    ${validationPanel('cycles')}
    <div class="list">${cycles.length ? cycles.map(([id, c]) => cycleForm(id, c)).join('') : emptyState('Nessun ciclo', 'Crea un ciclo per organizzare gruppi di zone, orari e simulazioni dry-run.')}</div>
  </section>`;
}
function cycleForm(id, c) {
  const schedule = c.schedule || { days: [], times: [] };
  const scheduleMode = schedule.start_date || schedule.every_days ? 'interval' : 'weekly';
  const isDraft = Object.prototype.hasOwnProperty.call(draftCycles, id);
  const status = isDraft ? '<span class="pill warn">bozza</span>' : '<span class="pill ok">salvato</span>';
  return `<div class="card">
    <div class="toolbar"><h3>${esc(c.name || id)} ${status}</h3><button class="danger" onclick="${esc(action('deleteCycle', id))}">Elimina</button></div>
    <div class="row">
      ${field(`cycle-${id}-id`, 'ID', id, 'span-2')}
      ${field(`cycle-${id}-name`, 'Nome', c.name, 'span-3')}
      <label class="span-2"><span>Modo</span><select id="cycle-${esc(id)}-mode"><option ${c.mode === 'Manual' ? 'selected' : ''}>Manual</option><option ${c.mode === 'Automatic' ? 'selected' : ''}>Automatic</option></select></label>
      <label class="span-2"><span>Abilitato</span><select id="cycle-${esc(id)}-enabled"><option value="true" ${c.enabled !== false ? 'selected' : ''}>Si</option><option value="false" ${c.enabled === false ? 'selected' : ''}>No</option></select></label>
      <div class="span-12">
        <div class="toolbar"><h3>Orari partenza</h3><button type="button" onclick="${esc(action('addCycleTime', id))}">Aggiungi orario</button></div>
        <div class="list" id="cycle-${esc(id)}-times-list">${cycleTimesEditor(id, schedule.times || [])}</div>
      </div>
      <label class="span-3"><span>Programmazione</span><select id="cycle-${esc(id)}-schedule-mode"><option value="weekly" ${scheduleMode === 'weekly' ? 'selected' : ''}>Giorni settimana</option><option value="interval" ${scheduleMode === 'interval' ? 'selected' : ''}>Ogni N giorni</option></select></label>
      <div class="span-12">${daysControl(`cycle-${id}-days`, schedule.days || [])}</div>
      ${field(`cycle-${id}-start`, 'Data inizio alternanza', schedule.start_date || '', 'span-3')}
      ${numberField(`cycle-${id}-every`, 'Ogni quanti giorni', schedule.every_days ?? '', 'span-3')}
      <div class="span-12">
        <div class="toolbar"><h3>Step zone</h3><button type="button" onclick="${esc(action('addCycleStep', id))}">Aggiungi step</button></div>
        <div class="list" id="cycle-${esc(id)}-steps-list">${cycleStepsEditor(id, c.steps || [])}</div>
      </div>
    </div>
    <div class="actions" style="margin-top:12px; justify-content:flex-start">
      <button onclick="${esc(action('saveCycle', id))}">Salva ciclo</button>
      <button class="blue" onclick="${esc(action('startCycle', id))}">Avvia</button>
      <button class="secondary" onclick="${esc(action('dryRunCycle', id))}">Simula</button>
    </div>
    ${cycleDecisionPreview(id, c)}
    ${cycleEventRegister(id)}
  </div>`;
}
function cycleDecisionPreview(id, c) {
  if (!decisionPlan) return '';
  const today = cycleDecisionForDay(decisionPlan.today, id, c);
  const tomorrow = cycleDecisionForDay(decisionPlan.tomorrow, id, c);
  if (!today && !tomorrow) {
    return `<div class="cycle-preview">
      <h3>Anteprima decisionale</h3>
      <div class="cycle-preview-day">${emptyState('Nessuna valutazione pianificata', 'I cicli manuali partono solo quando li avvii. Per i cicli automatici imposta giorni/orari o alternanza.')}</div>
    </div>`;
  }
  return `<div class="cycle-preview">
    <h3>Anteprima decisionale</h3>
    <div class="cycle-preview-grid">
      ${cycleDecisionTile('Oggi', today)}
      ${cycleDecisionTile('Domani', tomorrow)}
    </div>
  </div>`;
}
function cycleDecisionForDay(day, id, c) {
  return (day?.cycles || []).find(item => item.id === id || item.name === c.name || item.name === id);
}
function cycleDecisionTile(label, item) {
  if (!item) return `<div class="cycle-preview-day"><div class="forecast-line">${iconBadge('NA')}<strong>${esc(label)}</strong></div><span class="muted">Nessuna partenza prevista</span></div>`;
  const zones = item.zones || [];
  return `<div class="cycle-preview-day">
    <div class="forecast-line">${iconBadge(item.icon)}<strong>${esc(label)} ${esc(item.time || '')}</strong><span class="pill ${esc(item.decision_class)}">${esc(item.decision || '-')}</span></div>
    ${zones.length ? `<div class="mini">${zones.map(zone => `<span class="zone-chip">${iconBadge(zone.icon)}<strong>${esc(zone.name)}</strong><span class="muted">${esc(zone.text)}</span></span>`).join('')}</div>` : '<span class="muted">Nessuna zona da irrigare</span>'}
  </div>`;
}
function cycleStepsEditor(id, steps) {
  const normalized = steps.length ? steps : [{ zones: [], duration_seconds: 600, duration_minutes: 10 }];
  return normalized.map(step => cycleStepRow(id, (step.zones || [])[0] || '', stepDurationText(step))).join('');
}
function cycleTimesEditor(id, times) {
  const normalized = times.length ? times : ['06:00'];
  return normalized.map(time => cycleTimeRow(id, time)).join('');
}
function cycleTimeRow(id, time = '06:00') {
  return `<div class="time-row">
    <label><span>Ora start</span><input class="cycle-time" type="time" value="${esc(time)}"></label>
    <span class="muted">Formato 24h</span>
    <button type="button" class="danger" onclick="removeCycleTime(this)">Rimuovi</button>
  </div>`;
}
function cycleStepRow(id, zoneId = '', duration = '00:10:00') {
  return `<div class="step-row">
    <label><span>Zona</span>${zoneSelect(zoneId)}</label>
    <label><span>Tempo hh:mm:ss</span><input class="cycle-step-duration" value="${esc(duration)}" placeholder="00:10:00"></label>
    <button type="button" class="danger" onclick="removeCycleStep(this)">Rimuovi</button>
  </div>`;
}
function zoneSelect(value = '') {
  const options = Object.entries(config.zones || {}).map(([id, zone]) =>
    `<option value="${esc(id)}" ${id === value ? 'selected' : ''}>${esc(zone.name || id)}</option>`
  ).join('');
  return `<select class="cycle-step-zone">${options}</select>`;
}
function stepDurationText(step) {
  const seconds = step.duration_seconds ?? ((step.duration_minutes ?? 10) * 60);
  return formatDurationInput(seconds);
}
function cycleEventRegister(id) {
  const events = (overview.recent_events || []).filter(event => event.cycle_id === id).slice(0, 8);
  if (!events.length) return `<div style="margin-top:12px">${emptyState('Nessun evento per questo ciclo', 'Avvia o simula il ciclo per popolare il registro dedicato.')}</div>`;
  return `<div style="margin-top:12px">
    <h3>Registro ciclo</h3>
    <table><thead><tr><th>Quando</th><th>Tipo</th><th>Zona</th><th>Messaggio</th></tr></thead><tbody>
      ${events.map(e => `<tr><td>${esc(new Date(e.timestamp).toLocaleString())}</td><td><span class="pill">${esc(eventLabel(e.type))}</span></td><td>${esc(e.zone_id || '-')}</td><td>${esc(e.message)}</td></tr>`).join('')}
    </tbody></table>
  </div>`;
}
function eventLabel(type) {
  const labels = {
    cycle_started: 'Avvio',
    cycle_completed: 'Fine',
    cycle_skipped: 'Saltato',
    zone_started: 'Zona on',
    zone_completed: 'Zona off',
    zone_skipped: 'Zona skip',
    dry_run_started: 'Sim start',
    dry_run_completed: 'Sim fine',
    dry_run_zone_planned: 'Sim zona',
    dry_run_zone_skipped: 'Sim skip',
    dry_run_master_valve: 'Sim master',
    master_valve_started: 'Master on',
    master_valve_stopped: 'Master off',
    cycle_saved: 'Salvato',
    cycle_deleted: 'Eliminato'
  };
  return labels[type] || type || '-';
}
function renderWeather() {
  const w = config.weather || {};
  return `<section class="section">
  <div class="card notice"><strong>Configurazione Home Assistant</strong><p class="muted">In uso reale queste opzioni generali sono pensate per la scheda Config dell'add-on. Questa vista resta utile per sviluppo e modifiche avanzate.</p></div>
  ${validationPanel('weather')}
  ${weatherSettingsOverview(w)}
  <div class="card"><div class="row">
    ${weatherEntityOptionsList()}
    ${weatherEntityField('weather-entity', 'Entita meteo', w.entity, 'span-4')}
    ${field('weather-type', 'Tipo forecast', w.forecast_type || 'hourly', 'span-2')}
    ${numberField('weather-lookahead', 'Ore previsione', w.rain_lookahead_hours, 'span-2')}
    ${numberField('weather-efficiency', 'Efficienza pioggia', w.rain_efficiency, 'span-2', '0.01')}
    ${numberField('weather-skipmm', 'Skip sopra mm', w.skip_if_expected_rain_mm_above, 'span-2', '0.1')}
    ${numberField('weather-skipprob', 'Skip probabilita %', w.skip_if_rain_probability_above, 'span-2')}
    ${field('weather-et0', 'Sensore ET0 opzionale', w.external_et0_sensor_entity || '', 'span-4')}
  </div><div class="actions" style="margin-top:12px; justify-content:flex-start"><button onclick="saveWeather()">Salva meteo</button></div></div></section>`;
}
function renderPlant() {
  const h = config.hydraulic || {};
  const s = config.safety || {};
  const m = config.mqtt_discovery || {};
  return `<section class="section">
    <div class="card notice"><strong>Configurazione Home Assistant</strong><p class="muted">Idraulica, sicurezze e MQTT Discovery possono essere gestite dalla scheda Config dell'add-on. Il collegamento laterale usa Ingress con panel_title/panel_icon.</p></div>
    ${validationPanel('hydraulic')}
    ${validationPanel('safety')}
    ${validationPanel('mqtt_discovery')}
    ${plantFlow(h, s)}
    <div class="card"><h3>Idraulica</h3><div class="row">
      ${entityOptionsList()}
      ${entityField('hyd-master', 'Valvola master', h.master_valve_entity || '', 'span-4')}
      <label class="span-3"><span>Zone parallele</span><select id="hyd-parallel"><option value="false" ${!h.allow_parallel_zones ? 'selected' : ''}>No</option><option value="true" ${h.allow_parallel_zones ? 'selected' : ''}>Si</option></select></label>
      ${numberField('hyd-max', 'Max zone insieme', h.max_parallel_zones, 'span-3')}
      ${numberField('hyd-pause', 'Pausa tra zone sec.', h.pause_between_zones_seconds, 'span-3')}
    </div></div>
    <div class="card"><h3>Sicurezze</h3><div class="row">
      ${selectBool('safe-startup', "Spegni all'avvio", s.turn_off_all_zones_on_startup, 'span-3')}
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
function weatherSettingsOverview(w) {
  const weather = overview.weather || {};
  const last = overview.diagnostics?.last_weather;
  return `<div class="card">
    <div class="toolbar"><h3>Meteo operativo</h3><span class="pill">${esc(formatWeatherState(weather.state))}</span></div>
    <div class="setting-board" style="margin-top:10px">
      <div class="setting-tile"><span>Entita HA</span><strong>${esc(w.entity || weather.entity || '-')}</strong></div>
      <div class="setting-tile"><span>Forecast</span><strong>${esc(w.forecast_type || weather.forecast_type || 'hourly')}</strong></div>
      <div class="setting-tile"><span>Finestra pioggia</span><strong>${num(w.rain_lookahead_hours, 24)} h</strong></div>
      <div class="setting-tile"><span>Efficienza pioggia</span><strong>${Math.round(num(w.rain_efficiency, .75) * 100)}%</strong></div>
      <div class="setting-tile"><span>Skip pioggia</span><strong>${num(w.skip_if_expected_rain_mm_above, 4).toFixed(1)} mm</strong></div>
      <div class="setting-tile"><span>Skip probabilita</span><strong>${num(w.skip_if_rain_probability_above, 70)}%</strong></div>
      <div class="setting-tile"><span>Ultima ET0</span><strong>${last ? num(last.et0_mm).toFixed(1) + ' mm' : '-'}</strong></div>
      <div class="setting-tile"><span>Pioggia utile</span><strong>${last ? num(last.effective_rain_mm).toFixed(1) + ' mm' : '-'}</strong></div>
    </div>
    ${weatherDiagnosticsPanel(last)}
  </div>`;
}
function weatherDiagnosticsPanel(last) {
  if (!last) return `<div class="empty" style="margin-top:10px"><strong>Dati ricevuti da Home Assistant</strong><span>Nessuna chiamata meteo registrata. Avvia un dry-run o attendi l'aggiornamento automatico del bilancio.</span></div>`;
  const first = last.first_forecast_at ? new Date(last.first_forecast_at).toLocaleString() : '-';
  const lastAt = last.last_forecast_at ? new Date(last.last_forecast_at).toLocaleString() : '-';
  return `<div style="margin-top:12px">
    <div class="toolbar"><h3>Dati ricevuti da Home Assistant</h3><span class="pill ${last.forecast_available ? 'ok' : 'warn'}">${last.forecast_available ? 'Forecast presente' : 'Forecast assente'}</span></div>
    <div class="setting-board" style="margin-top:10px">
      <div class="setting-tile"><span>Entita chiamata</span><strong>${esc(last.entity || '-')}</strong></div>
      <div class="setting-tile"><span>Tipo forecast</span><strong>${esc(last.forecast_type || '-')}</strong></div>
      <div class="setting-tile"><span>Record nella finestra</span><strong>${num(last.forecast_records, 0)}</strong></div>
      <div class="setting-tile"><span>Esito</span><strong>${esc(last.message || '-')}</strong></div>
      <div class="setting-tile span-6"><span>Prima previsione</span><strong>${esc(first)}</strong></div>
      <div class="setting-tile span-6"><span>Ultima previsione</span><strong>${esc(lastAt)}</strong></div>
    </div>
  </div>`;
}
function plantFlow(h, s) {
  const zones = Object.entries(config.zones || {});
  const master = h.master_valve_entity || 'Nessuna valvola master configurata';
  return `<div class="card">
    <div class="toolbar"><h3>Schema impianto</h3><span class="pill ${h.master_valve_entity ? 'ok' : 'warn'}">${h.master_valve_entity ? 'Master attiva' : 'Master assente'}</span></div>
    <div class="plant-flow" style="margin-top:10px">
      <div class="flow-node">
        <div class="forecast-line">${iconBadge('OK')}<strong>Monte impianto</strong></div>
        <span class="muted">${esc(master)}</span>
        <span>${h.allow_parallel_zones ? `Fino a ${num(h.max_parallel_zones, 1)} zone insieme` : 'Una zona alla volta'}</span>
        <span class="muted">Pausa tra zone: ${num(h.pause_between_zones_seconds, 0)} sec.</span>
      </div>
      <div class="flow-node">
        <div class="forecast-line">${iconBadge('DROP')}<strong>Zone collegate</strong><span class="pill">${zones.length}</span></div>
        <div class="flow-zones">
          ${zones.length ? zones.map(([id, zone]) => `<span class="zone-chip">${iconBadge('DROP')}<strong>${esc(zone.name || id)}</strong><span class="muted">${esc(zone.entity || '-')}</span></span>`).join('') : emptyState('Nessuna zona', 'Aggiungi una zona per vedere qui il ramo idraulico.')}
        </div>
      </div>
    </div>
    <div class="mini muted" style="margin-top:10px">
      <span>${s.turn_off_all_zones_on_startup ? "Spegnimento valvole all'avvio attivo" : "Spegnimento valvole all'avvio disattivo"}</span>
      <span>${s.stop_all_known_zones_on_error ? 'Stop su errore attivo' : 'Stop su errore disattivo'}</span>
      <span>${s.verify_zone_state_after_switch ? 'Verifica stato valvole attiva' : 'Verifica stato valvole disattiva'}</span>
    </div>
  </div>`;
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
    <div class="card"><h3>Eventi recenti</h3><table><thead><tr><th>Quando</th><th>Tipo</th><th>Ciclo</th><th>Zona</th><th>Messaggio</th></tr></thead><tbody>${events.map(e => `<tr><td>${esc(new Date(e.timestamp).toLocaleString())}</td><td>${esc(e.type)}</td><td>${esc(e.cycle_id || '-')}</td><td>${esc(e.zone_id || '-')}</td><td>${esc(e.message)}</td></tr>`).join('')}</tbody></table></div>
  </section>`;
}
function renderRaw() {
  return `<section class="section"><div class="card">
    ${validationPanel()}
    <textarea id="raw-json" style="min-height:65vh">${esc(JSON.stringify(config, null, 2))}</textarea>
    <div class="actions" style="margin-top:12px; justify-content:flex-start"><button onclick="saveRaw()">Salva JSON</button></div>
  </div></section>`;
}
function metric(label, value) { return `<div class="card metric"><span class="muted">${esc(label)}</span><strong>${esc(value)}</strong></div>`; }
function emptyState(title, text) { return `<div class="empty"><strong>${esc(title)}</strong><span>${esc(text)}</span></div>`; }
function validationCard(validation) {
  const errors = validation.errors || [];
  const warnings = validation.warnings || [];
  const issues = [...errors.map(x => ['danger', x]), ...warnings.map(x => ['warn', x])];
  if (!issues.length) return '<div class="card notice"><strong>Configurazione valida</strong></div>';
  return `<div class="card notice ${errors.length ? 'danger' : 'warn'}"><strong>Configurazione da verificare</strong><ul>${issues.map(([cls, x]) => `<li class="${cls}"><strong>${esc(x.path)}</strong> ${esc(x.message)}</li>`).join('')}</ul></div>`;
}
function validationIssues(scope = '') {
  if (!lastValidation) return [];
  const errors = (lastValidation.errors || []).map(x => ({ ...x, level: 'danger' }));
  const warnings = (lastValidation.warnings || []).map(x => ({ ...x, level: 'warn' }));
  return [...errors, ...warnings].filter(issue => !scope || String(issue.path || '').startsWith(scope));
}
function validationPanel(scope = '') {
  const issues = validationIssues(scope);
  if (!issues.length) return '';
  return `<div class="card notice ${issues.some(x => x.level === 'danger') ? 'danger' : 'warn'} validation-panel">
    <strong>Salvataggio non riuscito</strong>
    <span class="muted">Correggi questi punti e salva di nuovo.</span>
    <ul>${issues.map(issue => `<li class="${esc(issue.level)}"><strong>${esc(issue.path || 'config')}</strong> ${esc(issue.message || '')}</li>`).join('')}</ul>
  </div>`;
}
function field(id, label, value, cls = '') { return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" value="${esc(value ?? '')}"></label>`; }
function numberField(id, label, value, cls = '', step = '1') { return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" type="number" step="${esc(step)}" value="${esc(value ?? '')}"></label>`; }
function entityField(id, label, value, cls = '') {
  return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" list="irrigation-entities" value="${esc(value ?? '')}"></label>`;
}
function entityOptionsList() {
  const options = irrigationEntities.map(entity => {
    const text = entity.friendly_name ? `${entity.entity_id} - ${entity.friendly_name}` : entity.entity_id;
    return `<option value="${esc(entity.entity_id)}">${esc(text)}</option>`;
  }).join('');
  return `<datalist id="irrigation-entities">${options}</datalist>`;
}
function weatherEntityField(id, label, value, cls = '') {
  return `<label class="${cls}"><span>${esc(label)}</span><input id="${esc(id)}" list="weather-entities" value="${esc(value ?? '')}"></label>`;
}
function weatherEntityOptionsList() {
  const options = weatherEntities.map(entity => {
    const text = entity.friendly_name ? `${entity.entity_id} - ${entity.friendly_name}` : entity.entity_id;
    return `<option value="${esc(entity.entity_id)}">${esc(text)}</option>`;
  }).join('');
  return `<datalist id="weather-entities">${options}</datalist>`;
}
function selectBool(id, label, value, cls = '') { return `<label class="${cls}"><span>${esc(label)}</span><select id="${esc(id)}"><option value="true" ${value ? 'selected' : ''}>Si</option><option value="false" ${!value ? 'selected' : ''}>No</option></select></label>`; }
function daysControl(id, values) {
  return `<div class="days">${dayNames.map((d, i) => `<label><input type="checkbox" data-days="${esc(id)}" value="${d}" ${values.includes(d) ? 'checked' : ''}>${dayLabels[i]}</label>`).join('')}</div>`;
}
function getDays(id) { return [...document.querySelectorAll(`[data-days="${id}"]:checked`)].map(x => x.value); }
function formatDurationInput(seconds) {
  const total = Math.max(0, Number(seconds) || 0);
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = Math.floor(total % 60);
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}
function parseDurationInput(value) {
  const parts = String(value || '').trim().split(':').map(x => Number(x));
  if (parts.length !== 3 || parts.some(x => !Number.isInteger(x) || x < 0)) return null;
  const [hours, minutes, seconds] = parts;
  if (minutes > 59 || seconds > 59) return null;
  const total = hours * 3600 + minutes * 60 + seconds;
  return total > 0 ? total : null;
}
function addCycleStep(id) {
  const list = document.getElementById(`cycle-${id}-steps-list`);
  if (!list) return;
  list.insertAdjacentHTML('beforeend', cycleStepRow(id, Object.keys(config.zones || {})[0] || '', '00:10:00'));
}
function addCycleTime(id) {
  const list = document.getElementById(`cycle-${id}-times-list`);
  if (!list) return;
  list.insertAdjacentHTML('beforeend', cycleTimeRow(id, '06:00'));
}
function removeCycleTime(button) {
  button.closest('.time-row')?.remove();
}
function removeCycleStep(button) {
  button.closest('.step-row')?.remove();
}
function collectCycleTimes(id) {
  const list = document.getElementById(`cycle-${id}-times-list`);
  if (!list) return [];
  return [...list.querySelectorAll('.cycle-time')]
    .map(input => input.value)
    .filter(Boolean);
}
function collectCycleSteps(id) {
  const list = document.getElementById(`cycle-${id}-steps-list`);
  if (!list) return [];
  return [...list.querySelectorAll('.step-row')].map(row => {
    const zoneId = row.querySelector('.cycle-step-zone')?.value || '';
    const seconds = parseDurationInput(row.querySelector('.cycle-step-duration')?.value || '');
    if (!zoneId || seconds === null) return null;
    return { zones: [zoneId], duration_seconds: seconds, duration_minutes: Math.ceil(seconds / 60) };
  }).filter(Boolean);
}
async function startCycle(id) { try { toast((await api('/api/cycles/' + id + '/start', { method: 'POST' })).message); } catch(e) { toast(e.message || 'Errore avvio ciclo', true); } }
async function dryRunCycle(id) { try { toast((await api('/api/cycles/' + id + '/dry-run', { method: 'POST' })).message); await reloadAll(); } catch(e) { toast(e.message || 'Errore simulazione ciclo', true); } }
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
  next.zones ||= {};
  const newId = normalizeId(val(`zone-${id}-id`));
  if (!newId) return toast('ID zona obbligatorio', true);
  if (newId !== id && next.zones[newId]) return toast('Esiste gia una zona con questo ID', true);
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
  renameZoneInCycles(next, id, newId);
  if (await saveConfig(next, 'Zona salvata', false, 'zone_saved', newId)) {
    delete draftZones[id];
    delete draftZones[newId];
    render();
  }
}
async function addZone() {
  const id = normalizeId(prompt('ID nuova zona, es. prato_nord'));
  if (!id) return;
  if ((config.zones || {})[id] || draftZones[id]) return toast('Esiste gia una zona con questo ID', true);
  const firstEntity = irrigationEntities[0]?.entity_id || 'switch.';
  draftZones[id] = { name: id, entity: firstEntity, precipitation_rate_mm_h: 10, crop_coefficient: 1, min_minutes: 3, max_minutes: 30, target_deficit_mm: 0, soil_moisture_entity: null, skip_if_soil_moisture_above: null };
  setPage('zones');
  render();
}
async function deleteZone(id) {
  if (draftZones[id]) {
    delete draftZones[id];
    render();
    return;
  }
  if (!confirm('Eliminare la zona ' + id + '?')) return;
  const next = cloneConfig();
  delete next.zones[id];
  removeZoneFromCycles(next, id);
  await saveConfig(next, 'Zona eliminata', true, 'zone_deleted', id);
}
function removeZoneFromCycles(next, zoneId) {
  for (const [cycleId, cycle] of Object.entries(next.cycles || {})) {
    cycle.steps = (cycle.steps || [])
      .map(step => ({ ...step, zones: (step.zones || []).filter(id => id !== zoneId) }))
      .filter(step => (step.zones || []).length > 0);
    if (cycle.steps.length === 0) delete next.cycles[cycleId];
  }
}
function renameZoneInCycles(next, oldZoneId, newZoneId) {
  if (oldZoneId === newZoneId) return;
  for (const cycle of Object.values(next.cycles || {})) {
    cycle.steps = (cycle.steps || []).map(step => ({
      ...step,
      zones: (step.zones || []).map(id => id === oldZoneId ? newZoneId : id)
    }));
  }
}
async function saveCycle(id) {
  const next = cloneConfig();
  next.cycles ||= {};
  const newId = normalizeId(val(`cycle-${id}-id`));
  if (!newId) return toast('ID ciclo obbligatorio', true);
  if (newId !== id && next.cycles[newId]) return toast('Esiste gia un ciclo con questo ID', true);
  const mode = val(`cycle-${id}-mode`);
  const times = collectCycleTimes(id);
  const scheduleMode = val(`cycle-${id}-schedule-mode`);
  const schedule = mode === 'Automatic'
    ? {
        days: scheduleMode === 'weekly' ? getDays(`cycle-${id}-days`) : [],
        times,
        start_date: scheduleMode === 'interval' ? val(`cycle-${id}-start`) || null : null,
        every_days: scheduleMode === 'interval' ? num(val(`cycle-${id}-every`), 1) : null
      }
    : null;
  if (mode === 'Automatic' && times.length === 0) return toast('Aggiungi almeno un orario di partenza valido', true);
  const steps = collectCycleSteps(id);
  if (steps.length === 0) return toast('Aggiungi almeno uno step con zona e tempo hh:mm:ss valido', true);
  const c = {
    name: val(`cycle-${id}-name`),
    enabled: val(`cycle-${id}-enabled`) === 'true',
    mode,
    schedule,
    steps
  };
  delete next.cycles[id];
  next.cycles[newId] = c;
  if (await saveConfig(next, 'Ciclo salvato', false, 'cycle_saved', '', newId)) {
    delete draftCycles[id];
    delete draftCycles[newId];
    render();
  }
}
async function addCycle() {
  const id = normalizeId(prompt('ID nuovo ciclo, es. mattina_prato'));
  if (!id) return;
  const firstZone = Object.keys(config.zones || {})[0] || '';
  if (!firstZone) {
    toast('Crea e salva almeno una zona prima di aggiungere un ciclo', true);
    setPage('zones');
    return;
  }
  if ((config.cycles || {})[id] || draftCycles[id]) return toast('Esiste gia un ciclo con questo ID', true);
  draftCycles[id] = { name: id, enabled: true, mode: 'Manual', schedule: null, steps: [{ zones: [firstZone], duration_seconds: 600, duration_minutes: 10 }] };
  setPage('cycles');
  render();
}
async function deleteCycle(id) {
  if (draftCycles[id]) {
    delete draftCycles[id];
    render();
    return;
  }
  if (!confirm('Eliminare il ciclo ' + id + '?')) return;
  const next = cloneConfig();
  delete next.cycles[id];
  await saveConfig(next, 'Ciclo eliminato', true, 'cycle_deleted', '', id);
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
  await saveConfig(next, 'Meteo salvato', true, 'weather_saved');
}
async function savePlant() {
  const next = cloneConfig();
  next.hydraulic = { allow_parallel_zones: val('hyd-parallel') === 'true', max_parallel_zones: num(val('hyd-max'), 1), pause_between_zones_seconds: num(val('hyd-pause'), 0), master_valve_entity: val('hyd-master') || null };
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
  await saveConfig(next, 'Impianto salvato', true, 'plant_saved');
}
async function saveRaw() {
  try { await saveConfig(JSON.parse(val('raw-json')), 'JSON salvato', true, 'raw_json_saved'); }
  catch (e) { toast('JSON non valido: ' + e.message, true); }
}
function renderValidationError(error) {
  if (error?.errors?.length || error?.warnings?.length) render();
}
reloadAll().catch(error => {
  document.getElementById('content').innerHTML = `<div class="card notice danger"><strong>Errore caricamento</strong><p>${esc(error.message || JSON.stringify(error))}</p></div>`;
});
</script>
</body>
</html>
""";
}

