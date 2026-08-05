/**
 * auth.js — Gestión de sesión y guardia de rutas.
 *
 * Todas las redirecciones usan getBase() para construir URLs absolutas
 * que funcionan con cualquier servidor/puerto (3000, 5500, etc.)
 */

import CONFIG from './config.js';
import { getLoginUrl, getDashboardUrl } from './routes.js';

export const AuthService = {

  // ── Persistencia ─────────────────────────────────────────────────────────

  saveSession(data) {
    localStorage.setItem(CONFIG.STORAGE.TOKEN,         data.token);
    localStorage.setItem(CONFIG.STORAGE.REFRESH_TOKEN, data.refreshToken);
    localStorage.setItem(CONFIG.STORAGE.EXPIRATION,    data.expiration);
    localStorage.setItem(CONFIG.STORAGE.USER, JSON.stringify({
      email: data.email,
      role:  data.role,
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

  // ── Refresco del token ────────────────────────────────────────────────────

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
          headers: {
            'Content-Type':  'application/json',
            'Authorization': `Bearer ${this.getToken()}`,
          },
          body: JSON.stringify({ refreshToken: rt }),
        });
      } catch { /* silencioso */ }
    }
    this.clearSession();
    window.location.href = getLoginUrl();
  },

  // ── Guardias de ruta ──────────────────────────────────────────────────────

  /**
   * Llama esto al inicio de cada página protegida.
   * Si no hay sesión → login.
   * Si el rol no coincide → dashboard correcto para ese rol.
   */
  requireAuth(requiredRole = null) {
    if (!this.isAuthenticated()) {
      window.location.href = getLoginUrl();
      return false;
    }

    if (requiredRole && this.getRole() !== requiredRole) {
      window.location.href = getDashboardUrl(this.getRole());
      return false;
    }

    return true;
  },

  /**
   * Llama esto en login.html.
   * Si ya hay sesión redirige al dashboard del rol correcto.
   */
  redirectIfAuthenticated() {
    if (!this.isAuthenticated()) return;
    window.location.href = getDashboardUrl(this.getRole());
  },
};
