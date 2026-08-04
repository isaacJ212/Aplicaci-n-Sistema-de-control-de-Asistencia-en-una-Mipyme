/**
 * layout.js — Carga el sidebar compartido e inicializa el layout admin.
 * Llama a initAdminLayout(currentPage) en cada página admin.
 */
import { AuthService } from './auth.js';
import { toast }       from './utils.js';

/**
 * Carga el sidebar HTML, activa el link de la página actual
 * e inicializa los datos del usuario en el footer del sidebar.
 *
 * @param {string} currentPage — coincide con data-page del link activo
 *                               ej: 'empleados', 'planillas', 'dashboard'
 */
export async function initAdminLayout(currentPage = '') {
  // 1. Guardia de ruta
  if (!AuthService.requireAuth('Admin')) return;

  // 2. Carga el sidebar en #sidebar-container
  const container = document.getElementById('sidebar-container');
  if (container) {
    try {
      // Ruta absoluta relativa a la raíz del servidor de desarrollo
      const res  = await fetch('/frontend/components/sidebar-admin.html');
      const html = await res.text();
      container.innerHTML = html;
    } catch {
      console.warn('No se pudo cargar el sidebar.');
    }
  }

  // 3. Marca el link activo
  if (currentPage) {
    const link = document.querySelector(`.sidebar-link[data-page="${currentPage}"]`);
    link?.classList.add('active');
  }

  // 4. Rellena el avatar y email del usuario en el sidebar
  const user = AuthService.getUser();
  if (user) {
    const emailEl  = document.getElementById('sidebar-email');
    const avatarEl = document.getElementById('sidebar-avatar');
    if (emailEl)  emailEl.textContent  = user.email;
    if (avatarEl) avatarEl.textContent = user.email[0].toUpperCase();
  }

  // 5. Logout
  document.addEventListener('click', (e) => {
    if (e.target.closest('#sidebar-logout-btn')) {
      toast('Cerrando sesión...', 'info', 800);
      setTimeout(() => AuthService.logout(), 600);
    }
  });
}
