/**
 * utils.js — Utilidades genéricas reutilizables en todo el frontend.
 */

// ── Fechas ────────────────────────────────────────────────────────────────────

/** Formatea una fecha ISO a "DD/MM/YYYY" */
export function formatDate(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d)) return '—';
  return d.toLocaleDateString('es-NI', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

/** Formatea una fecha ISO a "DD/MM/YYYY HH:mm" */
export function formatDateTime(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d)) return '—';
  return d.toLocaleString('es-NI', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

/** Devuelve "YYYY-MM" para el mes actual */
export function currentPeriod() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}

// ── Números / Moneda ──────────────────────────────────────────────────────────

/** Formatea un número como moneda en Córdobas (C$) */
export function formatCurrency(value) {
  if (value === null || value === undefined) return 'C$ 0.00';
  return 'C$ ' + Number(value).toLocaleString('es-NI', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

// ── DOM ───────────────────────────────────────────────────────────────────────

/** Shortcut para document.querySelector */
export const $ = (selector, ctx = document) => ctx.querySelector(selector);

/** Shortcut para document.querySelectorAll */
export const $$ = (selector, ctx = document) => [...ctx.querySelectorAll(selector)];

/** Muestra un elemento (quita la clase hidden) */
export function show(el) {
  if (el) el.classList.remove('hidden');
}

/** Oculta un elemento (agrega la clase hidden) */
export function hide(el) {
  if (el) el.classList.add('hidden');
}

/** Alterna visibilidad */
export function toggle(el) {
  if (el) el.classList.toggle('hidden');
}

/** Devuelve el badge HTML para un estado (Pendiente / Aprobado / Rechazado) */
export function badgeEstado(estado) {
  const map = {
    'Aprobado':   'badge-green',
    'Pendiente':  'badge-yellow',
    'Rechazado':  'badge-red',
    'A Tiempo':   'badge-green',
    'Tardanza':   'badge-yellow',
    'Ausente':    'badge-red',
    'Admin':      'badge-blue',
    'Empleado':   'badge-gray',
  };
  const cls = map[estado] ?? 'badge-gray';
  return `<span class="badge ${cls}">${estado}</span>`;
}

// ── Toast ─────────────────────────────────────────────────────────────────────

let _toastContainer = null;

function getToastContainer() {
  if (!_toastContainer) {
    _toastContainer = document.getElementById('toast-container');
    if (!_toastContainer) {
      _toastContainer = document.createElement('div');
      _toastContainer.id = 'toast-container';
      document.body.appendChild(_toastContainer);
    }
  }
  return _toastContainer;
}

/**
 * Muestra un toast no bloqueante.
 * @param {string} message
 * @param {'success'|'error'|'warning'|'info'} type
 * @param {number} duration  ms antes de desaparecer
 */
export function toast(message, type = 'info', duration = 3500) {
  const icons = {
    success: '✅',
    error:   '❌',
    warning: '⚠️',
    info:    'ℹ️',
  };

  const container = getToastContainer();
  const el = document.createElement('div');
  el.className = `toast ${type}`;
  el.innerHTML = `<span>${icons[type] ?? 'ℹ️'}</span><span>${message}</span>`;
  container.appendChild(el);

  setTimeout(() => {
    el.style.animation = 'slideOut .25s ease forwards';
    el.addEventListener('animationend', () => el.remove(), { once: true });
  }, duration);
}

// ── Debounce ──────────────────────────────────────────────────────────────────

/** Devuelve una versión debounced de la función fn */
export function debounce(fn, ms) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => fn(...args), ms);
  };
}

// ── Validaciones ──────────────────────────────────────────────────────────────

export function isValidEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

export function isValidPassword(pass) {
  // Mínimo 8 chars, una mayúscula, una minúscula, un número
  return /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(pass);
}

// ── Carga de página ───────────────────────────────────────────────────────────

export function showPageLoader() {
  let el = document.getElementById('page-loader');
  if (!el) {
    el = document.createElement('div');
    el.id = 'page-loader';
    el.className = 'page-loader';
    el.innerHTML = '<div class="big-spinner"></div>';
    document.body.appendChild(el);
  }
  el.classList.remove('hidden');
}

export function hidePageLoader() {
  const el = document.getElementById('page-loader');
  if (el) el.classList.add('hidden');
}
