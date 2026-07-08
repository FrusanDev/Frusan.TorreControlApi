-- ============================================================
-- Torre de Control — AGR-000 Plataforma Habilitante
-- Script 04: Seguridad — API Key por sistema origen
-- ============================================================


-- ------------------------------------------------------------
-- TC_OrigenAutorizado
-- Sistemas autorizados a insertar alertas via POST /api/alertas
-- (SAP, FSN, jobs, etc.). Guarda hash+salt de la API Key,
-- nunca la key en texto plano.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_OrigenAutorizado', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TC_OrigenAutorizado (
        IdOrigenAutorizado  INT             NOT NULL IDENTITY(1,1),
        Codigo              NVARCHAR(50)    NOT NULL,
        Descripcion         NVARCHAR(200)   NULL,
        ApiKeyHash          VARBINARY(32)   NOT NULL,
        ApiKeySalt          VARBINARY(16)   NOT NULL,
        Activo              BIT             NOT NULL DEFAULT 1,
        FechaCreacion       DATETIME        NOT NULL DEFAULT GETDATE(),
        FechaUltimoUso      DATETIME        NULL,

        CONSTRAINT PK_TC_OrigenAutorizado         PRIMARY KEY (IdOrigenAutorizado),
        CONSTRAINT UQ_TC_OrigenAutorizado_Codigo  UNIQUE (Codigo)
    );
END
GO


-- ------------------------------------------------------------
-- TC_SP_ValidarOrigenAutorizado
-- Valida una API Key en texto plano contra el hash guardado.
-- Si matchea un origen activo, actualiza FechaUltimoUso.
-- Usado por ApiKeyAuthHandler en cada request a la API.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_ValidarOrigenAutorizado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_ValidarOrigenAutorizado;
GO

CREATE PROCEDURE dbo.TC_SP_ValidarOrigenAutorizado
    @ApiKeyPlano NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdOrigenAutorizado INT;

    SELECT @IdOrigenAutorizado = IdOrigenAutorizado
    FROM dbo.TC_OrigenAutorizado
    WHERE Activo = 1
      AND ApiKeyHash = HASHBYTES('SHA2_256', ApiKeySalt + CONVERT(VARBINARY(4000), @ApiKeyPlano));

    IF @IdOrigenAutorizado IS NOT NULL
    BEGIN
        UPDATE dbo.TC_OrigenAutorizado
        SET FechaUltimoUso = GETDATE()
        WHERE IdOrigenAutorizado = @IdOrigenAutorizado;
    END

    SELECT
        IdOrigenAutorizado,
        Codigo,
        Activo
    FROM dbo.TC_OrigenAutorizado
    WHERE IdOrigenAutorizado = @IdOrigenAutorizado;
END
GO


-- ------------------------------------------------------------
-- TC_SP_InsertarOrigenAutorizado
-- Aprovisiona un nuevo sistema origen. La API Key la GENERA el
-- propio SP (CRYPT_GEN_RANDOM) y la devuelve una única vez por
-- @ApiKeyPlano OUTPUT — nunca se recibe como parámetro de entrada
-- ni se guarda en texto plano (solo su hash+salt). Ejecución
-- manual (SSMS), nunca expuesto por HTTP. Así, provisionar un
-- origen nuevo no depende de ninguna herramienta externa para
-- generar la key: alcanza con un EXEC en SSMS.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_InsertarOrigenAutorizado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_InsertarOrigenAutorizado;
GO

CREATE PROCEDURE dbo.TC_SP_InsertarOrigenAutorizado
    @Codigo             NVARCHAR(50),
    @Descripcion        NVARCHAR(200) = NULL,
    @ApiKeyPlano        NVARCHAR(200) OUTPUT,
    @IdOrigenAutorizado INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.TC_OrigenAutorizado WHERE Codigo = @Codigo)
    BEGIN
        RAISERROR('Ya existe un origen autorizado con ese Codigo.', 16, 1);
        RETURN;
    END

    DECLARE @KeyBytes  VARBINARY(32) = CRYPT_GEN_RANDOM(32);
    DECLARE @KeyBase64 VARCHAR(100) = CAST('' AS XML).value('xs:base64Binary(sql:variable("@KeyBytes"))', 'VARCHAR(100)');
    SET @ApiKeyPlano = 'tc_' + REPLACE(REPLACE(REPLACE(@KeyBase64, '+', '-'), '/', '_'), '=', '');

    DECLARE @Salt VARBINARY(16) = CRYPT_GEN_RANDOM(16);

    INSERT INTO dbo.TC_OrigenAutorizado (
        Codigo,
        Descripcion,
        ApiKeyHash,
        ApiKeySalt,
        Activo,
        FechaCreacion
    )
    VALUES (
        @Codigo,
        @Descripcion,
        HASHBYTES('SHA2_256', @Salt + CONVERT(VARBINARY(4000), @ApiKeyPlano)),
        @Salt,
        1,
        GETDATE()
    );

    SET @IdOrigenAutorizado = SCOPE_IDENTITY();
END
GO


-- ------------------------------------------------------------
-- TC_SP_CambiarEstadoOrigenAutorizado
-- Activa/desactiva un origen autorizado (revocación instantánea
-- de una API Key sin borrar el historial). Ejecución manual.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.TC_SP_CambiarEstadoOrigenAutorizado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.TC_SP_CambiarEstadoOrigenAutorizado;
GO

CREATE PROCEDURE dbo.TC_SP_CambiarEstadoOrigenAutorizado
    @Codigo NVARCHAR(50),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TC_OrigenAutorizado
    SET Activo = @Activo
    WHERE Codigo = @Codigo;

    IF @@ROWCOUNT = 0
        RAISERROR('Origen autorizado no encontrado.', 16, 1);
END
GO


PRINT '>> Script 04 ejecutado: tabla TC_OrigenAutorizado y SPs TC_SP_ValidarOrigenAutorizado, TC_SP_InsertarOrigenAutorizado, TC_SP_CambiarEstadoOrigenAutorizado creados.';
