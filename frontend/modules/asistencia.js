import { asistenciaApi, sedeApi } from './api.js';


export const AsistenciaGPS = {

  /**
   * Obtiene coordenadas GPS del dispositivo.
   * @param {number} timeoutMs Tiempo máximo de espera (default 8s)
   * @returns {Promise<{lat:number, lon:number, accuracy:number}|null>}
   */
  async obtenerUbicacion(timeoutMs = 8000) {
    if (!navigator.geolocation) return null;

    return new Promise((resolve) => {
      navigator.geolocation.getCurrentPosition(
        (pos) => resolve({
          lat: pos.coords.latitude,
          lon: pos.coords.longitude,
          accuracy: pos.coords.accuracy ?? 0,
        }),
        () => resolve(null),
        { timeout: timeoutMs, enableHighAccuracy: true, maximumAge: 5000 }
      );
    });
  },

  /**
   * Calcula distancia Haversine en metros entre 2 coordenadas decimales.
   */
  distanciaMetros(lat1, lon1, lat2, lon2) {
    const R = 6371000;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2
            + Math.cos(lat1 * Math.PI / 180)
            * Math.cos(lat2 * Math.PI / 180)
            * Math.sin(dLon / 2) ** 2;
    return Math.round(R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a)));
  },

 
  async esperarGPS(timeoutMs = 10000) {
    const inicio = Date.now();
    return new Promise((resolve) => {
      const timer = setInterval(async () => {
        const pos = await this.obtenerUbicacion(3000);
        if (pos && pos.lat != null) {
          clearInterval(timer);
          resolve(pos);
        } else if (Date.now() - inicio > timeoutMs) {
          clearInterval(timer);
          resolve(null);
        }
      }, 500);
    });
  },
};



const HTML5_QR_CODE_CDN = 'https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js';

export function cargarHtml5QrCode() {
  if (window.Html5QrcodeScanner || window.Html5Qrcode) {
    return Promise.resolve();
  }
  if (document.querySelector(`script[src="${HTML5_QR_CODE_CDN}"]`)) {
    return new Promise((res) => {
      const check = setInterval(() => {
        if (window.Html5QrcodeScanner || window.Html5Qrcode) {
          clearInterval(check); res();
        }
      }, 100);
    });
  }
  return new Promise((resolve, reject) => {
    const s = document.createElement('script');
    s.src = HTML5_QR_CODE_CDN;
    s.async = true;
    s.onload = resolve;
    s.onerror = () => reject(new Error('No se pudo cargar html5-qrcode.'));
    document.head.appendChild(s);
  });
}



export const AsistenciaService = {

  /**
   * Registra un marcaje enviando el body esperado por el backend.
   *
   * Formato final enviado a POST /api/Asistencia/registrar:
   *   {
   *     idEmpleado,
   *     tipoMarcaje,                // 'Automatico' deja que el backend decida
   *     latitudMarcaje,
   *     longitudMarcaje,
   *     tokenQrEscaneado,           // <- QR del kiosco
   *     codigoOtpGenerado: ''
   *   }
   *
   * @param {object} params
   * @returns {Promise<AsistenciaResponseDto>}
   */
  async registrarMarcaje({
    idEmpleado,
    qrToken,
    latitud,
    longitud,
    tipoMarcaje = 'Automatico',
  }) {
    if (!idEmpleado) throw new Error('No se identificó el empleado.');
    if (!qrToken) throw new Error('Escanea el código QR del kiosco primero.');
    if (latitud == null || longitud == null) throw new Error('Faltan las coordenadas GPS.');

    const body = {
      idEmpleado,
      tipoMarcaje,
      latitudMarcaje: Number(latitud),
      longitudMarcaje: Number(longitud),
      tokenQrEscaneado: qrToken,
      codigoOtpGenerado: '',
    };

    return asistenciaApi.registrar(body);
  },

  /**
   * Obtiene el QR activo de la sede + info de sede en una sola llamada conveniente.
   */
  async obtenerContexto() {
    const [sede, qr] = await Promise.all([
      sedeApi.get().catch(() => null),
      asistenciaApi.qrActual().catch(() => null),
    ]);
    return { sede, qrToken: qr?.tokenQrActual ?? qr?.token ?? null, qr };
  },

 
  determinarSiguienteMarcaje(asistenciaHoy) {
    if (!asistenciaHoy) return 'Entrada';
    if (asistenciaHoy.horaSalida) return null;
    if (!asistenciaHoy.inicioAlmuerzo) return 'InicioAlmuerzo';
    if (!asistenciaHoy.finAlmuerzo) return 'FinAlmuerzo';
    return 'Salida';
  },
};


export class AsistenciaScanner {

  constructor(opts = {}) {
    this.containerId      = opts.containerId || 'scanner-container';
    this.videoConstraints = opts.videoConstraints || { facingMode: { ideal: 'environment' } };
    this.onStatus         = opts.onStatus      || (() => {});
    this.onSuccess        = opts.onSuccess     || (() => {});
    this.onError          = opts.onError       || (() => {});
    this.onDeteccionCruda = opts.onDeteccionCruda || null;

    this.html5Qr = null;
    this.activo  = false;
    this.gps     = null;
  }



  async iniciar() {
    this.activo = true;
    await cargarHtml5QrCode();
    await this._iniciarCamara();
    this._obtenerGPSEnParalelo();
  }

  async detener() {
    this.activo = false;
    if (this.html5Qr) {
      try {
        if (this.html5Qr.isScanning) await this.html5Qr.stop();
        this.html5Qr.clear();
      } catch { /* ignore */ }
      this.html5Qr = null;
    }
  }

  async forzarDeteccion(qrToken, { idEmpleado, latitud, longitud }) {
    if (this.onDeteccionCruda) this.onDeteccionCruda(qrToken);
    await this._procesarMarcaje(qrToken, { idEmpleado, latitud, longitud });
  }


  async _iniciarCamara() {
    const container = document.getElementById(this.containerId);
    if (!container) throw new Error(`No existe el contenedor #${this.containerId}`);

    try {
      if (window.Html5Qrcode) {
        this.html5Qr = new window.Html5Qrcode(this.containerId, {
          formatsToSupport: [0],
          verbose: false,
        });
        await this.html5Qr.start(
          this.videoConstraints,
          { fps: 10, qrbox: { width: 260, height: 260 } },
          (decodedText) => this._alDetectar(decodedText),
          () => { /* escaneo silencioso de frames — ignoramos errores por frame */ }
        );
      } else if (window.Html5QrcodeScanner) {
        this.html5Qr = new window.Html5QrcodeScanner(this.containerId, {
          fps: 10, qrbox: 260, rememberLastUsedCamera: true,
        });
        this.html5Qr.render(
          (decodedText) => this._alDetectar(decodedText),
          () => {}
        );
      }
      this.onStatus('Cámara lista. Apunta al QR del kiosco.', 'info');
    } catch (err) {
      this.onStatus('No se pudo acceder a la cámara: ' + err.message, 'error');
      throw err;
    }
  }

  async _obtenerGPSEnParalelo() {
    const pos = await AsistenciaGPS.obtenerUbicacion(8000);
    if (pos) {
      this.gps = pos;
      this.onStatus(
        `GPS ok: ${pos.lat.toFixed(4)}, ${pos.lon.toFixed(4)} (±${Math.round(pos.accuracy)}m)`,
        'success'
      );
    } else {
      this.onStatus('GPS no disponible. Esperando ubicación...', 'warning');
      this.gps = await AsistenciaGPS.esperarGPS(12000);
      if (!this.gps) this.onStatus('No se pudo obtener ubicación GPS.', 'error');
    }
  }

  async _alDetectar(decodedText) {
    if (!this.activo) return;
    this.activo = false;

    try {
      this._beepOk();
      if (this.onDeteccionCruda) this.onDeteccionCruda(decodedText);
      this.onStatus('QR detectado. Registrando marcaje...', 'info');

      await this._procesarMarcaje(decodedText, {
        idEmpleado: null,
        latitud: this.gps?.lat,
        longitud: this.gps?.lon,
      });
    } catch (err) {
      this.onError(err);
      this._beepError();
    }
  }

  async _procesarMarcaje(qrToken, { idEmpleado, latitud, longitud }) {
    // Si no tenemos GPS, esperamos un poco más
    if (latitud == null || longitud == null) {
      this.onStatus('Esperando coordenadas GPS para validar la sede...', 'warning');
      const pos = await AsistenciaGPS.esperarGPS(10000);
      if (!pos) {
        throw new Error('No se obtuvo ubicación GPS. Activa los permisos o usa marcaje manual.');
      }
      latitud  = pos.lat;
      longitud = pos.lon;
    }

    const resultado = await AsistenciaService.registrarMarcaje({
      idEmpleado,
      qrToken,
      latitud,
      longitud,
      tipoMarcaje: 'Automatico',
    });

    this.onSuccess(resultado, { latitud, longitud, qrToken });
  }

  _beep(freq = 800, ms = 60, type = 'sine', gain = 0.1) {
    try {
      const AC = window.AudioContext || window.webkitAudioContext;
      if (!AC) return;
      const ctx = new AC();
      const o = ctx.createOscillator();
      const g = ctx.createGain();
      o.type = type; o.frequency.value = freq;
      g.gain.setValueAtTime(0.0001, ctx.currentTime);
      g.gain.exponentialRampToValueAtTime(gain, ctx.currentTime + 0.01);
      g.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + ms / 1000);
      o.connect(g); g.connect(ctx.destination);
      o.start(); o.stop(ctx.currentTime + ms / 1000 + 0.02);
    } catch { /* silent */ }
  }

  _beepOk() {
    this._beep(880, 80, 'sine', 0.15);
    setTimeout(() => this._beep(1320, 100, 'sine', 0.12), 110);
  }

  _beepError() {
    this._beep(220, 200, 'square', 0.1);
  }
}

export default {
  AsistenciaGPS,
  AsistenciaService,
  AsistenciaScanner,
  cargarHtml5QrCode,
};
