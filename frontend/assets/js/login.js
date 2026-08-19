import { AuthService } from '../../modules/auth.js';
import { authApi } from '../../modules/api.js';
import { getDashboardUrl } from '../../modules/routes.js';

let pendingLogin = null;

export function initLoginApp({ CONFIG, utils, AuthService: authService, authApi: authApiModule }) {
  const { $, show, hide, isValidEmail, toast } = utils;

  const form = $('#login-form');
  const verifyForm = $('#verify-2fa-form');
  const emailInput = $('#email');
  const passInput = $('#password');
  const submitBtn = $('#submit-btn');
  const btnText = $('#btn-text');
  const errorBox = $('#error-box');
  const errorMsg = $('#error-msg');
  const emailError = $('#email-error');
  const passError = $('#password-error');
  const togglePass = $('#toggle-password');
  const eyeIcon = $('#eye-icon');
  const verifyCodeInput = $('#verification-code');
  const verifyBtn = $('#verify-btn');
  const verifyBtnText = $('#verify-btn-text');
  const verificationCodeError = $('#verification-code-error');
  const cancel2faBtn = $('#cancel-2fa-btn');

  if (!form || !emailInput || !passInput || !submitBtn || !errorBox || !togglePass || !eyeIcon) {
    return;
  }

  const EYE_OPEN = `
    <path d="M10 12a2 2 0 100-4 2 2 0 000 4z"/>
    <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd"/>
  `;
  const EYE_OFF = `
    <path fill-rule="evenodd" d="M3.28 2.22a.75.75 0 00-1.06 1.06l14.5 14.5a.75.75 0 101.06-1.06l-1.745-1.745a10.029 10.029 0 003.3-4.38 1.651 1.651 0 000-1.185A10.004 10.004 0 009.999 3a9.956 9.956 0 00-4.744 1.194L3.28 2.22z" clip-rule="evenodd"/>
    <path d="M10.748 13.93l2.523 2.524a10.065 10.065 0 01-5.271 0l-1.07-1.07A10.05 10.05 0 013.5 13.5a10.004 10.004 0 01-2.034-2.893 1.651 1.651 0 010-1.214A10.003 10.003 0 015 5.5l2.23 2.23a4 4 0 005.518 6.2z"/>
  `;

  function setFormMode(mode) {
    const isVerify = mode === 'verify';
    form.classList.toggle('hidden', isVerify);
    if (verifyForm) verifyForm.classList.toggle('hidden', !isVerify);
  }

  function setLoading(loading, target = 'login') {
    if (target === 'verify') {
      verifyBtn.disabled = loading;
      verifyBtnText.textContent = loading ? 'Verificando...' : 'Verificar código';
      return;
    }

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

  function validateLogin() {
    let ok = true;
    const email = emailInput.value.trim();
    const pass = passInput.value;

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

  function validate2Fa() {
    const code = verifyCodeInput.value.trim();
    if (!/^[0-9]{6}$/.test(code)) {
      verifyCodeInput.classList.add('error');
      verificationCodeError.textContent = 'Ingresa un código de 6 dígitos.';
      show(verificationCodeError);
      return false;
    }
    return true;
  }

  togglePass.addEventListener('click', () => {
    const isText = passInput.type === 'text';
    passInput.type = isText ? 'password' : 'text';
    eyeIcon.innerHTML = isText ? EYE_OPEN : EYE_OFF;
    togglePass.title = isText ? 'Mostrar contraseña' : 'Ocultar contraseña';
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

  verifyCodeInput.addEventListener('input', () => {
    verifyCodeInput.classList.remove('error');
    hide(verificationCodeError);
    hide(errorBox);
  });

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hide(errorBox);
    if (!validateLogin()) return;

    const email = emailInput.value.trim().toLowerCase();
    const password = passInput.value;
    setLoading(true);

    try {
      const data = await authApiModule.login({ email, password });

      if (data?.requires2Fa) {
        pendingLogin = { email, password };
        setFormMode('verify');
        if (data.codigo2FaSoloPruebas) {
          toast(`Código 2FA de estación: ${data.codigo2FaSoloPruebas}`, 'info', 6000);
        } else {
          toast('Se ha enviado un código a su estación de trabajo.', 'info', 3000);
        }
        verifyCodeInput.focus();
        setLoading(false);
        return;
      }

      authService.saveSession(data);
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
        showError('No se pudo conectar al servidor. Verifica que el backend esté activo en: ' + CONFIG.API_BASE_URL);
      } else {
        showError(err.message || 'Error inesperado. Intenta de nuevo.');
      }
    }
  });

  if (verifyForm) {
    verifyForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      hide(errorBox);
      if (!validate2Fa() || !pendingLogin) return;

      const code = verifyCodeInput.value.trim();
      setLoading(true, 'verify');

      try {
        const data = await authApiModule.verify2fa({
          email: pendingLogin.email,
          code,
        });

        authService.saveSession(data);
        toast('Código validado correctamente.', 'success', 1500);
        const dest = getDashboardUrl(data.role);
        setTimeout(() => { window.location.href = dest; }, 800);
      } catch (err) {
        setLoading(false, 'verify');
        showError(err.message || 'Código inválido o expirado.');
      }
    });

    cancel2faBtn?.addEventListener('click', () => {
      pendingLogin = null;
      verifyCodeInput.value = '';
      hide(verificationCodeError);
      setFormMode('login');
      emailInput.focus();
    });
  }

  authService.redirectIfAuthenticated();
  emailInput.focus();
}

export { AuthService, authApi };
