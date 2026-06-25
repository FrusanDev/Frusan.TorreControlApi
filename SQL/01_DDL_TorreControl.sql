-- ============================================================
-- Torre de Control — AGR-000 Plataforma Habilitante
-- Script 01: Creación de tablas e índices
-- Base de datos: BDFRUSAN (o la que defina el equipo)
-- ============================================================

-- ------------------------------------------------------------
-- DROP en orden inverso (hijos antes que padre)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_Evento',      'U') IS NOT NULL DROP TABLE dbo.TC_Evento;
IF OBJECT_ID('dbo.TC_Responsable', 'U') IS NOT NULL DROP TABLE dbo.TC_Responsable;
IF OBJECT_ID('dbo.TC_TipoAlerta',  'U') IS NOT NULL DROP TABLE dbo.TC_TipoAlerta;


-- ------------------------------------------------------------
-- TC_TipoAlerta
-- Catálogo maestro de tipos de alerta.
-- Acceso restringido al equipo técnico (solo ellos insertan).
-- Los usuarios solo asignan responsables.
-- ------------------------------------------------------------
CREATE TABLE dbo.TC_TipoAlerta (
    IdTipoAlerta    INT             NOT NULL IDENTITY(1,1),
    Codigo          NVARCHAR(20)    NOT NULL,   -- ej: AGR-001, LOG-002
    Nombre          NVARCHAR(200)   NOT NULL,
    Area            NVARCHAR(10)    NOT NULL,   -- AGR | LOG | CAL | IND | COM
    Activo          BIT             NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME        NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_TC_TipoAlerta PRIMARY KEY (IdTipoAlerta),
    CONSTRAINT UQ_TC_TipoAlerta_Codigo UNIQUE (Codigo)
);

CREATE INDEX IX_TC_TipoAlerta_Codigo ON dbo.TC_TipoAlerta (Codigo);
CREATE INDEX IX_TC_TipoAlerta_Area   ON dbo.TC_TipoAlerta (Area);


-- ------------------------------------------------------------
-- TC_Responsable
-- Responsables por tipo de alerta (1..N).
-- El primero que toma la alerta la gestiona.
-- Al menos 1 responsable activo por tipo de alerta.
-- ------------------------------------------------------------
CREATE TABLE dbo.TC_Responsable (
    IdResponsable   INT             NOT NULL IDENTITY(1,1),
    IdTipoAlerta    INT             NOT NULL,
    Nombre          NVARCHAR(100)   NOT NULL,
    Telefono        NVARCHAR(20)    NULL,       -- formato internacional: +56912345678
    Email           NVARCHAR(150)   NULL,
    Activo          BIT             NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME        NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_TC_Responsable PRIMARY KEY (IdResponsable),
    CONSTRAINT FK_TC_Responsable_TipoAlerta
        FOREIGN KEY (IdTipoAlerta) REFERENCES dbo.TC_TipoAlerta (IdTipoAlerta)
);

CREATE INDEX IX_TC_Responsable_IdTipoAlerta ON dbo.TC_Responsable (IdTipoAlerta, Activo);


-- ------------------------------------------------------------
-- TC_Evento
-- Tabla central de eventos/alertas. Nunca se sobreescribe:
-- cada cambio relevante genera un nuevo registro.
-- Excepción: alertas tipo "foto diaria" (borrón y cuenta nueva)
-- se manejan con lógica especial en el SP correspondiente.
-- ------------------------------------------------------------
CREATE TABLE dbo.TC_Evento (
    IdEvento            INT             NOT NULL IDENTITY(1,1),
    IdTipoAlerta        INT             NOT NULL,
    Payload             NVARCHAR(MAX)   NULL,       -- JSON flexible, raw
    Estado              NVARCHAR(20)    NOT NULL DEFAULT 'Pendiente',  -- Pendiente | Gestionada
    AccionRespuesta     NVARCHAR(MAX)   NULL,       -- texto libre al gestionar (obligatorio al cerrar)
    QuienGestiono       NVARCHAR(100)   NULL,       -- usuario que tomó la alerta
    FechaGestion        DATETIME        NULL,       -- cuándo se cerró
    FechaOcurrencia     DATETIME        NOT NULL DEFAULT GETDATE(),
    OrigenSistema       NVARCHAR(50)    NOT NULL,   -- FSN | SAP | Job | Manual

    CONSTRAINT PK_TC_Evento PRIMARY KEY (IdEvento),
    CONSTRAINT FK_TC_Evento_TipoAlerta
        FOREIGN KEY (IdTipoAlerta) REFERENCES dbo.TC_TipoAlerta (IdTipoAlerta),
    CONSTRAINT CK_TC_Evento_Estado
        CHECK (Estado IN ('Pendiente', 'Gestionada'))
);

CREATE INDEX IX_TC_Evento_Estado          ON dbo.TC_Evento (Estado);
CREATE INDEX IX_TC_Evento_IdTipoAlerta    ON dbo.TC_Evento (IdTipoAlerta);
CREATE INDEX IX_TC_Evento_FechaOcurrencia ON dbo.TC_Evento (FechaOcurrencia DESC);


PRINT '>> Script 01 ejecutado: tablas TC_TipoAlerta, TC_Responsable, TC_Evento creadas.';
