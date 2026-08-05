/**
 * layout.js — Carga el sidebar compartido e inicializa el layout.
 *
 * Compatible con dos modos de servidor:
 *   A) Live Preview desde workspace root → URL: /frontend/pages/admin/dashboard.html
 *   B) Live Server desde frontend/       → URL: /pages/admin/dashboard.html
 *
 * getBase() detecta el modo automáticamente leyendo window.location.pathname.
 */

import { AuthService } from './auth.js';
import { toast }       from './utils.js';

// ── Detectar base del servidor ────────────────────────────────────────────────

/**
 * Retorna el prefijo de URL hasta la raíz del proyecto frontend.
 *
 * Modo A  (pathname tiene /frontend/):  http://127.0.0.1:3000/frontend
 * Modo B  (pathname NO tiene /frontend): http://127.0.0.1:5500
 */
function getBase() {
  const { origin, pathname } = window.location;
  const idx = pathname.indexOf('/frontend/');
  return idx !== -1
    ? origin + pathname.slice(0, idx) + '/frontend'
    : origin;
}

// ── Cargar sidebar ────────────────────────────────────────────────────────────

async function loadSidebar(htmlFile, currentPage) {
  const container = document.getElementById('sidebar-container');
  if (!container) {
    console.error('[layout] No existe #sidebar-container en el DOM.');
    return;
  }

  const base     = getBase();
  const url      = `${base}/components/${htmlFile}`;

  // Fetch con diagnóstico claro en consola
  let html = '';
  try {
    const res = await fetch(url);
    if (!res.ok) {
      console.error(`[layout] No se pudo cargar el sidebar: ${url} → HTTP ${res.status}`);
      container.innerHTML = `
        <aside style="width:240px;background:#1E293B;padding:20px;color:#EF4444;font-size:.8rem">
          ⚠️ Sidebar no encontrado<br>
          <small style="color:#94A3B8">${url}</small>
        </aside>`;
      return;
    }
    html = await res.text();
  } catch (err) {
    console.error(`[layout] Error de red cargando sidebar: ${err.message}`);
    container.innerHTML = `
      <aside style="width:240px;background:#1E293B;padding:20px;color:#EF4444;font-size:.8rem">
        🔌 Error de red<br>
        <small style="color:#94A3B8">${err.message}</small>
      </aside>`;
    return;
  }

  container.innerHTML = html;

  // Resolver data-nav → href absoluto usando getBase()
  container.querySelectorAll('[data-nav]').forEach(el => {
    const nav = el.getAttribute('data-nav');
    if (nav) el.setAttribute('href', `${base}/${nav}`);
  });

  // Marcar página activa
  if (currentPage) {
    container.querySelector(`.sidebar-link[data-page="${currentPage}"]`)
             ?.classList.add('active');
  }

  // Rellenar datos del usuario
  const user = AuthService.getUser();
  if (user) {
    const emailEl  = container.querySelector('#sidebar-email');
    const avatarEl = container.querySelector('#sidebar-avatar');
    if (emailEl)  emailEl.textContent  = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }

  // Botón logout
  document.addEventListener('click', (e) => {
    if (e.target.closest('#sidebar-logout-btn')) {
      toast('Cerrando sesión...', 'info', 800);
      setTimeout(() => AuthService.logout(), 600);
    }
  });

  // Menú hamburguesa en móvil (solo portal empleado)
  if (htmlFile === 'sidebar-empleado.html') {
    const sidebar = container.querySelector('.sidebar-emp');
    const topbar  = document.querySelector('.topbar');
    if (sidebar && topbar && !topbar.querySelector('.mobile-menu-toggle')) {
      const btn = document.createElement('button');
      btn.type      = 'button';
      btn.className = 'mobile-menu-toggle';
      btn.setAttribute('aria-label', 'Abrir menú');
      btn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
        stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M4 6h16M4 12h16M4 18h16"/></svg>`;
      topbar.prepend(btn);

      btn.addEventListener('click', () => sidebar.classList.toggle('open'));
      document.addEventListener('click', (ev) => {
        if (window.innerWidth <= 1024
            && sidebar.classList.contains('open')
            && !ev.target.closest('.sidebar-emp')
            && !ev.target.closest('.mobile-menu-toggle')) {
          sidebar.classList.remove('open');
        }
      });
      window.addEventListener('resize', () => {
        if (window.innerWidth > 1024) sidebar.classList.remove('open');
      });
    }
  }
}

// ── Exports públicos ──────────────────────────────────────────────────────────

/** Inicializa el layout del portal Admin. */
export async function initAdminLayout(currentPage = '') {
  if (!AuthService.requireAuth('Admin')) return;
  await loadSidebar('sidebar-admin.html', currentPage);
}

/** Inicializa el layout del portal Empleado. */
export async function initEmpleadoLayout(currentPage = '') {
  if (!AuthService.requireAuth('Empleado')) return;
  await loadSidebar('sidebar-empleado.html', currentPage);
}
