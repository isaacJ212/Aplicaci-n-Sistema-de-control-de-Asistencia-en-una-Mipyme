/**
 * layout.js — Carga el sidebar compartido e inicializa el layout.
 *
 * Optimizado para reducir peticiones redundantes y listeners duplicados.
 */

import { AuthService } from './auth.js';
import { toast }       from './utils.js';

const SIDEBAR_CACHE = new Map();
let logoutListenerBound = false;
let mobileMenuListenerBound = false;

function getBase() {
  const { origin, pathname } = window.location;
  const idx = pathname.indexOf('/frontend/');
  return idx !== -1
    ? origin + pathname.slice(0, idx) + '/frontend'
    : origin;
}

function bindSidebarLogout() {
  if (logoutListenerBound) return;
  document.addEventListener('click', (e) => {
    if (!e.target.closest('#sidebar-logout-btn')) return;
    toast('Cerrando sesión...', 'info', 800);
    window.setTimeout(() => AuthService.logout(), 600);
  });
  logoutListenerBound = true;
}

function bindMobileSidebar(sidebar, topbar) {
  if (!sidebar || !topbar || mobileMenuListenerBound) return;

  const btn = document.createElement('button');
  btn.type = 'button';
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

  mobileMenuListenerBound = true;
}

function applySidebarHtml(container, html, base, currentPage) {
  container.innerHTML = html;

  container.querySelectorAll('[data-nav]').forEach((el) => {
    const nav = el.getAttribute('data-nav');
    if (nav) el.setAttribute('href', `${base}/${nav}`);
  });

  if (currentPage) {
    container.querySelector(`.sidebar-link[data-page="${currentPage}"]`)
      ?.classList.add('active');
  }

  const user = AuthService.getUser();
  if (user) {
    const emailEl = container.querySelector('#sidebar-email');
    const avatarEl = container.querySelector('#sidebar-avatar');
    if (emailEl) emailEl.textContent = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }
}

async function loadSidebar(htmlFile, currentPage) {
  const container = document.getElementById('sidebar-container');
  if (!container) {
    console.error('[layout] No existe #sidebar-container en el DOM.');
    return;
  }

  const base = getBase();
  const url = `${base}/components/${htmlFile}`;

  if (SIDEBAR_CACHE.has(htmlFile)) {
    applySidebarHtml(container, SIDEBAR_CACHE.get(htmlFile), base, currentPage);
    bindSidebarLogout();

    if (htmlFile === 'sidebar-empleado.html') {
      const sidebar = container.querySelector('.sidebar-emp');
      const topbar = document.querySelector('.topbar');
      if (sidebar && topbar && !topbar.querySelector('.mobile-menu-toggle')) {
        bindMobileSidebar(sidebar, topbar);
      }
    }
    return;
  }

  try {
    const res = await fetch(url, { cache: 'force-cache' });
    if (!res.ok) {
      console.error(`[layout] No se pudo cargar el sidebar: ${url} → HTTP ${res.status}`);
      container.innerHTML = `
        <aside style="width:240px;background:#1E293B;padding:20px;color:#EF4444;font-size:.8rem">
          ⚠️ Sidebar no encontrado<br>
          <small style="color:#94A3B8">${url}</small>
        </aside>`;
      return;
    }

    const html = await res.text();
    SIDEBAR_CACHE.set(htmlFile, html);
    applySidebarHtml(container, html, base, currentPage);
  } catch (err) {
    console.error(`[layout] Error de red cargando sidebar: ${err.message}`);
    container.innerHTML = `
      <aside style="width:240px;background:#1E293B;padding:20px;color:#EF4444;font-size:.8rem">
        🔌 Error de red<br>
        <small style="color:#94A3B8">${err.message}</small>
      </aside>`;
    return;
  }

  bindSidebarLogout();

  if (htmlFile === 'sidebar-empleado.html') {
    const sidebar = container.querySelector('.sidebar-emp');
    const topbar = document.querySelector('.topbar');
    if (sidebar && topbar && !topbar.querySelector('.mobile-menu-toggle')) {
      bindMobileSidebar(sidebar, topbar);
    }
  }
}

export async function updateEmpleadoSidebarEvaluacion() {
  try {
    const link = document.querySelector('.sidebar-link[data-page="evaluacion"]');
    if (!link) return;
    const title = link.previousElementSibling;

    const evals = await evaluacionApi.getAll().catch(() => []);
    const pendientes = Array.isArray(evals) ? evals.filter(e => e.estado === 'Pendiente') : [];

    if (pendientes.length > 0) {
      link.style.display = 'flex';
      if (title && title.classList.contains('sidebar-section-title')) title.style.display = '';

      let badge = link.querySelector('.eval-pending-badge');
      if (!badge) {
        badge = document.createElement('span');
        badge.className = 'eval-pending-badge';
        badge.style.cssText = 'margin-left:auto;background:#EF4444;color:#FFF;font-size:0.68rem;font-weight:700;padding:2px 7px;border-radius:999px;box-shadow:0 0 8px rgba(239,68,68,0.5);';
        link.appendChild(badge);
      }
      badge.textContent = `${pendientes.length} pendiente${pendientes.length > 1 ? 's' : ''}`;
    } else {
      // Si no tiene evaluaciones pendientes o ya las completó, ocultar del sidebar
      link.style.display = 'none';
      if (title && title.classList.contains('sidebar-section-title') && title.textContent.includes('Desempeño')) {
        title.style.display = 'none';
      }
    }
  } catch {}
}

export async function initAdminLayout(currentPage = '') {
  const isAnalista = AuthService.isAnalista();
  const adminOnlyPages = ['configuracion', 'kiosko', 'usuarios'];
  const allowedRoles = adminOnlyPages.includes(currentPage) ? ['Admin'] : ['Admin', 'Analista'];

  if (!AuthService.requireAuth(allowedRoles)) return;

  if (isAnalista && currentPage === 'dashboard' && !window.location.pathname.includes('/analista/')) {
    window.location.href = AuthService.getDashboardUrl();
    return;
  }

  const sidebarFile = isAnalista ? 'sidebar-analista.html' : 'sidebar-admin.html';
  await loadSidebar(sidebarFile, currentPage);
}

export async function initEmpleadoLayout(currentPage = '') {
  if (!AuthService.requireAuth('Empleado')) return;
  await loadSidebar('sidebar-empleado.html', currentPage);
  await updateEmpleadoSidebarEvaluacion();
}

export async function initAnalistaLayout(currentPage = '') {
  if (!AuthService.requireAuth('Analista')) return;
  await loadSidebar('sidebar-analista.html', currentPage);
}
