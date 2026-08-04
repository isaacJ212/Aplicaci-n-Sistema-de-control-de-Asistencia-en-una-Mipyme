/**
 * auth.js — Gestión de sesión: guardar/leer tokens, usuario, logout y guardia de rutas.
 *
 * IMPORTANTE sobre redirecciones:
 *   Las rutas de navegación se resuelven con window.location.href relativo
 *   a la raíz del origen actual (origin + path). Para que funcionen en
 *   cualquier servidor (puerto 3000, 5500, Live Preview, etc.) usamos
 *   rutas relativas a la raíz del servidor con getBasePath().
 */

import CONFIG from './config.js';

/** Devuelve el path base hasta la carpeta frontend/ */
function getBase() {
  // window.location.pathname ejemplo: /frontend/pages/auth/login.html
  // Buscamos el segmento "frontend" y cortamos hasta ahí
  const path = window.location.pathname;
  const idx  = path.indexOf('/frontend');
  if (idx !== -1) return path.slice(0, idx + '/frontend'.length);
  // Si el servidor sirve desde dentro de frontend/ el path empieza en /
  // En ese caso no hay prefijo /frontend
  return '';
}

export const AuthService = {

  // ── Persistencia ──────────────────────────────────────────────────────────

  saveSession(loginResponse) {
    localStorage.setItem(CONFIG.STORAGE.TOKEN,         loginResponse.token);
    localStorage.setItem(CONFIG.STORAGE.REFRESH_TOKEN, loginResponse.refreshToken);
    localStorage.setItem(CONFIG.STORAGE.EXPIRATION,    loginResponse.expiration);
    localStorage.setItem(CONFIG.STORAGE.USER, JSON.stringify({
      email: loginResponse.email,
      role:  loginResponse.role,
    }));
  },

  clearSession() {
    Object.values(CONFIG.STORAGE).forEach(k => localStorage.removeItem(k));
  },

  getToken()        { return localStorage.getItem(CONFIG.STORAGE.TOKEN); },
  getRefreshToken() { return localStorage.getItem(CONFIG.STORAGE.REFRESH_TOKEN); },

  getUser() {
    try { return JSON.parse(localStorage.getItem(CONFIG.STORAGE.USER) ?? 'null'); }
    catch { return null; }
  },

  getRole()         { return this.getUser()?.role ?? null; },
  isAuthenticated() { return !!this.getToken(); },
  isAdmin()         { return this.getRole() === CONFIG.ROLES.ADMIN; },

  // ── Refresco automático de token ──────────────────────────────────────────

  async tryRefresh() {
    const rt = this.getRefreshToken();
    if (!rt) return false;
    try {
      const res = await fetch(`${CONFIG.API_BASE_URL}/auth/refresh`, {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify({ refreshToken: rt }),
      });
      if (!res.ok) return false;
      const body = await res.json();
      if (body?.success && body.data) { this.saveSession(body.data); return true; }
      return false;
    } catch { return false; }
  },

  // ── Logout ────────────────────────────────────────────────────────────────

  async logout() {
    const rt = this.getRefreshToken();
    if (rt) {
      try {
        await fetch(`${CONFIG.API_BASE_URL}/auth/logout`, {
          method:  'POST',
          headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${this.getToken()}` },
          body:    JSON.stringify({ refreshToken: rt }),
        });
      } catch { /* silencioso */ }
    }
    this.clearSession();
    // Redirige a login usando ruta relativa al origen actual
    const base = getBase();
    window.location.href = `${base}/pages/auth/login.html`;
  },

  // ── Guardias de ruta ──────────────────────────────────────────────────────

  requireAuth(requiredRole = null) {
    if (!this.isAuthenticated()) {
      const base = getBase();
      window.location.href = `${base}/pages/auth/login.html`;
      return false;
    }
    if (requiredRole && this.getRole() !== requiredRole) {
      const base = getBase();
      const dest = CONFIG.REDIRECT_AFTER_LOGIN[this.getRole()];
      // dest es relativo a login.html (../../pages/...) pero desde aquí usamos base
      const role = this.getRole();
      if (role === CONFIG.ROLES.ADMIN)
        window.location.href = `${base}/pages/admin/dashboard.html`;
      else
        window.location.href = `${base}/pages/empleado/dashboard.html`;
      return false;
    }
    return true;
  },

  redirectIfAuthenticated() {
    if (!this.isAuthenticated()) return;
    const base = getBase();
    const role = this.getRole();
    if (role === CONFIG.ROLES.ADMIN)
      window.location.href = `${base}/pages/admin/dashboard.html`;
    else
      window.location.href = `${base}/pages/empleado/dashboard.html`;
  },
};
