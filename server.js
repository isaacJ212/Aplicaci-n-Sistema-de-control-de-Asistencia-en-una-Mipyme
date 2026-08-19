/**
 * server.js — Servidor HTTP estandarizado para el frontend de MipymeAsistencia.
 *
 * Soluciona permanentemente los errores 404 de módulos JS al servir
 * contenido con rutas limpias y mapeo predecible.
 *
 * USO:
 *   npm install        # instalar dependencia (express)
 *   npm run frontend   # iniciar servidor en puerto 3000
 *
 * RUTAS DISPONIBLES:
 *   /                    → Redirección inteligente (dashboard o login)
 *   /login               → Pantalla de inicio de sesión
 *   /admin/dashboard     → Dashboard del administrador
 *   /empleado/dashboard  → Dashboard del empleado
 *   /empleado/marcaje    → Pantalla de marcaje (con QR/GPS)
 *   /kiosko-qr           → ⭐ Kiosko público de QR (pantalla/tableta)
 *   /asistencia/estacion-qr → Alias del kiosko QR
 *   /api/*               → (opcional) Proxy al backend si existe
 */

const path    = require('path');
const express = require('express');
const app     = express();

const PORT       = process.env.PORT || 3000;
const FRONT_DIR  = path.join(__dirname, 'frontend');

const ADMIN_PAGES = [
  'dashboard', 'empleados', 'empleado-detalle', 'planillas',
  'aprobaciones', 'rendimiento', 'expedientes', 'configuracion',
  'evaluacion', 'informe-asistencia', 'kiosko', 'usuarios'
];

const ANALISTA_PAGES = [
  'dashboard', 'evaluacion',
];

const EMPLEADO_PAGES = [
  'dashboard', 'marcaje', 'historial', 'nomina',
  'solicitudes', 'horas-extras', 'evaluacion'
];

// ── Middleware: Log de peticiones ─────────────────────────────────────────────
app.use((req, _res, next) => {
  const ts = new Date().toISOString().slice(11, 19);
  console.log(`[${ts}] ${req.method} ${req.url}`);
  next();
});

// ── Rutas cortas / amigables ─────────────────────────────────────────────────
app.get('/', (_req, res) => {
  res.sendFile(path.join(FRONT_DIR, 'index.html'));
});

app.get('/login', (_req, res) => {
  res.sendFile(path.join(FRONT_DIR, 'pages', 'auth', 'login.html'));
});

// Alias /pages/* — enlaces internos en HTML y compatibilidad con rutas legacy
app.get('/pages/auth/login.html', (_req, res) => {
  res.sendFile(path.join(FRONT_DIR, 'pages', 'auth', 'login.html'));
});

ADMIN_PAGES.forEach(page => {
  app.get(`/pages/admin/${page}.html`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'admin', `${page}.html`));
  });
});

EMPLEADO_PAGES.forEach(page => {
  app.get(`/pages/empleado/${page}.html`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'empleado', `${page}.html`));
  });
});

app.get('/kiosko-qr', (_req, res) => {
  res.sendFile(path.join(FRONT_DIR, 'pages', 'kiosko-qr.html'));
});

app.get('/asistencia/estacion-qr', (_req, res) => {
  res.sendFile(path.join(FRONT_DIR, 'pages', 'kiosko-qr.html'));
});

// ── Rutas admin ───────────────────────────────────────────────────────────────
ADMIN_PAGES.forEach(page => {
  app.get(`/admin/${page}`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'admin', `${page}.html`));
  });
});

// ── Rutas analista ────────────────────────────────────────────────────────────
ANALISTA_PAGES.forEach(page => {
  app.get(`/analista/${page}`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'analista', `${page}.html`));
  });
  app.get(`/pages/analista/${page}.html`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'analista', `${page}.html`));
  });
});

// ── Rutas empleado ────────────────────────────────────────────────────────────
EMPLEADO_PAGES.forEach(page => {
  app.get(`/empleado/${page}`, (_req, res) => {
    res.sendFile(path.join(FRONT_DIR, 'pages', 'empleado', `${page}.html`));
  });
});

// ── Archivos estáticos (assets, modules, components) ─────────────────────────
app.use('/assets',     express.static(path.join(FRONT_DIR, 'assets')));
app.use('/modules',    express.static(path.join(FRONT_DIR, 'modules')));
app.use('/components', express.static(path.join(FRONT_DIR, 'components')));

// Fallback: si alguien accede a /frontend/pages/... también funciona
app.use('/frontend', express.static(FRONT_DIR));

// ── Manejo de 404 amigable ────────────────────────────────────────────────────
app.use((req, res) => {
  res.status(404).type('html').send(`
    <html lang="es">
    <head>
      <meta charset="UTF-8">
      <title>404 — MipymeAsistencia</title>
      <style>
        body { font-family: Inter, system-ui, sans-serif; background: #f8fafc;
               display: flex; align-items: center; justify-content: center;
               min-height: 100vh; margin: 0; color: #1e293b; }
        .card { background: #fff; padding: 48px; border-radius: 16px;
                box-shadow: 0 10px 25px rgba(0,0,0,.08); text-align: center;
                max-width: 420px; }
        h1 { font-size: 3rem; margin: 0 0 8px; color: #2563eb; }
        h2 { font-size: 1.2rem; margin: 0 0 16px; }
        p  { color: #64748b; margin: 0 0 24px; font-size: .9rem; }
        a  { display: inline-block; background: #2563eb; color: #fff;
             padding: 10px 20px; border-radius: 8px; text-decoration: none;
             font-weight: 600; font-size: .9rem; }
        a:hover { background: #1d4ed8; }
        code { background: #f1f5f9; padding: 2px 6px; border-radius: 4px;
               font-size: .8rem; color: #334155; }
      </style>
    </head>
    <body>
      <div class="card">
        <h1>404</h1>
        <h2>Página no encontrada</h2>
        <p>La ruta <code>${req.path}</code> no existe o fue movida.</p>
        <p style="font-size:.8rem">
          Rutas disponibles: <a href="/login" style="background:transparent;color:#2563eb;padding:0">/login</a> ·
          <a href="/kiosko-qr" style="background:transparent;color:#2563eb;padding:0">/kiosko-qr</a>
        </p>
        <a href="/">Ir al inicio</a>
      </div>
    </body>
    </html>
  `);
});

// ── Inicio del servidor ───────────────────────────────────────────────────────
app.listen(PORT, () => {
  console.log('\n' + '═'.repeat(60));
  console.log('  🏢 MipymeAsistencia — Servidor Frontend');
  console.log('═'.repeat(60));
  console.log(`  ✅ En línea:       http://localhost:${PORT}`);
  console.log(`  🔐 Login:          http://localhost:${PORT}/login`);
  console.log(`  📱 Kiosko QR:      http://localhost:${PORT}/kiosko-qr`);
  console.log(`                    http://localhost:${PORT}/asistencia/estacion-qr`);
  console.log(`  👑 Admin Panel:    http://localhost:${PORT}/admin/dashboard`);
  console.log(`  👷 Empleado:       http://localhost:${PORT}/empleado/dashboard`);
  console.log(`  📂 Static root:    ${FRONT_DIR}`);
  console.log('═'.repeat(60) + '\n');
});
