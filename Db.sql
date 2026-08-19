-- Base de Datos PostgreSQL para Mipyme (Nicaragua)
-- Incluye: Fotos de Perfil (Admin/Empleado), QR Dinámico, GPS, OTP/2FA, Historiales y Leyes NI

-- Tabla: configuracion_sede
CREATE TABLE configuracion_sede (
    id_sede SERIAL PRIMARY KEY,
    nombre_sede VARCHAR(100) DEFAULT 'Sede Principal',
    latitud_sede DECIMAL(10, 8) NOT NULL,
    longitud_sede DECIMAL(11, 8) NOT NULL,
    radio_tolerancia_metros INT DEFAULT 100,
    hora_entrada_oficial TIME DEFAULT '08:00:00',
    hora_salida_oficial TIME DEFAULT '17:00:00',
    duracion_almuerzo_minutos INT DEFAULT 60,
    token_qr_actual VARCHAR(255),
    qr_ultima_actualizacion TIMESTAMP
);

COMMENT ON COLUMN configuracion_sede.latitud_sede IS 'Coordenada GPS de la empresa';
COMMENT ON COLUMN configuracion_sede.longitud_sede IS 'Coordenada GPS de la empresa';
COMMENT ON COLUMN configuracion_sede.radio_tolerancia_metros IS 'Radio permitido en metros para el marcaje';
COMMENT ON COLUMN configuracion_sede.duracion_almuerzo_minutos IS 'Tiempo oficial de descanso';
COMMENT ON COLUMN configuracion_sede.token_qr_actual IS 'Token dinámico del QR expuesto en la pantalla/tablet';
COMMENT ON COLUMN configuracion_sede.qr_ultima_actualizacion IS 'Expira cada 15-30 segundos';

-- Tabla: roles
CREATE TABLE roles (
    id_rol SERIAL PRIMARY KEY,
    nombre_rol VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);

COMMENT ON COLUMN roles.nombre_rol IS 'Admin, Empleado';

-- Tabla: usuarios
CREATE TABLE usuarios (
    id_usuario SERIAL PRIMARY KEY,
    id_rol INT NOT NULL REFERENCES roles(id_rol),
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    secret_2fa VARCHAR(255),
    es_2fa_activo BOOLEAN DEFAULT FALSE,
    estado_activo BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT NOW()
);

COMMENT ON COLUMN usuarios.secret_2fa IS 'Llave secreta para Authenticator App / TOTP';

-- Tabla: empleados
CREATE TABLE empleados (
    id_empleado SERIAL PRIMARY KEY,
    id_usuario INT NOT NULL UNIQUE REFERENCES usuarios(id_usuario),
    cedula_identificacion VARCHAR(20) NOT NULL UNIQUE,
    foto_url VARCHAR(500),
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    cargo_funcion VARCHAR(100) NOT NULL,
    responsabilidades TEXT NOT NULL,
    fecha_contratacion DATE NOT NULL,
    salario_base_mensual DECIMAL(12, 2) NOT NULL,
    dias_vacaciones_acumuladas DECIMAL(5, 2) DEFAULT 0.0
);

COMMENT ON COLUMN empleados.cedula_identificacion IS 'Formato NI: 001-XXXXXX-XXXXX';
COMMENT ON COLUMN empleados.foto_url IS 'URL o ruta de la foto de perfil del colaborador o Admin';
COMMENT ON COLUMN empleados.salario_base_mensual IS 'En Córdobas (NIO)';
COMMENT ON COLUMN empleados.dias_vacaciones_acumuladas IS 'Acumula 2.5 días por mes (Código del Trabajo Ley 185)';

-- Tabla: validaciones_qr_marcaje
CREATE TABLE validaciones_qr_marcaje (
    id_validacion SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    codigo_otp_generado VARCHAR(6) NOT NULL,
    token_qr_escaneado VARCHAR(255) NOT NULL,
    fecha_creacion TIMESTAMP DEFAULT NOW(),
    fecha_expiracion TIMESTAMP NOT NULL,
    fue_utilizado BOOLEAN DEFAULT FALSE,
    intentos_fallidos INT DEFAULT 0
);

COMMENT ON COLUMN validaciones_qr_marcaje.codigo_otp_generado IS 'Código de 6 dígitos enviado/validado';
COMMENT ON COLUMN validaciones_qr_marcaje.fecha_expiracion IS 'Expira en 30 segundos';

-- Tabla: historial_asistencia
CREATE TABLE historial_asistencia (
    id_asistencia SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    fecha DATE NOT NULL,
    hora_entrada TIME NOT NULL,
    inicio_almuerzo TIME,
    fin_almuerzo TIME,
    hora_salida TIME,
    latitud_marcaje DECIMAL(10, 8) NOT NULL,
    longitud_marcaje DECIMAL(11, 8) NOT NULL,
    distancia_calculada_metros DECIMAL(8, 2) NOT NULL,
    estado_asistencia VARCHAR(20) NOT NULL,
    minutos_tardanza INT DEFAULT 0,
    esta_dentro_del_rango_gps BOOLEAN DEFAULT TRUE
);

COMMENT ON COLUMN historial_asistencia.estado_asistencia IS 'A Tiempo, Tardanza, Ausente';

-- Tabla: horas_extras
CREATE TABLE horas_extras (
    id_hora_extra SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    id_usuario_aprobador INT REFERENCES usuarios(id_usuario),
    fecha DATE NOT NULL,
    cantidad_horas DECIMAL(4, 2) NOT NULL,
    motivo TEXT NOT NULL,
    monto_pagar DECIMAL(12, 2) NOT NULL,
    estado VARCHAR(20) DEFAULT 'Aprobado'
);

COMMENT ON COLUMN horas_extras.monto_pagar IS 'Cálculo al 100% adicional según Código del Trabajo Arto. 62';
COMMENT ON COLUMN horas_extras.estado IS 'Pendiente, Aprobado, Rechazado';

-- Tabla: historial_permisos_vacaciones
CREATE TABLE historial_permisos_vacaciones (
    id_solicitud SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    id_usuario_aprobador INT REFERENCES usuarios(id_usuario),
    tipo_solicitud VARCHAR(30) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    dias_solicitados DECIMAL(4, 1) NOT NULL,
    motivo TEXT NOT NULL,
    estado_solicitud VARCHAR(20) DEFAULT 'Pendiente',
    fecha_respuesta TIMESTAMP
);

COMMENT ON COLUMN historial_permisos_vacaciones.id_usuario_aprobador IS 'Usuario Admin que aprobó/rechazó';
COMMENT ON COLUMN historial_permisos_vacaciones.tipo_solicitud IS 'Vacaciones, Permiso Medico, Permiso Personal';
COMMENT ON COLUMN historial_permisos_vacaciones.estado_solicitud IS 'Pendiente, Aprobado, Rechazado';

-- Tabla: historial_planillas
CREATE TABLE historial_planillas (
    id_planilla SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    periodo_mes_anio VARCHAR(7) NOT NULL,
    salario_base DECIMAL(12, 2) NOT NULL,
    total_horas_extras DECIMAL(5, 2) DEFAULT 0.0,
    pago_horas_extras DECIMAL(12, 2) DEFAULT 0.0,
    salario_bruto DECIMAL(12, 2) NOT NULL,
    inss_laboral DECIMAL(12, 2) NOT NULL,
    ir_laboral DECIMAL(12, 2) NOT NULL,
    otras_deducciones DECIMAL(12, 2) DEFAULT 0.0,
    total_deducciones DECIMAL(12, 2) NOT NULL,
    salario_neto DECIMAL(12, 2) NOT NULL,
    acumulado_aguinaldo DECIMAL(12, 2) NOT NULL,
    fecha_emision DATE NOT NULL
);

COMMENT ON COLUMN historial_planillas.periodo_mes_anio IS 'Format: YYYY-MM';
COMMENT ON COLUMN historial_planillas.inss_laboral IS '7% del Salario Bruto';
COMMENT ON COLUMN historial_planillas.ir_laboral IS 'IR según tarifa Art. 52 Ley 822 (LCT)';
COMMENT ON COLUMN historial_planillas.acumulado_aguinaldo IS '8.33% mensual';

-- Tabla: evaluaciones_desempeno
CREATE TABLE evaluaciones_desempeno (
    id_evaluacion SERIAL PRIMARY KEY,
    id_empleado INT NOT NULL REFERENCES empleados(id_empleado),
    id_evaluador INT NOT NULL REFERENCES usuarios(id_usuario),
    periodo VARCHAR(20) NOT NULL,
    porcentaje_puntualidad DECIMAL(5, 2),
    calificacion_cumplimiento_funciones INT,
    observaciones TEXT,
    fecha_evaluacion DATE DEFAULT NOW()
);

COMMENT ON COLUMN evaluaciones_desempeno.periodo IS 'Ej. Agosto 2026, Q3-2026';
COMMENT ON COLUMN evaluaciones_desempeno.porcentaje_puntualidad IS '% Calculado automáticamente desde historial_asistencia';
COMMENT ON COLUMN evaluaciones_desempeno.calificacion_cumplimiento_funciones IS 'Escala 1 a 5 según responsabilidades del empleado';



-- Índices para búsquedas frecuentes
CREATE INDEX idx_usuarios_email ON usuarios(email);
CREATE INDEX idx_empleados_cedula ON empleados(cedula_identificacion);
CREATE INDEX idx_historial_asistencia_empleado_fecha ON historial_asistencia(id_empleado, fecha);
CREATE INDEX idx_historial_asistencia_estado ON historial_asistencia(estado_asistencia);
CREATE INDEX idx_validaciones_qr_empleado ON validaciones_qr_marcaje(id_empleado);
CREATE INDEX idx_validaciones_qr_token ON validaciones_qr_marcaje(token_qr_escaneado);
CREATE INDEX idx_permisos_vacaciones_empleado ON historial_permisos_vacaciones(id_empleado, fecha_inicio);
CREATE INDEX idx_planillas_periodo ON historial_planillas(periodo_mes_anio);
CREATE INDEX idx_horas_extras_fecha ON horas_extras(fecha);



-- Vista para resumen de asistencia mensual
CREATE VIEW vw_resumen_asistencia_mensual AS
SELECT 
    e.id_empleado,
    e.nombres || ' ' || e.apellidos AS nombre_completo,
    EXTRACT(YEAR FROM ha.fecha) AS anio,
    EXTRACT(MONTH FROM ha.fecha) AS mes,
    COUNT(*) AS total_dias,
    SUM(CASE WHEN estado_asistencia = 'A Tiempo' THEN 1 ELSE 0 END) AS dias_a_tiempo,
    SUM(CASE WHEN estado_asistencia = 'Tardanza' THEN 1 ELSE 0 END) AS dias_tardanza,
    SUM(minutos_tardanza) AS total_minutos_tardanza
FROM empleados e
JOIN historial_asistencia ha ON e.id_empleado = ha.id_empleado
GROUP BY e.id_empleado, e.nombres, e.apellidos, EXTRACT(YEAR FROM ha.fecha), EXTRACT(MONTH FROM ha.fecha);

-- Vista para cálculo de vacaciones disponibles
CREATE VIEW vw_vacaciones_disponibles AS
SELECT 
    e.id_empleado,
    e.nombres || ' ' || e.apellidos AS nombre_completo,
    e.fecha_contratacion,
    EXTRACT(YEAR FROM AGE(NOW(), e.fecha_contratacion)) * 2.5 + 
    EXTRACT(MONTH FROM AGE(NOW(), e.fecha_contratacion)) * 2.5 / 12 AS vacaciones_acumuladas_teoricas,
    e.dias_vacaciones_acumuladas AS vacaciones_consumidas,
    (EXTRACT(YEAR FROM AGE(NOW(), e.fecha_contratacion)) * 2.5 + 
     EXTRACT(MONTH FROM AGE(NOW(), e.fecha_contratacion)) * 2.5 / 12) - e.dias_vacaciones_acumuladas AS vacaciones_disponibles
FROM empleados e;