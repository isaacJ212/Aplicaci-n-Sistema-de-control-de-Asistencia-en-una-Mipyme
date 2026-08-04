/**
 * auth.js — Gestión de sesión: guardar/leer tokens, usuario, logout y guardia de rutas.
 */

import CONFIG from './config.js';

export const AuthService = {

  // ── Persistencia ──────────────────────────────────────────────────────────

  /** Guarda los datos del login en localStorage */
  saveSession(loginResponse) {
    localStorage.setItem(CONFIG.STORAGE.TOKEN,         loginResponse.token);
    localStorage.setItem(CONFIG.STORAGE.REFRESH_TOKEN, loginResponse.refreshToken);
    localStorage.setItem(CONFIG.STORAGE.EXPIRATION,    loginResponse.expiration);
    localStorage.setItem(CONFIG.STORAGE.USER, JSON.stringify({
      email: loginResponse.email,
      role:  loginResponse.role,
    }));
  },

  /** Borra la sesión del localStorage */
  clearSession() {
    Object.values(CONFIG.STORAGE).forEach(k => localStorage.removeItem(k));
  },

  /** Devuelve el JWT actual */
  getToken() {
    return localStorage.getItem(CONFIG.STORAGE.TOKEN);
  },

  /** Devuelve el refresh token */
  getRefreshToken() {
    return localStorage.getItem(CONFIG.STORAGE.REFRESH_TOKEN);
  },

  /** Devuelve el objeto usuario { email, role } */
  getUser() {
    try {
      return JSON.parse(localStorage.getItem(CONFIG.STORAGE.USER) ?? 'null');
    } catch {
      return null;
    }
  },

  /** Devuelve el rol del usuario autenticado */
  getRole() {
    return this.getUser()?.role ?? null;
  },

  /** Indica si hay una sesión activa (token presente) */
  isAuthenticated() {
    return !!this.getToken();
  },

  /** Indica si el usuario es Admin */
  isAdmin() {
    return this.getRole() === CONFIG.ROLES.ADMIN;
  },

  // ── Refresco automático de token ──────────────────────────────────────────

  /**
   * Intenta renovar el JWT usando el refresh token.
   * Retorna true si tuvo éxito, false si hay que re-autenticar.
   */
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
      if (body?.success && body.data) {
        this.saveSession(body.data);
        return true;
      }
      return false;
    } catch {
      return false;
    }
  },

  // ── Logout ────────────────────────────────────────────────────────────────

  /** Cierra sesión: revoca el token en el backend y limpia localStorage */
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
      } catch {
        // Silencioso — limpiamos local de todas formas
      }
    }
    this.clearSession();
    window.location.href = '/frontend/pages/auth/login.html';
  },

  // ── Guardia de rutas ──────────────────────────────────────────────────────

  /**
   * Llama esto al inicio de cada página protegida.
   * Si no hay sesión redirige al login.
   * Si se pasa un rol requerido y el usuario no lo tiene, redirige al dashboard correcto.
   */
  requireAuth(requiredRole = null) {
    if (!this.isAuthenticated()) {
      window.location.href = '/frontend/pages/auth/login.html';
      return false;
    }
    if (requiredRole && this.getRole() !== requiredRole) {
      // Redirige al dashboard del rol correcto
      const dest = CONFIG.REDIRECT_AFTER_LOGIN[this.getRole()]
                ?? '/frontend/pages/auth/login.html';
      window.location.href = dest;
      return false;
    }
    return true;
  },

  /**
   * Llama esto en la página de login.
   * Si ya hay sesión activa redirige al dashboard correspondiente.
   */
  redirectIfAuthenticated() {
    if (this.isAuthenticated()) {
      const dest = CONFIG.REDIRECT_AFTER_LOGIN[this.getRole()]
                ?? '/frontend/pages/auth/login.html';
      window.location.href = dest;
    }
  },
};
