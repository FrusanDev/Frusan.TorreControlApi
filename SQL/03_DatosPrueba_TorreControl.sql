-- ============================================================
-- Torre de Control — AGR-000 Plataforma Habilitante
-- Script 03: Datos iniciales (TC_TipoAlerta) + datos de prueba
-- ============================================================
-- NOTA: LOG-003 (Antigüedad fruta embalada) fue reclasificado
-- como "foto diaria" — no requiere gestión ni cierre.
-- Se excluye de este catálogo. Si se decide mostrarlo en el
-- dashboard diferenciado, se puede agregar con un flag especial.
-- ============================================================


-- ------------------------------------------------------------
-- Catálogo de tipos de alerta (datos reales de producción)
-- Fuente: reunión 23/06/2026 — Max Walther / Eduardo Fabres
-- ------------------------------------------------------------
SET IDENTITY_INSERT dbo.TC_TipoAlerta ON;

INSERT INTO dbo.TC_TipoAlerta (IdTipoAlerta, Codigo, Nombre, Area, Activo) VALUES
(1,  'AGR-001', 'Desviación calibre recepción vs proceso',           'AGR', 1),
(2,  'AGR-003', 'Desviación volumen diario pronosticado',             'AGR', 1),
(3,  'LOG-002', 'Atraso ETA contenedores',                            'LOG', 1),
(4,  'LOG-004', 'Cambios instructivo de embalaje (≥2do del día)',     'LOG', 1),
(5,  'LOG-005', 'Desviación código embalaje cargado',                 'LOG', 1),
(6,  'CAL-001', 'Bloqueo calidad recepción',                          'CAL', 1),
(7,  'CAL-003', 'Bloqueos y PNC final (SAP)',                         'CAL', 1),
(8,  'CAL-004', 'Fruta bloqueada con reserva Stock activa',           'CAL', 1),
(9,  'IND-005', 'Rechazo inspección SAG',                             'IND', 1);

SET IDENTITY_INSERT dbo.TC_TipoAlerta OFF;


-- ------------------------------------------------------------
-- Responsables de prueba (ajustar con nombres/teléfonos reales)
-- Formato teléfono: +56 + 9 dígitos
-- ------------------------------------------------------------
INSERT INTO dbo.TC_Responsable (IdTipoAlerta, Nombre, Telefono, Email, Activo) VALUES
-- AGR-001 Calibre
(1, 'Daniel Responsable', '+56912345001', 'daniel@frusan.cl', 1),

-- AGR-003 Volumen
(2, 'Eduardo Responsable', '+56912345002', 'eduardo@frusan.cl', 1),

-- LOG-002 ETA contenedores
(3, 'Daniel Responsable', '+56912345001', 'daniel@frusan.cl', 1),

-- LOG-004 Instructivo embalaje
(4, 'Hector Responsable', '+56912345003', 'hector@frusan.cl', 1),

-- LOG-005 Código embalaje
(5, 'Hector Responsable', '+56912345003', 'hector@frusan.cl', 1),

-- CAL-001 Bloqueo calidad
(6, 'Daniel Responsable', '+56912345001', 'daniel@frusan.cl', 1),

-- CAL-003 Bloqueos SAP
(7, 'Responsable SAP', '+56912345004', 'sap@frusan.cl', 1),

-- CAL-004 Fruta bloqueada con reserva
(8, 'Responsable SAP', '+56912345004', 'sap@frusan.cl', 1),

-- IND-005 SAG
(9, 'Responsable SAP', '+56912345004', 'sap@frusan.cl', 1);


-- ------------------------------------------------------------
-- Evento de prueba para verificar el flujo completo
-- (eliminar antes de ir a producción)
-- ------------------------------------------------------------
INSERT INTO dbo.TC_Evento (IdTipoAlerta, Payload, Estado, FechaOcurrencia, OrigenSistema)
VALUES (
    1,
    '{"lote":"L-2026-001","plantaId":1,"madLin":0.15,"madPond":0.18,"especie":"Uva"}',
    'Pendiente',
    GETDATE(),
    'Test'
);


-- Verificación
SELECT 'TC_TipoAlerta' AS Tabla, COUNT(*) AS Registros FROM dbo.TC_TipoAlerta
UNION ALL
SELECT 'TC_Responsable', COUNT(*) FROM dbo.TC_Responsable
UNION ALL
SELECT 'TC_Evento', COUNT(*) FROM dbo.TC_Evento;


PRINT '>> Script 03 ejecutado: 9 tipos de alerta, responsables de prueba y 1 evento de prueba insertados.';
