
import CONFIG          from './config.js';
import { AuthService } from './auth.js';

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



async function requestBlob(endpoint, options = {}) {
  const url     = `${CONFIG.API_BASE_URL}${endpoint}`;
  const headers = { ...options.headers };

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
    const err = new Error(
      'No se pudo conectar al servidor. Verifica que el backend esté corriendo en ' +
      CONFIG.API_BASE_URL
    );
    err.statusCode = 0;
    throw err;
  }

  if (response.status === 401 && !options._retry) {
    const renewed = await AuthService.tryRefresh();
    if (renewed) {
      return requestBlob(endpoint, { ...options, _retry: true });
    } else {
      AuthService.logout();
      return;
    }
  }

  if (!response.ok) {
    const text = await response.text();
    let errorMessage = text;
    try {
      const json = JSON.parse(text);
      errorMessage = json?.message || text;
    } catch {
      // ignore invalid json
    }
    const err = new Error(errorMessage || 'Error desconocido del servidor.');
    err.statusCode = response.status;
    throw err;
  }

  return response.blob();
}

export const api = {
  get:    (url, opts = {})       => request(url, { method: 'GET',    ...opts }),
  post:   (url, body, opts = {}) => request(url, { method: 'POST',   body, ...opts }),
  put:    (url, body, opts = {}) => request(url, { method: 'PUT',    body, ...opts }),
  patch:  (url, body, opts = {}) => request(url, { method: 'PATCH',  body, ...opts }),
  delete: (url, opts = {})       => request(url, { method: 'DELETE', ...opts }),
  download: (url, opts = {})     => requestBlob(url, { method: 'GET', ...opts }),
};



export const authApi = {
  login:       (body) => request('/auth/login',       { method: 'POST', body }),
  register:    (body) => request('/auth/register',    { method: 'POST', body }),
  refresh:     (body) => request('/auth/refresh',     { method: 'POST', body }),
  verify2fa:   (body) => request('/auth/verify-2fa',   { method: 'POST', body }),
  toggle2fa:   (body) => request('/auth/toggle-2fa',   { method: 'POST', body }),
  me:          ()     => api.get('/auth/me'),
  logout:      (body) => api.post('/auth/logout',     body),
};

export const sedeApi = {
  get:    ()     => api.get('/sede/configuracion'),
  update: (body) => api.put('/sede/configuracion', body),
};

export const empleadoApi = {
  getAll:              ()          => api.get('/empleado'),
  getById:             (id)        => api.get(`/empleado/${id}`),
  create:              (body)      => api.post('/empleado', body),
  update:              (id, body)  => api.put(`/empleado/${id}`, body),
  delete:              (id)        => api.delete(`/empleado/${id}`),
  acumularVacaciones:  (id)        => api.put(`/empleado/${id}/acumular-vacaciones`),
};

export const asistenciaApi = {
  getAll:          (params = '') => api.get(`/asistencia${params}`),
  historial:       (id)          => api.get(`/asistencia/historial/${id}`),
  qrActual:        ()            => api.get('/asistencia/qr-actual'),
  generarQr:       (id)          => api.post(`/asistencia/generar-qr/${id}`),
  rotarQrSede:     ()            => api.post('/asistencia/rotar-qr-sede'),
  validarQr:       (body)        => api.post('/asistencia/validar-qr', body),
  registrar:       (body)        => api.post('/asistencia/registrar', body),
  alertasTardanza: (periodo = '', umbral = 3) =>
    api.get(`/asistencia/alertas-tardanza?periodo=${encodeURIComponent(periodo)}&umbral=${umbral}`),
  informe: (params = {}) => {
    const qs = new URLSearchParams();
    if (params.idEmpleado) qs.set('idEmpleado', params.idEmpleado);
    if (params.fechaDesde) qs.set('fechaDesde', params.fechaDesde);
    if (params.fechaHasta) qs.set('fechaHasta', params.fechaHasta);
    const q = qs.toString();
    return api.get(`/asistencia/informe${q ? '?' + q : ''}`);
  },
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
  export:        (params = '') => api.download(`/planilla/export${params}`),
};

export const evaluacionApi = {
  getPreguntas:   ()              => api.get('/evaluacion/preguntas'),
  getAll:         (params = {})   => {
    const qs = new URLSearchParams();
    if (params.idEmpleado)  qs.set('idEmpleado',  params.idEmpleado);
    if (params.idEvaluador) qs.set('idEvaluador', params.idEvaluador);
    if (params.periodo)     qs.set('periodo',      params.periodo);
    if (params.estado)      qs.set('estado',       params.estado);
    const q = qs.toString();
    return api.get(`/evaluacion${q ? '?' + q : ''}`);
  },
  getById:        (id)            => api.get(`/evaluacion/${id}`),
  crear:          (body)          => api.post('/evaluacion', body),
  responder:      (id, body)      => api.put(`/evaluacion/${id}/responder`, body),
};
  getSolicitudes: (params = '') => api.get(`/permisovacacion/solicitudes${params}`),
  getMias:        (id, params = '') => api.get(`/permisovacacion/mis-solicitudes/${id}${params}`),
  solicitar:      (body)        => api.post('/permisovacacion/solicitar', body),
  responder:      (id, body)    => api.put(`/permisovacacion/${id}/responder`, body),
};
