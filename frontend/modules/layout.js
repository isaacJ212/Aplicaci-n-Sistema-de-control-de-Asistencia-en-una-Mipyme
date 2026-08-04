/**
 * layout.js — Carga el sidebar compartido e inicializa el layout admin.
 *
 * Live Preview sirve archivos estáticos desde la raíz del workspace.
 * Las rutas absolutas usan /frontend/ como prefijo.
 */
import { AuthService } from './auth.js';
import { toast }       from './utils.js';

/**
 * Inicializa el layout admin: carga el sidebar, marca el link activo,
 * rellena datos del usuario y conecta el botón de logout.
 *
 * @param {string} currentPage  Valor de data-page del link activo (ej: 'empleados')
 */
export async function initAdminLayout(currentPage = '') {
  // 1. Guardia de ruta
  if (!AuthService.requireAuth('Admin')) return;

  // 2. Inyecta el sidebar en su contenedor
  const container = document.getElementById('sidebar-container');
  if (container) {
    try {
      // Ruta absoluta desde la raíz del workspace (funciona con Live Preview)
      const res  = await fetch('/frontend/components/sidebar-admin.html');
      const html = await res.text();
      container.innerHTML = html;
    } catch (e) {
      console.warn('No se pudo cargar el sidebar:', e.message);
    }
  }

  // 3. Marca el link activo DESPUÉS de inyectar el HTML
  if (currentPage) {
    document.querySelector(`.sidebar-link[data-page="${currentPage}"]`)
            ?.classList.add('active');
  }

  // 4. Rellena datos del usuario en el footer del sidebar
  const user = AuthService.getUser();
  if (user) {
    const emailEl  = document.getElementById('sidebar-email');
    const avatarEl = document.getElementById('sidebar-avatar');
    if (emailEl)  emailEl.textContent  = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }

  // 5. Logout (delegado en document para capturarlo después del fetch)
  document.addEventListener('click', (e) => {
    if (e.target.closest('#sidebar-logout-btn')) {
      toast('Cerrando sesión...', 'info', 800);
      setTimeout(() => AuthService.logout(), 600);
    }
  });
}

/**
 * Inicializa el layout del portal del empleado.
 * @param {string} currentPage  Valor de data-page del link activo
 */
export async function initEmpleadoLayout(currentPage = '') {
  if (!AuthService.requireAuth('Empleado')) return;

  const container = document.getElementById('sidebar-container');
  if (container) {
    try {
      const res  = await fetch('/frontend/components/sidebar-empleado.html');
      const html = await res.text();
      container.innerHTML = html;
    } catch (e) {
      console.warn('No se pudo cargar el sidebar del empleado:', e.message);
    }
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
