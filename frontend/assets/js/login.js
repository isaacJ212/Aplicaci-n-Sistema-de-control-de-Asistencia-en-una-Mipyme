/**
 * login.js — Lógica del formulario de inicio de sesión.
 *
 * USA RUTAS RELATIVAS: ../../modules/
 * Desde assets/js/, dos niveles arriba llega a frontend/modules/
 * Funciona con cualquier servidor HTTP (puerto 3000, 5500, Live Preview, etc.)
 */

import { AuthService } from '../../modules/auth.js';
import { authApi }     from '../../modules/api.js';
import { $, show, hide, isValidEmail, toast } from '../../modules/utils.js';
import CONFIG          from '../../modules/config.js';
import { getDashboardUrl } from '../../modules/routes.js';

// ── Si ya hay sesión activa redirige de inmediato ─────────────────────────────
AuthService.redirectIfAuthenticated();

// ── Referencias al DOM ────────────────────────────────────────────────────────
const form       = $('#login-form');
const emailInput = $('#email');
const passInput  = $('#password');
const submitBtn  = $('#submit-btn');
const btnText    = $('#btn-text');
const errorBox   = $('#error-box');
const errorMsg   = $('#error-msg');
const emailError = $('#email-error');
const passError  = $('#password-error');
const togglePass = $('#toggle-password');
const eyeIcon    = $('#eye-icon');

// ── Mostrar/ocultar contraseña ────────────────────────────────────────────────
const EYE_OPEN = `
  <path d="M10 12a2 2 0 100-4 2 2 0 000 4z"/>
  <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd"/>
`;
const EYE_OFF = `
  <path fill-rule="evenodd" d="M3.28 2.22a.75.75 0 00-1.06 1.06l14.5 14.5a.75.75 0 101.06-1.06l-1.745-1.745a10.029 10.029 0 003.3-4.38 1.651 1.651 0 000-1.185A10.004 10.004 0 009.999 3a9.956 9.956 0 00-4.744 1.194L3.28 2.22z" clip-rule="evenodd"/>
  <path d="M10.748 13.93l2.523 2.524a10.065 10.065 0 01-5.271 0l-1.07-1.07A10.05 10.05 0 013.5 13.5a10.004 10.004 0 01-2.034-2.893 1.651 1.651 0 010-1.214A10.003 10.003 0 015 5.5l2.23 2.23a4 4 0 005.518 6.2z"/>
`;

togglePass.addEventListener('click', () => {
  const isText  = passInput.type === 'text';
  passInput.type    = isText ? 'password' : 'text';
  eyeIcon.innerHTML = isText ? EYE_OPEN : EYE_OFF;
  togglePass.title  = isText ? 'Mostrar contraseña' : 'Ocultar contraseña';
});

emailInput.addEventListener('input', () => {
  emailInput.classList.remove('error');
  hide(emailError);
  hide(errorBox);
});
passInput.addEventListener('input', () => {
  passInput.classList.remove('error');
  hide(passError);
  hide(errorBox);
});

// ── Helpers ───────────────────────────────────────────────────────────────────
function setLoading(loading) {
  submitBtn.disabled = loading;
  if (loading) {
    btnText.textContent = 'Verificando...';
    if (!submitBtn.querySelector('.spinner')) {
      const sp = document.createElement('div');
      sp.className = 'spinner';
      submitBtn.prepend(sp);
    }
  } else {
    btnText.textContent = 'Iniciar sesión';
    submitBtn.querySelector('.spinner')?.remove();
  }
}

function showError(message) {
  errorMsg.textContent = message;
  show(errorBox);
  errorBox.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

function validate() {
  let ok = true;
  const email = emailInput.value.trim();
  const pass  = passInput.value;

  if (!email) {
    emailInput.classList.add('error');
    emailError.textContent = 'El correo es obligatorio.';
    show(emailError);
    ok = false;
  } else if (!isValidEmail(email)) {
    emailInput.classList.add('error');
    emailError.textContent = 'Ingresa un correo válido.';
    show(emailError);
    ok = false;
  }

  if (!pass) {
    passInput.classList.add('error');
    passError.textContent = 'La contraseña es obligatoria.';
    show(passError);
    ok = false;
  }
  return ok;
}

// ── Submit ────────────────────────────────────────────────────────────────────
form.addEventListener('submit', async (e) => {
  e.preventDefault();
  hide(errorBox);
  if (!validate()) return;

  const email    = emailInput.value.trim().toLowerCase();
  const password = passInput.value;

  setLoading(true);

  try {
    const data = await authApi.login({ email, password });
    AuthService.saveSession(data);
    toast('¡Bienvenido! Redirigiendo...', 'success', 1500);

    const dest = getDashboardUrl(data.role);
    setTimeout(() => { window.location.href = dest; }, 800);

  } catch (err) {
    setLoading(false);
    const code = err.statusCode ?? 0;

    if (code === 401) {
      showError('Correo o contraseña incorrectos. Verifica tus datos.');
    } else if (code === 422) {
      const errores = err.data?.errores ?? [];
      showError(errores.length ? errores.join(' ') : err.message);
    } else if (code === 0 || !navigator.onLine) {
      showError(
        'No se pudo conectar al servidor. Verifica que el backend esté activo en: ' +
        CONFIG.API_BASE_URL
      );
    } else {
      showError(err.message || 'Error inesperado. Intenta de nuevo.');
    }
  }
});

// ── Focus al cargar ───────────────────────────────────────────────────────────
emailInput.focus();
