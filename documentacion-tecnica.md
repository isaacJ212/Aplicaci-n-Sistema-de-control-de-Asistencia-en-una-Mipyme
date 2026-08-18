# Documentación Técnica - Sistema de Control de Asistencia y Nómina para Mipymes

## 1. Información general del proyecto

Este proyecto corresponde a un sistema de gestión de asistencia laboral y nómina orientado a micro, pequeñas y medianas empresas con un máximo de 10 empleados. Su objetivo es automatizar el registro de asistencias, validar la ubicación geográfica del empleado mediante GPS, gestionar solicitudes de permisos y vacaciones, y calcular la nómina con deducciones legales.

La solución está diseñada con enfoque académico para la asignatura de Proyecto de TI II, con una estructura basada en arquitectura limpia, separación de responsabilidades y uso de una base de datos relacional.

### Datos académicos
- Asignatura: Proyecto de TI II
- Maestro: Álvaro Molina
- Fecha de inicio estimada: 29/07/2026
- Fecha límite de entrega: 05/08/2026

> Nota: el README indica la entrega como 5 de Agosto de 2026. En la práctica, la línea de tiempo del proyecto se entiende como inicio el 29/07/2026 y cierre estimado el 05/08/2026.

---

## 2. Propósito de la aplicación

El propósito principal es digitalizar la gestión del recurso humano en una mipyme. La app busca:

- registrar la asistencia diaria de cada empleado;
- validar que el marcaje se haga dentro del rango geográfico permitido;
- evitar fraudes con autenticación segura y validación por 2FA;
- controlar días de vacaciones, permisos y horarios; 
- calcular planillas con deducciones legales y pagos por horas extras;
- generar reportes para la administración.

El sistema está pensado para reemplazar procesos manuales o dispersos (listas en Excel, fichas físicas, cálculos a mano) por una plataforma centralizada.

---

## 3. Alcance del sistema

### 3.1 Alcance funcional

El sistema contempla los siguientes módulos:

1. Autenticación y seguridad
   - login con email y contraseña;
   - autenticación de dos pasos con TOTP/Authenticator;
   - protección de endpoints con JWT;
   - roles de administrador y empleado.

2. Administración de empleados
   - alta de empleados;
   - datos personales, cargo y salario base;
   - foto de perfil;
   - asignación de usuario y rol.

3. Control de asistencia
   - registro de entrada, descanso y salida;
   - validación de ubicación por GPS;
   - cálculo de tardanzas y estado de asistencia;
   - registro histórico de marcajes.

4. Permisos y vacaciones
   - solicitud de vacaciones;
   - solicitud de permisos médicos o personales;
   - aprobación o rechazo por parte del administrador.

5. Planilla y nómina
   - cálculo de salario bruto;
   - deducciones de INSS e IR;
   - horas extras;
   - salario neto y aguinaldo.

6. Reportes y evaluación
   - resumen mensual de asistencia;
   - desempeño y puntualidad;
   - exportación a Excel.

### 3.2 Alcance limitado

El proyecto está orientado específicamente a:

- mipymes de hasta 10 empleados;
- una sola sede o una ubicación principal;
- control de personal con reglas de asistencia y nómina básicas;
- entorno académico y de demostración, no necesariamente industrial.

### 3.3 Limitaciones

- No está diseñado para múltiples sucursales complejas;
- no incluye integración con módulos contables avanzados;
- la lógica de nómina se basa en reglas locales y puede requerir ajustes según legislación vigente;
- la gestión de roles es básica, diseñada para admin y empleado;
- la parte frontend todavía no se observa desarrollada de forma completa en este repositorio.

---

## 4. Cómo funciona la aplicación

### 4.1 Flujo de autenticación

El flujo principal es el siguiente:

1. El usuario ingresa su correo y contraseña.
2. El backend valida las credenciales.
3. Si son correctas, exige verificación en dos pasos con un código TOTP.
4. Una vez validado, se emite un token JWT.
5. El token se usa para acceder a los módulos protegidos.

Esto garantiza que el acceso se haga únicamente con usuarios autorizados y con una segunda capa de seguridad.

### 4.2 Flujo de asistencia

1. El empleado se ubica en la zona de trabajo.
2. El sistema obtiene la latitud y longitud del marcaje.
3. El backend compara la ubicación con la coordenada de la sede.
4. Se calcula la distancia mediante la fórmula de Haversine.
5. Si la ubicación está dentro del radio permitido, el marcaje se registra como válido.
6. Si está fuera del rango, se rechaza o se registra como no válido.
7. Dependiendo del horario oficial, el sistema determina si fue puntual o tardanza.

### 4.3 Flujo de vacaciones y permisos

1. El empleado solicita vacaciones, permiso médico o permiso personal.
2. El administrador revisa los datos y la disponibilidad.
3. Se aprueba, rechaza o queda pendiente.
4. La respuesta queda registrada con fecha y responsable.
5. El sistema puede calcular días acumulados y disponibles.

### 4.4 Flujo de nómina

1. El sistema toma el salario base del empleado.
2. Suma horas extras aprobadas.
3. Calcula bonos y deducciones conforme a la normativa.
4. Aplica INSS y otros cálculos pertinentes.
5. Genera el salario neto y almacena el registro en la planilla.

---

## 5. Flujos principales de la aplicación

### Flujo A: login y acceso al sistema

Usuario -> Frontend -> API -> Base de datos -> Validación 2FA -> Token JWT -> Acceso autorizado.

### Flujo B: marcaje de asistencia

Empleado -> Captura GPS -> API -> validación de distancia -> comparación con horario -> registro en historial_asistencia.

### Flujo C: aprobación de vacaciones

Empleado -> Solicitud -> Administrador -> Revisión -> Aprobación/Rechazo -> Historial actualizado.

### Flujo D: generación de nómina

Administrador -> Selecciona periodo -> Consulta asistencia/horasextras -> Cálculo de salario -> Registro en historial_planillas -> Download/consulta/reporte.

### Flujo E: reportes de desempeño

Sistema -> Reúne asistencia, tardanzas y evaluaciones -> Genera indicadores -> Muestra métricas para análisis administrativo.

---

## 6. Estructura técnica del proyecto

El repositorio organiza la solución en capas, siguiendo el enfoque de arquitectura limpia.

### 6.1 Estructura del repositorio

- README.md: descripción general del proyecto.
- contexto.md: contexto operativo y técnico del desarrollo.
- Db.sql: modelado inicial de la base de datos.
- backend/
  - MipymeAsistencia.sln
  - src/
    - MipymeAsistencia.Domain
    - MipymeAsistencia.Application
    - MipymeAsistencia.Infrastructure
    - MipymeAsistencia.WebApi
- frontend/
  - carpeta pendiente o en proceso de implementación.

### 6.2 Capas propuestas

#### Domain
- Entidades del negocio.
- Enums y eventos.
- Reglas centrales sin dependencia de infraestructura.

#### Application
- Casos de uso.
- DTOs.
- validadores.
- lógica de negocio mediatizada o modularizada.

#### Infrastructure
- DbContext.
- Entity Framework Core.
- repositorios.
- autenticación e identidad.
- servicios de exportación o cálculo.

#### WebApi
- Controllers.
- Middlewares.
- Program.cs.
- endpoints REST.

---

## 7. Instalación y configuración

### Requisitos previos

- .NET 8 SDK
- PostgreSQL o servicio compatible (Supabase/Neon)
- Git
- Editor o IDE compatible con C#
- Cliente para pruebas de API (Postman, Swagger o similar)

### Pasos de instalación

1. Clonar el repositorio.
2. Abrir la solución en Visual Studio o VS Code.
3. Restaurar paquetes NuGet.
4. Configurar la conexión a la base de datos en appsettings.json o variables de entorno.
5. Ejecutar las migraciones con Entity Framework.
6. Iniciar la API.
7. Verificar Swagger en la URL local.

### Ejemplo de configuración esperada

Se debe definir una cadena de conexión con el host, base de datos, usuario y contraseña del motor PostgreSQL. Además, se requieren datos de JWT y configuración de seguridad para 2FA.

---

## 8. Tecnologías utilizadas

### Backend
- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT (Json Web Token)
- Swagger / OpenAPI
- FluentValidation

### Base de datos
- PostgreSQL
- esquema relacional con tablas para usuarios, empleados, asistencia, vacaciones, planillas y evaluaciones.

### Infraestructura/servicios
- Supabase o PostgreSQL en la nube
- exportación de reportes (Excel)
- geolocalización y cálculo de distancia GPS

### Frontend
- aún no definido de forma clara en el repositorio, pero se considera un cliente que consumirá la API REST.

---

## 9. Funciones principales del sistema

- Registro de usuarios y roles.
- Login con 2FA.
- Registro de empleados con fotos y cargos.
- Control de entrada, salida y descansos.
- Validación por GPS.
- Evaluación de tardanzas.
- Solicitudes de vacaciones y permisos.
- Cálculo automático de horas extras.
- Cálculo de planilla con deducciones.
- Reportes de asistencia y desempeño.
- Generación de archivos o reportes descargables.

---

## 10. Base de datos analizada

La base de datos en Db.sql define varias tablas clave, entre ellas:

- configuracion_sede
- roles
- usuarios
- empleados
- validaciones_qr_marcaje
- historial_asistencia
- horas_extras
- historial_permisos_vacaciones
- historial_planillas
- evaluaciones_desempeno

También incluye índices y vistas como:
- vw_resumen_asistencia_mensual
- vw_vacaciones_disponibles

Esto evidencia que la aplicación no es solo un módulo de asistencia, sino un sistema completo de administración de personal y nómina.

---

## 11. Modelo de negocio inferido

El sistema implementa un modelo de negocio orientado a:

- control del horario laboral;
- cumplimiento de normas locales;
- auditoría de asistencia diaria;
- cumplimiento de seguridad mediante verificación de identidad;
- automatización de procesos financieros y operativos.

El dominio refleja una organización pequeña con recursos limitados y necesidad de supervisión administrativa centralizada.

---

## 12. Estado actual del proyecto

El repositorio ya presenta una base sólida:

- el README define objetivo, stack y arquitectura;
- la base de datos ya está modelada en SQL;
- la estructura de capas está preparada;
- el proyecto backend tiene una solución y un proyecto WebApi inicial.

Sin embargo, todavía requiere completar la implementación real de los módulos, migraciones, servicios, repositorios, controladores y endpoints funcionales.

---

## 13. Conclusión

Este proyecto responde a una necesidad real de gestión operativa en pequeñas empresas: controlar asistencia, asegurar seguridad del acceso, automatizar nóminas y reducir la carga administrativa. La implementación propuesta combina arquitectura profesional, enfoque empresarial y alta pertinencia académica.

El desarrollo se encuadra dentro de la asignatura de Proyecto de TI II y representa una solución aplicada de ingeniería de software con enfoque en backend, seguridad, base de datos y automatización de procesos.

---

## 14. Resumen ejecutivo

El sistema tiene como finalidad apoyar a empresas pequeñas en la administración de personal mediante una aplicación web/API que centraliza:

- autenticación segura;
- control de marcaje por horario y ubicación;
- gestión de vacaciones y permisos;
- cálculo de nómina;
- generación de reportes.

En conjunto, se busca crear una solución útil, ordenada, escalable y adecuada para una entrega académica con objetivos profesionales reales.
