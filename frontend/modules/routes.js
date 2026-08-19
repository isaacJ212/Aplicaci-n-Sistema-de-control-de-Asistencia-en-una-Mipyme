export function usesFrontendPrefix() {
  return window.location.pathname.includes('/frontend');
}


export function getBase() {
  const path = window.location.pathname;
  const idx  = path.indexOf('/frontend');
  return idx !== -1
    ? window.location.origin + path.slice(0, idx + '/frontend'.length)
    : window.location.origin;
}



export function resolvePageUrl(relativePath) {
  const base = getBase();

  if (usesFrontendPrefix()) {
    return `${base}/${relativePath}`;
  }

  const match = relativePath.match(/^pages\/(admin|analista|empleado|auth)\/(.+)\.html$/);
  if (!match) return `${base}/${relativePath}`;

  const [, section, page] = match;
  if (section === 'auth' && page === 'login') return `${base}/login`;
  return `${base}/${section}/${page}`;
}

export function getLoginUrl() {
  return resolvePageUrl('pages/auth/login.html');
}

export function getDashboardUrl(role) {
  if (role === 'Admin') return resolvePageUrl('pages/admin/dashboard.html');
  if (role === 'Analista') return resolvePageUrl('pages/analista/dashboard.html');
  return resolvePageUrl('pages/empleado/dashboard.html');
}