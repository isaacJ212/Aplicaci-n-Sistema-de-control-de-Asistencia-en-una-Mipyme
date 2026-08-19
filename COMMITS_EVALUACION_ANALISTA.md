# COMMITS — Portal Analista RRHH + Evaluaciones 360°

> **Rama:** `refactor/backend/Planilla`  
> **Última actualización:** 2026-08-19

---

## Commit 1 — `243df7a`
**Mensaje:** `feat: portal Analista RRHH + fix fallback connection string para Render`

### Archivos Modificados / Creados

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `backend/src/MipymeAsistencia.Infrastructure/DependencyInjection/DependencyInjection.cs` | MODIFY | Agrega fallback de connection string cuando la variable de entorno está vacía (fix Render) |
| `frontend/modules/auth.js` | MODIFY | Soporte 3 roles (Admin/Analista/Empleado). `requireAuth()` acepta array de roles. `getDashboardUrl()` redirige al portal correcto según rol |
| `frontend/modules/layout.js` | MODIFY | Agrega `initAnalistaLayout()` para rutas del portal Analista |
| `frontend/components/sidebar-analista.html` | NEW | Sidebar morado del portal Analista RRHH con links: Dashboard, Informes, Aprobaciones, Evaluaciones 360°, Pre-Planilla, Empleados |
| `frontend/pages/analista/dashboard.html` | NEW | Dashboard RRHH con estadísticas de evaluaciones y accesos rápidos |
| `frontend/pages/analista/evaluacion.html` | NEW | Panel de Gestión de Evaluaciones 360°: asignar formularios, ver 20 preguntas en preview, filtrar evaluaciones, ver resultados detallados |
| `server.js` | MODIFY | Agrega `ANALISTA_PAGES` array y rutas `/analista/dashboard`, `/analista/evaluacion` |

---

## Commit 2 — `251114c`
**Mensaje:** `feat: portal empleado evaluacion 360 + sidebar analista + roles`

### Archivos Modificados / Creados

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `frontend/pages/empleado/evaluacion.html` | NEW/MODIFY | Página completa con 20 preguntas Likert (1-5), barra de progreso, envío y vista de resultado con puntaje ponderado |
| `frontend/pages/admin/evaluacion.html` | NEW/MODIFY | Panel admin para crear/asignar evaluaciones y ver resultados |
| `server.js` | MODIFY | Agrega rutas `/empleado/evaluacion` y `/admin/evaluacion` |

---

## Commit 3 — `f2d3439`
**Mensaje:** `feat: módulo Evaluación 360° backend completo (entities, commands, queries, controller)`

### Archivos Backend Creados

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `backend/src/MipymeAsistencia.Domain/Entities/EvaluacionDesempeno.cs` | NEW | Entidad cabecera de evaluación 360° |
| `backend/src/MipymeAsistencia.Domain/Entities/EvaluacionRespuesta.cs` | NEW | Entidad para cada respuesta Likert (1-5) por pregunta |
| `backend/src/MipymeAsistencia.Domain/Services/Evaluacion360Preguntas.cs` | NEW | 20 preguntas ponderadas (100% total) con fórmula Σ(Cal_i/5 × Peso_i) |
| `backend/src/.../Features/Evaluacion/Commands/CrearEvaluacion/` | NEW | CQRS Command para crear formulario de evaluación |
| `backend/src/.../Features/Evaluacion/Commands/ResponderEvaluacion/` | NEW | CQRS Command para responder y calcular puntaje |
| `backend/src/.../Features/Evaluacion/Queries/GetEvaluaciones/` | NEW | CQRS Query para listar evaluaciones con filtros |
| `backend/src/.../Features/Evaluacion/Queries/GetEvaluacionById/` | NEW | CQRS Query para detalle de una evaluación |
| `backend/src/.../Features/Evaluacion/Queries/GetPreguntas/` | NEW | CQRS Query para obtener las 20 preguntas del formulario |
| `backend/src/MipymeAsistencia.WebApi/Controllers/EvaluacionController.cs` | NEW | API REST con endpoints: GET /preguntas, GET /, GET /{id}, POST /, PUT /{id}/responder |

---

## Base de Datos — Cambios Aplicados Directamente

| Cambio | Descripción |
|--------|-------------|
| `INSERT INTO roles (3, 'Analista', ...)` | Rol Analista RRHH creado |
| `INSERT INTO usuarios (analista@mipyme.com, id_rol=3)` | Usuario Analista semilla |
| Tablas `evaluaciones_desempeno`, `evaluaciones_respuestas` | Migración aplicada en sesión anterior |

---

## Credenciales de Prueba

| Rol | Email | Contraseña | Portal |
|-----|-------|-----------|--------|
| **Admin** | `isaac@gmail.com` | `123456789` | `/admin/dashboard` |
| **Analista RRHH** | `analista@mipyme.com` | `123456789` | `/analista/dashboard` |
| **Empleado** | `master@gmail.com` | *(ver BD)* | `/empleado/dashboard` |

---

## Flujo de Evaluaciones 360° Implementado

```
Analista RRHH
  → /analista/evaluacion
  → Clic "Asignar evaluación"
  → Selecciona: Empleado + Perspectiva + Evaluador + Período
  → Sistema crea EvaluacionDesempeno (Estado: Pendiente)
      ↓
Empleado (Evaluador)
  → /empleado/evaluacion
  → Ve formulario en la tabla de "Mis Evaluaciones"
  → Clic "Responder"
  → Responde 20 preguntas Likert (1-5)
  → Sistema calcula: Puntaje = Σ(Cal_i/5 × Peso_i) [0-100%]
  → Estado cambia a "Completada"
      ↓
Analista RRHH
  → Ve puntaje final y detalle pregunta a pregunta en modal
```
