/**
 * layout.js — Carga el sidebar compartido e inicializa el layout admin o empleado.
 *
 * Usa rutas relativas calculadas dinámicamente desde window.location
 * para funcionar en cualquier servidor (puerto 3000, 5500, etc.).
 */
import { AuthService } from './auth.js';
import { toast }       from './utils.js';

/** Devuelve el path base hasta la carpeta frontend/ */
function getBase() {
  const path = window.location.pathname;
  const idx  = path.indexOf('/frontend');
  if (idx !== -1) return path.slice(0, idx + '/frontend'.length);
  return '';
}

async function loadSidebar(htmlFile, currentPage) {
  const container = document.getElementById('sidebar-container');
  if (!container) return;
  try {
    const base = getBase();
    const res  = await fetch(`${base}/components/${htmlFile}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    container.innerHTML = await res.text();
  } catch (e) {
    console.warn('No se pudo cargar el sidebar:', e.message);
  }

  if (currentPage) {
    document.querySelector(`.sidebar-link[data-page="${currentPage}"]`)
            ?.classList.add('active');
  }

  const user = AuthService.getUser();
  if (user) {
    const emailEl  = document.getElementById('sidebar-email');
    const avatarEl = document.getElementById('sidebar-avatar');
    if (emailEl)  emailEl.textContent  = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }

  document.addEventListener('click', (e) => {
    if (e.target.closest('#sidebar-logout-btn')) {
      toast('Cerrando sesión...', 'info', 800);
      setTimeout(() => AuthService.logout(), 600);
    }
  });
}

export async function initAdminLayout(currentPage = '') {
  if (!AuthService.requireAuth('Admin')) return;
  await loadSidebar('sidebar-admin.html', currentPage);
}

export async function initEmpleadoLayout(currentPage = '') {
  if (!AuthService.requireAuth('Empleado')) return;
  await loadSidebar('sidebar-empleado.html', currentPage);
}
