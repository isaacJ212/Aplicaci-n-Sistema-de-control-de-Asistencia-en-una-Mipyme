# 🏢 Sistema de Control de Asistencia y Nómina para Mipyme

Sistema de gestión de recursos humanos y marcaje de asistencia con geolocalización, diseñado para mipymes. Incluye administración de empleados, control de jornadas, solicitudes de vacaciones, manejo de horas extras, generación de planillas y opciones de marcaje GPS/QR.

---

## 📅 Información del Proyecto
* **Fecha límite de entrega:** 5 de Agosto de 2026
* **Tipo de proyecto:** Evaluación aplicada de backend + frontend
* **Objetivo:** Portal administrativo y portal empleado para micro y pequeñas empresas
* **Asignatura:** Proyecto de TI II
* **Docente:** Álvaro Molina

---

## 🛠️ Stack Tecnológico
* **Backend:** C# / .NET 8 Web API
* **Frontend:** HTML / CSS / JavaScript
* **Servidor frontend:** Express (`server.js`)
* **Base de Datos:** PostgreSQL
* **Autenticación:** ASP.NET Core Identity + JWT
* **Arquitectura backend:** Clean Architecture + CQRS
* **Validación:** FluentValidation
* **Contenedores:** Docker / Docker Compose

---

## 🧱 Estructura del Proyecto

```text
.
├── backend/
│   ├── Dockerfile
│   ├── MipymeAsistencia.sln
│   └── src/
│       ├── MipymeAsistencia.Application/
│       ├── MipymeAsistencia.Domain/
│       ├── MipymeAsistencia.Infrastructure/
│       └── MipymeAsistencia.WebApi/
├── frontend/
│   ├── assets/
│   ├── components/
│   ├── modules/
│   ├── pages/
│   └── ...
├── docker-compose.yml
├── package.json
├── server.js
└── README.md
```

### Backend
* `backend/src/MipymeAsistencia.WebApi/`: API REST del servicio
* `backend/src/MipymeAsistencia.Application/`: lógica de negocio, comandos y queries
* `backend/src/MipymeAsistencia.Infrastructure/`: EF Core, Identity, repositorios y servicios
* `backend/src/MipymeAsistencia.Domain/`: entidades del dominio, enums y reglas de negocio

### Frontend
* `frontend/pages/empleado/`: portal empleado
* `frontend/pages/admin/`: portal administrador
* `frontend/pages/auth/`: login
* `frontend/pages/`: kiosko QR y páginas públicas
* `frontend/modules/`: lógica JS compartida y API wrappers
* `frontend/assets/css/`: estilos globales y específicos

---

## 🚀 Funcionalidades principales
* Portal administrador con gestión de empleados, planillas, solicitudes y rendimiento.
* Portal empleado con marcaje GPS/QR, historial de asistencia, horas extras, solicitudes y nómina.
* Kiosko QR público para marcaje desde recepción o punto fijo.
* UI responsive con experiencia móvil mejorada para empleados.
* Backend RESTful con autenticación JWT y roles `Admin` / `Empleado`.

---

## 📌 Reglas de negocio destacadas
### Autenticación y 2FA
* El usuario se autentica con `Email` + `Password`.
* Tras validar, se emite un token JWT con el rol del usuario.

### Control de asistencia
* Registro de marcajes: `Entrada`, `InicioAlmuerzo`, `FinAlmuerzo`, `Salida`.
* El cliente envía latitud/longitud para validar cercanía a la sede.
* Se calcula distancia con la fórmula de Haversine.
* El sistema determina estados como `A Tiempo`, `Tardanza` o `Ausente`.

### Nómina y horas extras
* Se calcula ingreso bruto, deducciones y salario neto.
* Las horas extras aprobadas se suman al pago final.
* La planilla muestra el último recibo y los totales relevantes.

### Solicitudes y vacaciones
* Empleados envían solicitudes de vacaciones y permisos.
* Los administradores aprueban o rechazan solicitudes.
* Se gestiona inventario de días disponibles y días tomados.

---

## 🛠️ Ejecución del proyecto
### Requisitos previos
* .NET 8 SDK instalado
* Node.js instalado
* PostgreSQL disponible
* Docker (opcional)

### Arrancar frontend local
```bash
cd "/home/hack4/Documentos/proyecto de TI/AsistenciaControlPyme"
npm install
npm run frontend
```

Abrir en el navegador:
* `http://localhost:3000/login`
* `http://localhost:3000/empleado/dashboard`
* `http://localhost:3000/admin/dashboard`
* `http://localhost:3000/kiosko-qr`

### Arrancar backend local
```bash
cd "/home/hack4/Documentos/proyecto de TI/AsistenciaControlPyme/backend"
dotnet restore
dotnet build
dotnet run --project src/MipymeAsistencia.WebApi/MipymeAsistencia.WebApi.csproj
```

### Iniciar con Docker Compose
```bash
docker compose up --build
```

El backend expone el puerto:
* `http://localhost:8080`

### Configuración de la API en el frontend
* Revisa `frontend/modules/config.js`
* Asegúrate de que `API_BASE_URL` apunte al backend activo

---

## 🌐 Rutas del frontend
* `/login` → Pantalla de inicio de sesión
* `/admin/dashboard` → Panel administrativo
* `/empleado/dashboard` → Panel de empleado
* `/empleado/marcaje` → Marcaje GPS / QR
* `/kiosko-qr` → Kiosko QR público

---

## 📄 Archivos clave
* `server.js` — servidor Express de frontend
* `frontend/modules/config.js` — base URL de la API
* `backend/Dockerfile` — construcción del backend
* `docker-compose.yml` — despliegue Docker
* `backend/MipymeAsistencia.sln` — solución .NET

## link del deploy 

https://aplicaci-n-sistema-de-control-de-74ff.onrender.com/pages/admin/rendimiento.html
https://aplicaci-n-sistema-de-control-de.onrender.com/swagger/index.html


