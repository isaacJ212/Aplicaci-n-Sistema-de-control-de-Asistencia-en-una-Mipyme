/**
 * layout.js — Carga el sidebar compartido e inicializa el layout admin o empleado.
 *
 * ESTRATEGIA DE RUTAS:
 *   Los sidebars tienen data-nav="pages/admin/dashboard.html" (ruta relativa a frontend/).
 *   usando resolvePageUrl() de routes.js, compatible con rutas cortas y /frontend/.
 */

import { AuthService } from './auth.js';
import { toast }       from './utils.js';
import { getBase, resolvePageUrl } from './routes.js';

/**
 * Carga el sidebar HTML, resuelve los data-nav a hrefs absolutos,
 * marca el link activo e inicializa usuario + logout.
 */
async function loadSidebar(htmlFile, currentPage) {
  const container = document.getElementById('sidebar-container');
  if (!container) return;

  // 1. Fetch del sidebar HTML
  try {
    const base = getBase();
    const res  = await fetch(`${base}/components/${htmlFile}`);
    if (!res.ok) throw new Error(`HTTP ${res.status} al cargar ${htmlFile}`);
    container.innerHTML = await res.text();
  } catch (e) {
    console.warn('No se pudo cargar el sidebar:', e.message);
    return;
  }

  // 2. Resolver data-nav → href correcto para TODOS los links del sidebar
  container.querySelectorAll('[data-nav]').forEach(el => {
    const navPath = el.getAttribute('data-nav');
    if (navPath) {
      el.setAttribute('href', resolvePageUrl(navPath));
    }
  });

  // 3. Marcar link activo según la página actual
  if (currentPage) {
    container.querySelector(`.sidebar-link[data-page="${currentPage}"]`)
             ?.classList.add('active');
  }

  // 4. Rellenar datos del usuario en el footer del sidebar
  const user = AuthService.getUser();
  if (user) {
    const emailEl  = container.querySelector('#sidebar-email');
    const avatarEl = container.querySelector('#sidebar-avatar');
    if (emailEl)  emailEl.textContent  = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }

  // 5. Botón de logout
  document.addEventListener('click', (e) => {
    if (e.target.closest('#sidebar-logout-btn')) {
      toast('Cerrando sesión...', 'info', 800);
      setTimeout(() => AuthService.logout(), 600);
    }
  });

  // 6. Menú móvil para el portal empleado
  if (htmlFile === 'sidebar-empleado.html') {
    const topbar = document.querySelector('.topbar');
    const sidebar = container.querySelector('.sidebar-emp');
    if (topbar && sidebar && !topbar.querySelector('.mobile-menu-toggle')) {
      const toggle = document.createElement('button');
      toggle.type = 'button';
      toggle.className = 'mobile-menu-toggle';
      toggle.setAttribute('aria-label', 'Abrir menú');
      toggle.innerHTML = `
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M4 6h16M4 12h16M4 18h16" />
        </svg>`;
      topbar.prepend(toggle);

      toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
      document.addEventListener('click', (event) => {
        if (window.innerWidth <= 1024 && sidebar.classList.contains('open') &&
            !event.target.closest('.sidebar-emp') &&
            !event.target.closest('.mobile-menu-toggle')) {
          sidebar.classList.remove('open');
        }
      });
      window.addEventListener('resize', () => {
        if (window.innerWidth > 1024) sidebar.classList.remove('open');
      });
    }
  }
}

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
