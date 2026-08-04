/**
 * config.js — Configuración global del frontend.
 *
 * IMPORTANTE — REDIRECT_AFTER_LOGIN usa rutas RELATIVAS al archivo login.html
 * (que está en pages/auth/). Así funciona con cualquier puerto y servidor.
 */

const CONFIG = {
  // URL base de la API.
  // Desarrollo local: 'http://localhost:5244/api'
  // Producción Render: cambia por tu URL real
  API_BASE_URL: 'https://aplicaci-n-sistema-de-control-de.onrender.com/api',

  // Claves en localStorage
  STORAGE: {
    TOKEN:         'mipyme_token',
    REFRESH_TOKEN: 'mipyme_refresh_token',
    USER:          'mipyme_user',
    EXPIRATION:    'mipyme_expiration',
  },

  // Roles del sistema
  ROLES: {
    ADMIN:    'Admin',
    EMPLEADO: 'Empleado',
  },

  // Rutas de redirección RELATIVAS al archivo login.html (pages/auth/)
  // ../../ sube dos niveles hasta la raíz del servidor (frontend/)
  REDIRECT_AFTER_LOGIN: {
    Admin:    '../../pages/admin/dashboard.html',
    Empleado: '../../pages/empleado/dashboard.html',
  },

  DEBOUNCE_MS:      350,
  REFRESH_BEFORE_MS: 5 * 60 * 1000,
};

export default CONFIG;
