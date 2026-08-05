

import { AuthService } from './auth.js';
import { toast }       from './utils.js';
import { getBase, resolvePageUrl } from './routes.js';


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
