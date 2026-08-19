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

-- ══════════════════════════════════════════════════════════════════════════════
-- TABLAS DE DÍAS FERIADOS Y CONFIGURACIÓN LABORAL / TABLA IR (LEY 185 Y LEY 822)
-- ══════════════════════════════════════════════════════════════════════════════

-- Tabla: dias_feriados
CREATE TABLE IF NOT EXISTS dias_feriados (
    id_dia_feriado SERIAL PRIMARY KEY,
    fecha DATE NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255),
    es_recuperable BOOLEAN DEFAULT TRUE,
    es_movil BOOLEAN DEFAULT FALSE
);

COMMENT ON TABLE dias_feriados IS 'Días feriados oficiales y locales para cálculo de pago doble y omisión de tardanzas';

-- Tabla: parametros_laborales
CREATE TABLE IF NOT EXISTS parametros_laborales (
    id_parametro SERIAL PRIMARY KEY,
    clave VARCHAR(50) NOT NULL UNIQUE,
    valor DECIMAL(10, 4) NOT NULL,
    descripcion VARCHAR(255) NOT NULL,
    fecha_modificacion TIMESTAMP DEFAULT NOW()
);

COMMENT ON TABLE parametros_laborales IS 'Parámetros dinámicos de nómina: INSS Laboral/Patronal, INATEC, Horas Mes, etc.';

-- Tabla: tabla_impuesto_renta
CREATE TABLE IF NOT EXISTS tabla_impuesto_renta (
    id_tabla_ir SERIAL PRIMARY KEY,
    desde_monto_anual DECIMAL(14, 2) NOT NULL,
    hasta_monto_anual DECIMAL(14, 2),
    porcentaje_aplicable DECIMAL(5, 4) NOT NULL,
    monto_base_exceso DECIMAL(14, 2) NOT NULL,
    cuota_fija DECIMAL(14, 2) NOT NULL,
    anio_vigencia INT DEFAULT 2026,
    activo BOOLEAN DEFAULT TRUE
);

COMMENT ON TABLE tabla_impuesto_renta IS 'Tramos progresivos del Impuesto sobre la Renta (IR) Ley 822 LCT Nicaragua';

-- Seeds iniciales: Feriados 2026 Nicaragua
INSERT INTO dias_feriados (fecha, nombre, descripcion, es_recuperable, es_movil) VALUES
('2026-01-01', 'Año Nuevo', 'Feriado Nacional Obligatorio', TRUE, FALSE),
('2026-04-02', 'Jueves Santo', 'Semana Santa', TRUE, TRUE),
('2026-04-03', 'Viernes Santo', 'Semana Santa', TRUE, TRUE),
('2026-05-01', 'Día Internacional de los Trabajadores', 'Feriado Nacional Obligatorio', TRUE, FALSE),
('2026-07-19', 'Día de la Revolución', 'Feriado Nacional', TRUE, FALSE),
('2026-08-01', 'Santo Domingo de Guzmán (Bajada)', 'Feriado Local Managua', TRUE, FALSE),
('2026-08-10', 'Santo Domingo de Guzmán (Dejada)', 'Feriado Local Managua', TRUE, FALSE),
('2026-09-14', 'Batalla de San Jacinto', 'Fiestas Patrias', TRUE, FALSE),
('2026-09-15', 'Día de la Independencia de Centroamérica', 'Fiestas Patrias', TRUE, FALSE),
('2026-12-08', 'Día de la Inmaculada Concepción de María', 'Feriado Nacional', TRUE, FALSE),
('2026-12-25', 'Navidad', 'Feriado Nacional Obligatorio', TRUE, FALSE)
ON CONFLICT (fecha) DO NOTHING;

-- Seeds iniciales: Parámetros Laborales Nicaragua
INSERT INTO parametros_laborales (clave, valor, descripcion) VALUES
('INSS_LABORAL', 7.00, 'Aporte INSS laboral del empleado (%)'),
('INSS_PATRONAL', 21.50, 'Aporte INSS patronal de la empresa (%)'),
('INATEC', 2.00, 'Aporte INATEC patronal (%)'),
('HORAS_LABORALES_MES', 240.00, 'Horas laborales mensuales promedio para cálculo de horas extras y tardanzas'),
('TASA_PRESTACIONES_MENSUAL', 2.50, 'Días de provisión mensual para Aguinaldo, Vacaciones e Indemnización')
ON CONFLICT (clave) DO NOTHING;

-- Seeds iniciales: Tabla Progresiva IR 2026 (Ley 822 LCT)
INSERT INTO tabla_impuesto_renta (desde_monto_anual, hasta_monto_anual, porcentaje_aplicable, monto_base_exceso, cuota_fija, anio_vigencia, activo) VALUES
(0.00, 100000.00, 0.00, 0.00, 0.00, 2026, TRUE),
(100000.01, 200000.00, 0.15, 100000.00, 0.00, 2026, TRUE),
(200000.01, 350000.00, 0.20, 200000.00, 15000.00, 2026, TRUE),
(350000.01, 500000.00, 0.25, 350000.00, 45000.00, 2026, TRUE),
(500000.01, NULL, 0.30, 500000.00, 82500.00, 2026, TRUE);

-- ══════════════════════════════════════════════════════════════════════════════
-- TABLAS DE CIERRE DE PERIODOS DE PLANILLA Y SINCRONIZACIÓN BIOMÉTRICA
-- ══════════════════════════════════════════════════════════════════════════════

-- Tabla: periodos_cierre_planilla
CREATE TABLE IF NOT EXISTS periodos_cierre_planilla (
    id_periodo_cierre SERIAL PRIMARY KEY,
    periodo VARCHAR(7) NOT NULL UNIQUE,
    fecha_corte_horas_extras TIMESTAMP NOT NULL,
    fecha_emision_planilla TIMESTAMP NOT NULL,
    cerrado BOOLEAN DEFAULT FALSE,
    fecha_cierre_definitivo TIMESTAMP,
    id_usuario_cierre INT REFERENCES usuarios(id_usuario) ON DELETE SET NULL,
    observaciones TEXT
);

COMMENT ON TABLE periodos_cierre_planilla IS 'Fechas de corte de horas extras y control de cierre de nómina mensual';

-- Tabla: dispositivos_biometricos
CREATE TABLE IF NOT EXISTS dispositivos_biometricos (
    id_dispositivo SERIAL PRIMARY KEY,
    nombre_dispositivo VARCHAR(100) NOT NULL,
    direccion_ip VARCHAR(50) NOT NULL,
    puerto INT DEFAULT 4370,
    tipo_protocolo VARCHAR(50) DEFAULT 'ZKTeco_Standalone',
    ubicacion VARCHAR(150),
    clave_comunicacion VARCHAR(100),
    activo BOOLEAN DEFAULT TRUE,
    ultima_sincronizacion TIMESTAMP,
    estado_conexion VARCHAR(30) DEFAULT 'Desconectado'
);

COMMENT ON TABLE dispositivos_biometricos IS 'Relojes marcadores físicos (ZKTeco, Hikvision, etc.) en red TCP/IP';

-- Tabla: registros_marcajes_biometricos
CREATE TABLE IF NOT EXISTS registros_marcajes_biometricos (
    id_registro_biometrico SERIAL PRIMARY KEY,
    id_dispositivo INT NOT NULL REFERENCES dispositivos_biometricos(id_dispositivo) ON DELETE CASCADE,
    numero_enrollamiento VARCHAR(50) NOT NULL,
    fecha_hora TIMESTAMP NOT NULL,
    tipo_marcaje INT DEFAULT 0,
    tipo_verificacion VARCHAR(30) DEFAULT 'Huella',
    procesado BOOLEAN DEFAULT FALSE,
    fecha_procesado TIMESTAMP,
    id_asistencia_generada INT REFERENCES historial_asistencia(id_asistencia) ON DELETE SET NULL,
    error_procesamiento TEXT
);

COMMENT ON TABLE registros_marcajes_biometricos IS 'Auditoría en crudo de marcajes descargados o ingestados del hardware';
CREATE INDEX IF NOT EXISTS idx_biometrico_disp_enroll_fecha ON registros_marcajes_biometricos(id_dispositivo, numero_enrollamiento, fecha_hora);

-- Seeds iniciales: Periodos de cierre
INSERT INTO periodos_cierre_planilla (periodo, fecha_corte_horas_extras, fecha_emision_planilla, cerrado, fecha_cierre_definitivo, observaciones) VALUES
('2026-05', '2026-05-25 23:59:59', '2026-05-30 00:00:00', TRUE, '2026-05-30 18:00:00', 'Cierre de planilla Mayo 2026 (Rubí del Valle)'),
('2026-08', '2026-08-25 23:59:59', '2026-08-30 00:00:00', FALSE, NULL, 'Periodo activo Agosto 2026')
ON CONFLICT (periodo) DO NOTHING;

-- Seeds iniciales: Dispositivos Biométricos
INSERT INTO dispositivos_biometricos (nombre_dispositivo, direccion_ip, puerto, tipo_protocolo, ubicacion, activo, estado_conexion) VALUES
('Reloj Marcador Principal (Recepción)', '192.168.1.201', 4370, 'ZKTeco_Standalone', 'Entrada Principal / Recepción', TRUE, 'Conectado'),
('Reloj Marcador Taller / Bodega', '192.168.1.202', 4370, 'ZKTeco_Standalone', 'Acceso Bodega', TRUE, 'Desconectado');

-- ══════════════════════════════════════════════════════════════════════════════
-- TIPOS DE SOLICITUD CONFIGURABLES Y VALIDACIÓN DE ESTACIÓN DE TRABAJO
-- ══════════════════════════════════════════════════════════════════════════════

-- Columnas de validación de estación en configuracion_sede y usuarios
ALTER TABLE configuracion_sede ADD COLUMN IF NOT EXISTS ip_estacion_permitida VARCHAR(255) DEFAULT '127.0.0.1,::1,192.168.1.0/24';
ALTER TABLE configuracion_sede ADD COLUMN IF NOT EXISTS validar_ip_en_2fa BOOLEAN DEFAULT TRUE;

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ultima_ip_login VARCHAR(60);
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ultima_mac_login VARCHAR(50);
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ultima_fecha_login TIMESTAMP;

-- Tabla: tipos_solicitud_permiso
CREATE TABLE IF NOT EXISTS tipos_solicitud_permiso (
    id_tipo_solicitud SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    requiere_comprobante BOOLEAN DEFAULT FALSE,
    descuenta_vacaciones BOOLEAN DEFAULT FALSE,
    permite_por_horas BOOLEAN DEFAULT TRUE,
    maximo_dias_por_solicitud INT,
    icono VARCHAR(50) DEFAULT 'calendar',
    activo BOOLEAN DEFAULT TRUE
);

COMMENT ON TABLE tipos_solicitud_permiso IS 'Catálogo configurable de permisos, ausencias, licencias y vacaciones';

-- Seeds iniciales: Tipos de Solicitud de Permiso
INSERT INTO tipos_solicitud_permiso (nombre, descripcion, requiere_comprobante, descuenta_vacaciones, permite_por_horas, maximo_dias_por_solicitud, icono, activo) VALUES
('Vacaciones', 'Días de descanso anual remunerado con cargo al saldo acumulado', FALSE, TRUE, FALSE, 30, 'beach_access', TRUE),
('Permiso Médico', 'Incapacidad médica o cita médica justificada', TRUE, FALSE, TRUE, 15, 'local_hospital', TRUE),
('Permiso Personal', 'Asuntos personales o trámites administrativos', FALSE, FALSE, TRUE, 3, 'person', TRUE),
('Duelo / Calamidad', 'Fallecimiento de familiar directo o calamidad doméstica (Arto. 73 Código del Trabajo)', TRUE, FALSE, FALSE, 5, 'favorite', TRUE),
('Licencia de Estudio', 'Permiso por exámenes o capacitaciones laborales autorizadas', TRUE, FALSE, TRUE, 7, 'school', TRUE)
ON CONFLICT (nombre) DO NOTHING;