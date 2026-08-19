
import { asistenciaApi, sedeApi } from './api.js';


export const AsistenciaGPS = {
  posicionCache: null,
  timestampCache: 0,
  CACHE_DURACION_MS: 30000,

  async obtenerUbicacion(timeoutMs = 8000) {
    if (this.posicionCache && Date.now() - this.timestampCache < this.CACHE_DURACION_MS) {
      return this.posicionCache;
    }

    if (!navigator.geolocation) {
      return null;
    }

    return new Promise((resolve) => {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const resultado = {
            lat: pos.coords.latitude,
            lon: pos.coords.longitude,
            accuracy: pos.coords.accuracy,
            timestamp: pos.timestamp,
          };
          this.posicionCache = resultado;
          this.timestampCache = Date.now();
          resolve(resultado);
        },
        () => resolve(null),
        { enableHighAccuracy: true, timeout: timeoutMs, maximumAge: 10000 }
      );
    });
  },

  async esperarGPS(timeoutMs = 12000) {
    const ubicacion = await this.obtenerUbicacion(4000);
    if (ubicacion) return ubicacion;

    const inicio = Date.now();
    return new Promise((resolve) => {
      const timer = setInterval(async () => {
        const u = await this.obtenerUbicacion(2000);
        if (u) {
          clearInterval(timer);
          resolve(u);
        } else if (Date.now() - inicio > timeoutMs) {
          clearInterval(timer);
          resolve(null);
        }
      }, 500);
    });
  },
};


const HTML5_QR_CODE_CDNS = [
  'https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js',
  'https://cdn.jsdelivr.net/npm/html5-qrcode@2.3.8/html5-qrcode.min.js',
  'https://cdnjs.cloudflare.com/ajax/libs/html5-qrcode/2.3.8/html5-qrcode.min.js'
];

export async function cargarHtml5QrCode() {
  if (window.Html5Qrcode || window.Html5QrcodeScanner) {
    return Promise.resolve();
  }

  for (const cdnUrl of HTML5_QR_CODE_CDNS) {
    try {
      await new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = cdnUrl;
        s.async = true;
        s.onload = resolve;
        s.onerror = reject;
        document.head.appendChild(s);
      });
      if (window.Html5Qrcode || window.Html5QrcodeScanner) {
        return;
      }
    } catch {
      // Intentar con el siguiente CDN
    }
  }

  if (!window.Html5Qrcode && !window.Html5QrcodeScanner) {
    throw new Error('No se pudo cargar la librería de cámara/QR. Verifica tu conexión a internet.');
  }
}


export const AsistenciaService = {
  async registrarMarcaje({
    idEmpleado,
    qrToken,
    latitud,
    longitud,
    tipoMarcaje = 'Automatico',
  }) {
    if (!qrToken) throw new Error('Escanea el código QR del kiosco primero.');
    if (latitud == null || longitud == null) throw new Error('Faltan las coordenadas GPS.');

    const body = {
      idEmpleado: idEmpleado ? Number(idEmpleado) : 0,
      tipoMarcaje,
      latitudMarcaje: Number(latitud),
      longitudMarcaje: Number(longitud),
      tokenQrEscaneado: String(qrToken).trim(),
      codigoOtpGenerado: '',
    };

    return asistenciaApi.registrar(body);
  },

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
    this.containerId      = opts.containerId || 'html5qr-reader';
    this.videoConstraints = opts.videoConstraints || null;
    this.idEmpleado       = opts.idEmpleado || null;
    this.getIdEmpleado   = opts.getIdEmpleado || null;
    this.onStatus         = opts.onStatus      || (() => {});
    this.onSuccess        = opts.onSuccess     || (() => {});
    this.onError          = opts.onError       || (() => {});
    this.onCamerasFound   = opts.onCamerasFound|| (() => {});
    this.onDeteccionCruda = opts.onDeteccionCruda || null;

    this.html5Qr = null;
    this.activo  = false;
    this.gps     = null;
    this.camaras = [];
    this.camaraActualId = null;
  }

  async iniciar(cameraId = null) {
    this.activo = true;
    await cargarHtml5QrCode();
    await this._iniciarCamara(cameraId);
    this._obtenerGPSEnParalelo();
  }

  async detener() {
    this.activo = false;
    if (this.html5Qr) {
      try {
        if (this.html5Qr.isScanning) {
          await this.html5Qr.stop();
        }
        this.html5Qr.clear();
      } catch {
        /* ignorar errores al detener */
      }
      this.html5Qr = null;
    }
  }

  async escanearArchivo(archivoImagen) {
    if (!archivoImagen) return;
    this.activo = true;
    await cargarHtml5QrCode();

    if (!this.html5Qr) {
      this.html5Qr = new window.Html5Qrcode(this.containerId);
    }

    try {
      this.onStatus('Analizando imagen de código QR...', 'info');
      const decodedText = await this.html5Qr.scanFile(archivoImagen, true);
      this._alDetectar(decodedText);
    } catch (err) {
      this.onError(new Error('No se detectó ningún código QR en la imagen. Intenta con una foto más clara o usa la cámara.'));
    }
  }

  async forzarDeteccion(qrToken, { idEmpleado, latitud, longitud }) {
    if (this.onDeteccionCruda) this.onDeteccionCruda(qrToken);
    await this._procesarMarcaje(qrToken, { idEmpleado, latitud, longitud });
  }

  async _iniciarCamara(cameraId = null) {
    const container = document.getElementById(this.containerId);
    if (!container) throw new Error(`No existe el contenedor #${this.containerId}`);

    // Limpiar restos previos
    try {
      if (this.html5Qr) {
        if (this.html5Qr.isScanning) await this.html5Qr.stop();
        this.html5Qr.clear();
      }
    } catch { /* ignore */ }

    // 1. Diagnóstico de contexto seguro
    if (window.isSecureContext === false && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
      const msg = 'El navegador bloquea el acceso a la cámara en conexiones HTTP no seguras. Abre el sistema en https:// o localhost, o usa "Subir foto de QR".';
      this.onStatus(msg, 'warning');
    }

    // 2. Enumerar cámaras disponibles
    try {
      if (window.Html5Qrcode?.getCameras) {
        this.camaras = await window.Html5Qrcode.getCameras();
        if (this.camaras && this.camaras.length > 0) {
          this.onCamerasFound(this.camaras);
        }
      }
    } catch {
      this.camaras = [];
    }

    this.html5Qr = new window.Html5Qrcode(this.containerId, { verbose: false });

    // 3. Determinar configuración de cámara con fallback en cascada
    let targetCamera = cameraId;
    if (!targetCamera && this.camaras && this.camaras.length > 0) {
      // Priorizar cámara trasera en móviles si existe, o la primera encontrada
      const backCam = this.camaras.find(c => /back|rear|trasera|environment|posterior/i.test(c.label));
      targetCamera = backCam ? backCam.id : this.camaras[0].id;
    }

    const qrConfig = {
      fps: 10,
      qrbox: (viewfinderWidth, viewfinderHeight) => {
        const minEdge = Math.min(viewfinderWidth, viewfinderHeight);
        const edge = Math.floor(minEdge * 0.75);
        return { width: Math.max(edge, 200), height: Math.max(edge, 200) };
      },
      aspectRatio: 1.0,
    };

    const qrCallback = (decodedText) => this._alDetectar(decodedText);
    const qrErrorCallback = () => { /* escaneo silencioso */ };

    // Intento 1: Con cámara seleccionada / específica
    if (targetCamera) {
      try {
        await this.html5Qr.start(targetCamera, qrConfig, qrCallback, qrErrorCallback);
        this.camaraActualId = targetCamera;
        this.onStatus('Cámara encendida. Apunta al código QR del kiosco.', 'info');
        return;
      } catch (err1) {
        console.warn('[AsistenciaScanner] Intento 1 falló:', err1);
      }
    }

    // Intento 2: facingMode: environment (cámara trasera)
    try {
      await this.html5Qr.start({ facingMode: 'environment' }, qrConfig, qrCallback, qrErrorCallback);
      this.onStatus('Cámara trasera encendida. Apunta al QR.', 'info');
      return;
    } catch (err2) {
      console.warn('[AsistenciaScanner] Intento 2 (environment) falló:', err2);
    }

    // Intento 3: facingMode: user (webcam frontal de laptop/PC)
    try {
      await this.html5Qr.start({ facingMode: 'user' }, qrConfig, qrCallback, qrErrorCallback);
      this.onStatus('Webcam encendida. Apunta el QR a tu cámara.', 'info');
      return;
    } catch (err3) {
      console.warn('[AsistenciaScanner] Intento 3 (user) falló:', err3);
    }

    // Intento 4: Dispositivo de video genérico
    try {
      await this.html5Qr.start(true, qrConfig, qrCallback, qrErrorCallback);
      this.onStatus('Cámara lista. Apunta al QR.', 'info');
      return;
    } catch (errFinal) {
      let mensajeAmigable = 'No se pudo acceder a la cámara.';
      const errName = errFinal.name || '';
      const errMsg = errFinal.message || '';

      if (/NotAllowedError|PermissionDeniedError/i.test(errName + errMsg)) {
        mensajeAmigable = 'Permiso de cámara denegado. Permite el acceso a la cámara en la barra de direcciones de tu navegador.';
      } else if (/NotFoundError|DevicesNotFoundError/i.test(errName + errMsg)) {
        mensajeAmigable = 'No se encontró ninguna cámara conectada en este dispositivo. Puedes usar la opción "Subir foto de QR".';
      } else if (/NotReadableError|TrackStartError/i.test(errName + errMsg)) {
        mensajeAmigable = 'La cámara está siendo utilizada por otra aplicación. Ciérrala e intenta de nuevo.';
      } else if (/OverconstrainedError/i.test(errName + errMsg)) {
        mensajeAmigable = 'Las características de la cámara no son compatibles con este modo.';
      }

      this.onStatus(mensajeAmigable, 'error');
      throw new Error(mensajeAmigable);
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
      this.onStatus('GPS no disponible aún. Obteniendo señal...', 'warning');
      this.gps = await AsistenciaGPS.esperarGPS(12000);
      if (!this.gps) this.onStatus('No se pudo obtener ubicación GPS.', 'error');
    }
  }

  async _alDetectar(decodedText) {
    if (!this.activo) return;
    this.activo = false;

    try {
      const cleanToken = String(decodedText || '').trim();
      if (!cleanToken) {
        throw new Error('El código QR leído está vacío.');
      }

      if (this.onDeteccionCruda) this.onDeteccionCruda(cleanToken);
      this.onStatus('QR detectado. Registrando marcaje...', 'info');

      const empId = (typeof this.getIdEmpleado === 'function' ? this.getIdEmpleado() : this.idEmpleado) || null;

      await this._procesarMarcaje(cleanToken, {
        idEmpleado: empId,
        latitud: this.gps?.lat,
        longitud: this.gps?.lon,
      });
    } catch (err) {
      this._beepError();
      this.onError(err);
    }
  }

  async _procesarMarcaje(qrToken, { idEmpleado, latitud, longitud }) {
    if (latitud == null || longitud == null) {
      this.onStatus('Esperando coordenadas GPS para validar la sede...', 'warning');
      const pos = await AsistenciaGPS.esperarGPS(10000);
      if (!pos) {
        throw new Error('No se obtuvo ubicación GPS. Activa los permisos de ubicación o usa marcaje manual.');
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

    this._beepOk();
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
