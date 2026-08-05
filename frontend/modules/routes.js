/**
 * routes.js — Resolución de URLs del frontend.
 *
 * Soporta dos modos de despliegue:
 *   1. Servidor Express (rutas cortas): /admin/dashboard, /empleado/marcaje, /login
 *   2. Live Server / estático: /frontend/pages/admin/dashboard.html
 */

/** True si la URL actual incluye el prefijo /frontend */
export function usesFrontendPrefix() {
  return window.location.pathname.includes('/frontend');
}

/** Origin + path hasta /frontend, o solo origin con rutas cortas */
export function getBase() {
  const path = window.location.pathname;
  const idx  = path.indexOf('/frontend');
  return idx !== -1
    ? window.location.origin + path.slice(0, idx + '/frontend'.length)
    : window.location.origin;
}

/**
 * Convierte una ruta relativa a frontend/ en URL navegable.
 * Ej: "pages/admin/dashboard.html" → "/admin/dashboard" o "/frontend/pages/admin/dashboard.html"
 */
export function resolvePageUrl(relativePath) {
  const base = getBase();

  if (usesFrontendPrefix()) {
    return `${base}/${relativePath}`;
  }

  const match = relativePath.match(/^pages\/(admin|empleado|auth)\/(.+)\.html$/);
  if (!match) return `${base}/${relativePath}`;

  const [, section, page] = match;
  if (section === 'auth' && page === 'login') return `${base}/login`;
  return `${base}/${section}/${page}`;
}

export function getLoginUrl() {
  return resolvePageUrl('pages/auth/login.html');
}

export function getDashboardUrl(role) {
  return role === 'Admin'
    ? resolvePageUrl('pages/admin/dashboard.html')
    : resolvePageUrl('pages/empleado/dashboard.html');
}