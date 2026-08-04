/**
 * auth.js — Gestión de sesión: guardar/leer tokens, usuario, logout y guardia de rutas.
 */

import CONFIG from './config.js';

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

  getRole()           { return this.getUser()?.role ?? null; },
  isAuthenticated()   { return !!this.getToken(); },
  isAdmin()           { return this.getRole() === CONFIG.ROLES.ADMIN; },

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
    window.location.href = '/frontend/pages/auth/login.html';
  },

  // ── Guardias de ruta ──────────────────────────────────────────────────────

  requireAuth(requiredRole = null) {
    if (!this.isAuthenticated()) {
      window.location.href = '/frontend/pages/auth/login.html';
      return false;
    }
    if (requiredRole && this.getRole() !== requiredRole) {
      window.location.href = CONFIG.REDIRECT_AFTER_LOGIN[this.getRole()]
                          ?? '/frontend/pages/auth/login.html';
      return false;
    }
    return true;
  },

  redirectIfAuthenticated() {
    if (this.isAuthenticated()) {
      window.location.href = CONFIG.REDIRECT_AFTER_LOGIN[this.getRole()]
                          ?? '/frontend/pages/auth/login.html';
    }
  },
};
