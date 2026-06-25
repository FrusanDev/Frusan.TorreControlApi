-- ============================================================
-- Torre de Control — AGR-000 Plataforma Habilitante
-- Script 02: Stored Procedures
-- ============================================================


-- ------------------------------------------------------------
-- TC_SP_ObtenerTipoAlerta
-- Busca un tipo de alerta por su código.
-- Usado por AlertaDAL antes de insertar para validar
-- que el código existe y está activo.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_ObtenerTipoAlerta', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_ObtenerTipoAlerta;
GO

CREATE PROCEDURE dbo.TC_SP_ObtenerTipoAlerta
    @Codigo NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdTipoAlerta,
        Codigo,
        Nombre,
        Area,
        Activo
    FROM dbo.TC_TipoAlerta
    WHERE Codigo = @Codigo;
END
GO


-- ------------------------------------------------------------
-- TC_SP_InsertarEvento
-- Inserta un nuevo evento en TC_Evento.
-- Retorna el IdEvento generado via parámetro OUTPUT.
-- El estado inicial siempre es 'Pendiente'.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_InsertarEvento', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_InsertarEvento;
GO

CREATE PROCEDURE dbo.TC_SP_InsertarEvento
    @IdTipoAlerta       INT,
    @Payload            NVARCHAR(MAX),
    @Estado             NVARCHAR(20),
    @FechaOcurrencia    DATETIME,
    @OrigenSistema      NVARCHAR(50),
    @IdEvento           INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TC_Evento (
        IdTipoAlerta,
        Payload,
        Estado,
        FechaOcurrencia,
        OrigenSistema
    )
    VALUES (
        @IdTipoAlerta,
        @Payload,
        @Estado,
        @FechaOcurrencia,
        @OrigenSistema
    );

    SET @IdEvento = SCOPE_IDENTITY();
END
GO


-- ------------------------------------------------------------
-- TC_SP_ObtenerResponsables
-- Retorna los responsables activos de un tipo de alerta.
-- Usado por AlertaBLL para saber a quién notificar por WA.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_ObtenerResponsables', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_ObtenerResponsables;
GO

CREATE PROCEDURE dbo.TC_SP_ObtenerResponsables
    @IdTipoAlerta INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdResponsable,
        IdTipoAlerta,
        Nombre,
        Telefono,
        Email,
        Activo
    FROM dbo.TC_Responsable
    WHERE IdTipoAlerta = @IdTipoAlerta
      AND Activo = 1
    ORDER BY IdResponsable;
END
GO


-- ------------------------------------------------------------
-- TC_SP_GestionarEvento
-- Cierra un evento con la acción tomada.
-- AccionRespuesta es OBLIGATORIA (validado aquí).
-- Registra quién gestionó y cuándo.
-- Aunque la FSN hará esto directo, el SP centraliza
-- la lógica de cierre para cualquier origen.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_GestionarEvento', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_GestionarEvento;
GO

CREATE PROCEDURE dbo.TC_SP_GestionarEvento
    @IdEvento           INT,
    @AccionRespuesta    NVARCHAR(MAX),
    @QuienGestiono      NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF @AccionRespuesta IS NULL OR LTRIM(RTRIM(@AccionRespuesta)) = ''
    BEGIN
        RAISERROR('AccionRespuesta es obligatoria para gestionar un evento.', 16, 1);
        RETURN;
    END

    UPDATE dbo.TC_Evento
    SET
        Estado           = 'Gestionada',
        AccionRespuesta  = @AccionRespuesta,
        QuienGestiono    = @QuienGestiono,
        FechaGestion     = GETDATE()
    WHERE IdEvento = @IdEvento
      AND Estado   = 'Pendiente';

    IF @@ROWCOUNT = 0
        RAISERROR('Evento no encontrado o ya fue gestionado.', 16, 1);
END
GO


-- ------------------------------------------------------------
-- TC_SP_ObtenerEventosPendientes
-- Vista operativa: todos los eventos pendientes,
-- ordenados por antigüedad (más viejos primero).
-- Usado por la pantalla FSN de la Torre de Control.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_ObtenerEventosPendientes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_ObtenerEventosPendientes;
GO

CREATE PROCEDURE dbo.TC_SP_ObtenerEventosPendientes
    @Area NVARCHAR(10) = NULL   -- NULL = todas las áreas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.IdEvento,
        e.IdTipoAlerta,
        t.Codigo            AS CodigoTipoAlerta,
        t.Nombre            AS NombreTipoAlerta,
        t.Area,
        e.Payload,
        e.Estado,
        e.FechaOcurrencia,
        e.OrigenSistema,
        DATEDIFF(MINUTE, e.FechaOcurrencia, GETDATE()) AS MinutosPendiente
    FROM dbo.TC_Evento e
    INNER JOIN dbo.TC_TipoAlerta t ON e.IdTipoAlerta = t.IdTipoAlerta
    WHERE e.Estado = 'Pendiente'
      AND (@Area IS NULL OR t.Area = @Area)
    ORDER BY e.FechaOcurrencia ASC;
END
GO


PRINT '>> Script 02 ejecutado: SPs TC_SP_ObtenerTipoAlerta, TC_SP_InsertarEvento, TC_SP_ObtenerResponsables, TC_SP_GestionarEvento, TC_SP_ObtenerEventosPendientes creados.';
