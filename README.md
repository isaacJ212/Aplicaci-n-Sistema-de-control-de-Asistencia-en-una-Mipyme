
# 🏢 Sistema de Control de Asistencia y Nómina para Mipyme

Sistema de gestión de recursos humanos y marcaje de asistencia con geolocalización, diseñado para micro, pequeñas y medianas empresas (hasta 10 empleados). La solución permite la administración de usuarios, control de jornadas laborales, solicitudes de vacaciones, cálculo automático de nómina con deducciones de ley y generación de reportes.

---

## 📅 Información del Proyecto
* **Fecha límite de entrega:** 5 de Agosto de 2026
* **Tipo de proyecto:** Evaluación exploratoria y aplicada de desarrollo backend/fullstack.
* **Capacidad Objetivo:** Mipymes de hasta 10 empleados.
* **Compomente:** Proyecto de TI ll.
* **Maestro:** Álvaro Molina.



---

## 🛠️ Stack Tecnológico

* **Lenguaje & Framework:** C# / .NET 8 (Web API RESTful)
* **ORM:** Entity Framework Core 8
* **Base de Datos:** PostgreSQL en la nube (Desplegado en **Supabase**)
* **Autenticación & Seguridad:** ASP.NET Core Identity + JWT (JSON Web Tokens) + 2FA (TOTP / Authenticator App)
* **Arquitectura:** Clean Architecture (Arquitectura Limpia) + Patrón CQRS (Commands & Queries)
* **Validación & Librerías:** FluentValidation, EPPlus / ClosedXML (para exportación de reportes a Excel).

---

## 🏛️ Patrón de Diseño y Arquitectura Backend

El backend sigue los principios de **Clean Architecture** (desacoplamiento en 4 capas) combinada con el patrón **CQRS (Command Query Responsibility Segregation)** para separar las operaciones de lectura y escritura.


```

┌─────────────────────────────────────────────────────────────┐
│               1. WebApi (Presentation Layer)                │
│       • REST Controllers   • Middlewares   • JWT Auth       │
└──────────────────────────────┬──────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│            2. Application Layer (CQRS / BLL)                │
│       • Commands & Queries   • DTOs   • Validators          │
└──────────────────────────────┬──────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│             3. Infrastructure Layer (DAL)                   │
│   • EF Core DbContext   • PostgreSQL   • Identity 2FA       │
└──────────────────────────────┬──────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│               4. Domain Layer (Core Entities)               │
│       • Entities   • Enums   • Value Objects                │
└──────────────────────────────┴──────────────────────────────┘

```

### Justificación de la Arquitectura
1. **Clean Architecture:** Permite que las reglas de negocio (Dominio) no dependan del framework web ni del motor de base de datos.
2. **CQRS:** Facilita la legibilidad y mantenimiento al aislar la lógica de escritura (ej. registrar marcaje o procesar planilla) de las lecturas simples (ej. listar empleados).
3. **Escalabilidad:** Aunque el sistema inicie con 10 empleados, la estructura está preparada para escalar a cientos de usuarios sin refactorizaciones complejas.

---

## 📂 Estructura de Carpetas del Proyecto

```text
.
├── docs
└── src
    ├── MipymeAsistencia.Domain/              # Capa de Dominio (Entidades puras sin dependencias)
    │   ├── Common/                           # Entidades base y Value Objects (ej. CoordenadasGPS)
    │   ├── Entities/                         # Usuario, Empleado, Marcaje, Planilla, Vacacion
    │   ├── Enums/                            # EstadoAsistencia, RolUsuario, EstadoSolicitud
    │   └── Events/                           # Eventos del dominio
    │
    ├── MipymeAsistencia.Application/         # Lógica de Aplicación y Casos de Uso
    │   ├── Common/
    │   │   ├── DTOs/                         # Data Transfer Objects por módulo
    │   │   └── Interfaces/                   # Interfaces (IApplicationDbContext, IIdentityService)
    │   ├── DependencyInjection/              # Registro de servicios de aplicación
    │   ├── Features/                         # Módulos organizados por CQRS (Commands & Queries)
    │   │   ├── Asistencia/
    │   │   ├── Auth/
    │   │   ├── Empleados/
    │   │   ├── Planilla/
    │   │   └── Vacaciones/
    │   ├── Helpers/                          # Cálculo de fórmulas (Haversine GPS, Deducciones Ley)
    │   └── Validators/                       # Validaciones de entrada con FluentValidation
    │
    ├── MipymeAsistencia.Infrastructure/      # Persistencia, Base de Datos y Servicios Externos
    │   ├── DependencyInjection/              # Inyección de EF Core, Identity y Repositorios
    │   ├── Identity/                         # Implementación de ASP.NET Identity + 2FA TOTP
    │   ├── Persistence/                      # DbContext, Mapeos (Configurations) y Migraciones
    │   ├── Repositories/                     # Implementación de Repositorios / Unit of Work
    │   └── Services/                         # Servicio de exportación a Excel y Geometría GPS
    │
    └── MipymeAsistencia.WebApi/              # Punto de entrada HTTP (Presentación)
        ├── Controllers/                      # Endpoints REST (Auth, Asistencia, Planilla, etc.)
        ├── Middlewares/                      # Manejo global de excepciones y validación JWT
        └── Properties/                       # Configuraciones de lanzamiento (launchSettings.json)

```

---

## ⚡ Reglas de Negocio Clave

### 1. Autenticación y Verificación en Dos Pasos (2FA)

* El usuario ingresa credenciales básicas (`Email` + `Password`).
* Si las credenciales son válidas, el backend exige la verificación del código TOTP de 6 dígitos (Google Authenticator / Microsoft Authenticator).
* Al validar el 2FA, se expide un token **JWT** con los *claims* del rol (`Admin` o `Empleado`).

### 2. Control de Asistencia y Geolocalización (GPS)

* El empleado realiza marcajes de: **Entrada**, **Inicio/Fin de Descanso (Almuerzo)** y **Salida**.
* La petición debe enviar la latitud y longitud capturadas por el dispositivo cliente.
* El backend utiliza la **Fórmula de Haversine** para calcular la distancia en metros entre el punto de marcaje y la ubicación de la sucursal asignada.
* Si el marcaje está dentro del radio permitido (ej. < 100m), se registra la asistencia. Si no, se rechaza.
* Evaluador de estado automático: `A Tiempo` o `Tardanza` según el horario de entrada contratado.

### 3. Planilla de Salario y Deducciones de Ley

* **Salario Bruto:** Salario base mensual + pago de horas extras autorizadas.
* **Seguro Social:** Cálculo porcentual correspondiente al aporte laboral obligatorio.
* **Impuesto sobre la Renta (IR):** Aplicación de la tabla progresiva impositiva según norma legal vigente.
* **Salario Neto:**




### 4. Gestión de Vacaciones

* Acumulación automática de días según antigüedad (basado en la `FechaDeContratacion`).
* Flujo de solicitud por parte del empleado y aprobación/rechazo por el rol Administrador.

---

## 🚀 Guía de Inicio Rápido para Desarrolladores 

### Requisitos Previos

* SDK de **.NET 8** instalado.
* Instancia de **PostgreSQL** (Neon.tech o Supabase).

### Configuración del archivo `appsettings.json` (`WebApi`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=TU_NEON_HOST;Database=mipyme_asistencia;Username=TU_USUARIO;Password=TU_PASSWORD;SSL Mode=Require;"
  },
  "JwtSettings": {
    "Secret": "TU_CLAVE_SECRETA_SUPER_SEGURA_DE_32_CARACTERES_MINIMO",
    "Issuer": "MipymeAsistenciaApi",
    "Audience": "MipymeAsistenciaClients",
    "ExpiryMinutes": 120
  }
}

```

### Comandos Principales (.NET CLI)

```bash
# Restaurar dependencias
dotnet restore

# Compilar la solución
dotnet build

# Aplicar migraciones a PostgreSQL
dotnet ef database update --project src/MipymeAsistencia.Infrastructure --startup-project src/MipymeAsistencia.WebApi

# Ejecutar el proyecto
dotnet run --project src/MipymeAsistencia.WebApi

```


