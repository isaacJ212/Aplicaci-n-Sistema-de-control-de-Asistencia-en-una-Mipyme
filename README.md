# 🏢 Sistema Integral de Control de Asistencia, Evaluaciones 360° y Nómina para Mipyme

Sistema empresarial integral de gestión de recursos humanos, marcaje de asistencia geolocalizado, evaluaciones de desempeño 360°, seguridad 2FA a estación de trabajo y cálculo de nómina legal según la legislación laboral nicaragüense (INSS 7%, IR Ley 822).

---

## 🌟 Características Principales

### 👥 1. Arquitectura Multi-Portal (3 Roles)
* **👑 Administrador (`/admin/dashboard`):** Control total de la empresa, sedes, parámetros laborales, cierre de períodos y expedientes.
* **📊 Analista de Recursos Humanos (`/analista/dashboard`):** Aplicación masiva de evaluaciones 360°, revisión de asistencia, pre-planillas y resolución de solicitudes.
* **👷 Empleado (`/empleado/dashboard`):** Marcaje de jornada con QR y GPS, respuesta a evaluaciones asignadas, horas extras y recibos de pago.

### 📊 2. Motor de Evaluaciones de Desempeño 360°
* **20 Preguntas Ponderadas (100% total):** Escala Likert de 1 a 5 con cálculo automático mediante la fórmula:  
  $$\text{Puntaje Final} = \sum_{i=1}^{20} \left(\frac{\text{Calificación}_i}{5} \times \text{Peso}_i\right)$$
* **Asignación Semestral Masiva:** Botón `🚀 Aplicar a todos los empleados` para generar las evaluaciones de todo el personal con un solo clic.
* **Sidebar Dinámico:** La opción aparece en el portal del empleado al ser asignada y se **auto-oculta** en cuanto completa su evaluación.
* **Histórico Semestral:** Filtros y métricas de semestres anteriores (`2025-S1`, `2025-S2`, `2026-S1`, `2026-S2`).

### 🔐 3. Seguridad y Doble Factor (2FA a Estación de Trabajo)
* **Verificación de Estación:** Envío de código de seguridad de 6 dígitos mediante notificación de escritorio a la computadora física del usuario.
* **Kiosko QR Dinámico:** Pantalla de marcaje con rotación periódica de tokens y validación de radio geográfico GPS.

### 📜 4. Auditoría y Trazabilidad de Expedientes
* Historial inmutable de eventos (`auditoria_logs`) con línea de tiempo visual en el expediente de cada empleado (altas, modificaciones salariales, promociones y evaluaciones).

---

## 📚 Documentación y Diagramas de Flujo

Toda la documentación técnica detallada, casos de uso y diagramas de flujo interactivos (Mermaid) se encuentran en la carpeta [`docs/`](./docs/):

👉 **[Ver Manual de Arquitectura, Flujos y Casos de Uso](./docs/SISTEMA_Y_CASOS_DE_USO.md)**

---

## 🛠️ Stack Tecnológico

| Capa | Tecnologías |
|---|---|
| **Backend** | .NET 8 Web API, C#, Clean Architecture + CQRS (MediatR), FluentValidation |
| **Base de Datos** | PostgreSQL (Neon Cloud) + Entity Framework Core |
| **Frontend** | HTML5, CSS3 Custom Properties (Design System moderno), Vanilla JavaScript Modular |
| **Servidor Frontend** | Node.js + Express (`server.js`) |
| **Seguridad & 2FA** | JWT Bearer, BCrypt, Web Notification API, SignalR |
| **Almacenamiento** | Supabase Storage para fotos de perfil y comprobantes |

---

## 🚀 Puesta en Marcha Rápida

### 1. Iniciar Frontend
```bash
npm run frontend
# Acceder a: http://localhost:3000
```

### 2. Iniciar Backend
```bash
cd backend/src/MipymeAsistencia.WebApi
dotnet run
# Swagger API: http://localhost:5000/swagger
```

---

## 🔑 Credenciales de Acceso para Pruebas

| Rol | Correo | Contraseña | Portal |
|---|---|---|---|
| **👑 Administrador** | `isaac@gmail.com` | `123456789` | `/admin/dashboard` |
| **📊 Analista RRHH** | `analista@mipyme.com` | `123456789` | `/analista/dashboard` |
| **👷 Empleado** | `master@gmail.com` | `123456789` | `/empleado/dashboard` |
