/**
 * api.js — Capa de comunicación con el backend.
 * Todos los fetch de la app pasan por aquí.
 * Maneja automáticamente: token JWT, refresco, errores y formato ApiResponse.
 */

import CONFIG          from './config.js';
import { AuthService } from './auth.js';

// ── Cliente HTTP base ─────────────────────────────────────────────────────────

async function request(endpoint, options = {}) {
  const url     = `${CONFIG.API_BASE_URL}${endpoint}`;
  const headers = { 'Content-Type': 'application/json', ...options.headers };

  const token = AuthService.getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let response;
  try {
    response = await fetch(url, {
      ...options,
      headers,
      body: options.body ? JSON.stringify(options.body) : undefined,
    });
  } catch (networkError) {
    // Error de red puro (backend apagado, CORS preflight bloqueado, etc.)
    const err = new Error(
      'No se pudo conectar al servidor. Verifica que el backend esté corriendo en ' +
      CONFIG.API_BASE_URL
    );
    err.statusCode = 0;
    throw err;
  }

  // Si el token expiró (401) intenta renovarlo una vez
  if (response.status === 401 && !options._retry) {
    const renewed = await AuthService.tryRefresh();
    if (renewed) {
      return request(endpoint, { ...options, _retry: true });
    } else {
      AuthService.logout();
      return;
    }
  }

  // Parsea la respuesta como JSON (ApiResponse<T>)
  let data;
  try {
    data = await response.json();
  } catch {
    data = null;
  }

  // Si el backend devuelve success:false lanza un error legible
  if (data && data.success === false) {
    const err = new Error(data.message || 'Error desconocido del servidor.');
    err.statusCode = data.statusCode ?? response.status;
    err.data       = data.data;
    throw err;
  }

  return data?.data ?? data;
}

// ── Métodos convenientes ──────────────────────────────────────────────────────

export const api = {
  get:    (url, opts = {})       => request(url, { method: 'GET',    ...opts }),
  post:   (url, body, opts = {}) => request(url, { method: 'POST',   body, ...opts }),
  put:    (url, body, opts = {}) => request(url, { method: 'PUT',    body, ...opts }),
  patch:  (url, body, opts = {}) => request(url, { method: 'PATCH',  body, ...opts }),
  delete: (url, opts = {})       => request(url, { method: 'DELETE', ...opts }),
};

// ── Endpoints por módulo ──────────────────────────────────────────────────────

export const authApi = {
  login:    (body) => request('/auth/login',    { method: 'POST', body }),
  register: (body) => request('/auth/register', { method: 'POST', body }),
  refresh:  (body) => request('/auth/refresh',  { method: 'POST', body }),
  me:       ()     => api.get('/auth/me'),
  logout:   (body) => api.post('/auth/logout',  body),
};

export const sedeApi = {
  get:    ()     => api.get('/sede/configuracion'),
  update: (body) => api.put('/sede/configuracion', body),
};

export const empleadoApi = {
  getAll:  ()          => api.get('/empleado'),
  getById: (id)        => api.get(`/empleado/${id}`),
  create:  (body)      => api.post('/empleado', body),
  update:  (id, body)  => api.put(`/empleado/${id}`, body),
  delete:  (id)        => api.delete(`/empleado/${id}`),
};

export const asistenciaApi = {
  getAll:    (params = '') => api.get(`/asistencia${params}`),
  historial: (id)          => api.get(`/asistencia/historial/${id}`),
  qrActual:  ()            => api.get('/asistencia/qr-actual'),
  generarQr: (id)          => api.post(`/asistencia/generar-qr/${id}`),
  validarQr: (body)        => api.post('/asistencia/validar-qr', body),
  registrar: (body)        => api.post('/asistencia/registrar', body),
};

export const horasExtrasApi = {
  pendientes:      ()          => api.get('/horasextras/pendientes'),
  getByEmpleado:   (id)        => api.get(`/horasextras/empleado/${id}`),
  registrar:       (body)      => api.post('/horasextras', body),
  gestionarEstado: (id, body)  => api.patch(`/horasextras/${id}/estado`, body),
};

export const planillaApi = {
  getAll:        (params = '') => api.get(`/planilla${params}`),
  getByEmpleado: (id, periodo = '') =>
    api.get(`/planilla/empleado/${id}${periodo ? `?periodo=${periodo}` : ''}`),
  generar:       (body)        => api.post('/planilla', body),
};

export const permisoApi = {
  getSolicitudes: (params = '') => api.get(`/permisovacacion/solicitudes${params}`),
  getMias:        (id, params = '') => api.get(`/permisovacacion/mis-solicitudes/${id}${params}`),
  solicitar:      (body)        => api.post('/permisovacacion/solicitar', body),
  responder:      (id, body)    => api.put(`/permisovacacion/${id}/responder`, body),
};
