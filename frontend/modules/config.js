
const CONFIG = {
  API_BASE_URL: 'https://aplicaci-n-sistema-de-control-de.onrender.com/api',


  STORAGE: {
    TOKEN:         'mipyme_token',
    REFRESH_TOKEN: 'mipyme_refresh_token',
    USER:          'mipyme_user',
    EXPIRATION:    'mipyme_expiration',
  },


  ROLES: {
    ADMIN:    'Admin',
    EMPLEADO: 'Empleado',
  },

  // Debounce para buscadores (ms)
  DEBOUNCE_MS: 350,

  // Margen antes de expiración del JWT para renovar automáticamente (ms)
  REFRESH_BEFORE_MS: 5 * 60 * 1000,
};

export default CONFIG;
