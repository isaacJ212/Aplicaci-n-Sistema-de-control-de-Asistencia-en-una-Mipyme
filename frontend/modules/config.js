/**
 * config.js — Configuración global del frontend
 * Cambia API_BASE_URL para apuntar a producción en Render.
 */

const CONFIG = {
  // URL base de la API — en producción reemplaza con tu URL de Render
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

  // Rutas por rol — relativas a la raíz del servidor de Live Preview
  // Live Preview sirve desde la raíz del workspace, por eso la ruta
  // incluye la carpeta /frontend/ completa.
  REDIRECT_AFTER_LOGIN: {
    Admin:    '/frontend/pages/admin/dashboard.html',
    Empleado: '/frontend/pages/empleado/dashboard.html',
  },

  // Tiempo de debounce en ms para búsquedas
  DEBOUNCE_MS: 350,

  // Cuántos ms antes de la expiración del token renovar automáticamente
  REFRESH_BEFORE_MS: 5 * 60 * 1000, // 5 minutos
};

export default CONFIG;
