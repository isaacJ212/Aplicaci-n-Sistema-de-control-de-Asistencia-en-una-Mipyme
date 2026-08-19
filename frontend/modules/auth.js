import CONFIG from './config.js';



let cachedBase = null;

function getBase() {
  if (cachedBase) return cachedBase;

  const { origin, pathname } = window.location;
  const idx = pathname.indexOf('/frontend/');
  cachedBase = idx !== -1
    ? origin + pathname.slice(0, idx) + '/frontend'
    : origin;

  return cachedBase;
}

function urlPage(path) {
  return `${getBase()}/${path}`;
}



export const AuthService = {

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
  isAdmin()    { return this.getRole() === CONFIG.ROLES.ADMIN; },
  isAnalista() { return this.getRole() === CONFIG.ROLES.ANALISTA; },



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
    window.location.href = urlPage('pages/auth/login.html');
  },

  getDashboardUrl() {
    const role = this.getRole();
    if (role === CONFIG.ROLES.ADMIN)    return urlPage('pages/admin/dashboard.html');
    if (role === CONFIG.ROLES.ANALISTA) return urlPage('pages/analista/dashboard.html');
    return urlPage('pages/empleado/dashboard.html');
  },

  requireAuth(requiredRole = null) {
    if (!this.isAuthenticated()) {
      window.location.href = urlPage('pages/auth/login.html');
      return false;
    }
    if (requiredRole) {
      const roles = Array.isArray(requiredRole) ? requiredRole : [requiredRole];
      if (!roles.includes(this.getRole())) {
        window.location.href = this.getDashboardUrl();
        return false;
      }
    }
    return true;
  },

  redirectIfAuthenticated() {
    if (!this.isAuthenticated()) return;
    window.location.href = this.getDashboardUrl();
  },
};
