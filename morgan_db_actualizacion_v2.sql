-- =====================================================================
-- JOYERÍA MORGAN - SCRIPT DE ACTUALIZACIÓN v2
-- =====================================================================
-- Ejecutar este script SOLO si ya tienes la base JoyeriaMorganDB creada
-- con la versión anterior de morgan_db.sql y NO quieres perder tus datos.
--
-- Si vas a crear la base desde cero, ejecuta morgan_db.sql (que ya
-- incluye todo esto) y NO necesitas correr este archivo.
--
-- Contenido:
--   1. CRUD completo de Categorías (6 procedimientos nuevos)
--   2. Validación de correo duplicado en el registro (1 procedimiento)
--   3. Filtro por categoría en el catálogo (2 procedimientos modificados)
--
-- El script es idempotente: se puede ejecutar varias veces sin error.
-- =====================================================================

USE JoyeriaMorganDB;
GO

-- ---------------------------------------------------------------------
-- 1. CRUD DE CATEGORÍAS
-- ---------------------------------------------------------------------

IF OBJECT_ID('dbo.sp_Categoria_ListarConConteo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_ListarConConteo;
GO

-- Lista las categorías junto con la cantidad de joyas que tiene cada una.
-- Se usa LEFT JOIN para que las categorías vacías tambien aparezcan con 0.
CREATE PROCEDURE dbo.sp_Categoria_ListarConConteo
    @Buscar VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.Id,
        c.Nombre,
        COUNT(p.Id) AS TotalProductos
    FROM dbo.Categoria c
    LEFT JOIN dbo.Producto p ON p.CategoriaId = c.Id
    WHERE (@Buscar IS NULL OR c.Nombre LIKE '%' + @Buscar + '%')
    GROUP BY c.Id, c.Nombre
    ORDER BY c.Nombre ASC;
END;
GO

IF OBJECT_ID('dbo.sp_Categoria_ObtenerPorId', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_ObtenerPorId;
GO

CREATE PROCEDURE dbo.sp_Categoria_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.Id,
        c.Nombre,
        (SELECT COUNT(*) FROM dbo.Producto p WHERE p.CategoriaId = c.Id) AS TotalProductos
    FROM dbo.Categoria c
    WHERE c.Id = @Id;
END;
GO

IF OBJECT_ID('dbo.sp_Categoria_ExisteNombre', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_ExisteNombre;
GO

-- Verifica si un nombre de categoria ya existe.
-- @IdExcluir permite ignorar la propia categoria cuando se esta editando.
CREATE PROCEDURE dbo.sp_Categoria_ExisteNombre
    @Nombre VARCHAR(50),
    @IdExcluir INT = NULL,
    @Existe BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Categoria
               WHERE Nombre = @Nombre
                 AND (@IdExcluir IS NULL OR Id <> @IdExcluir))
        SET @Existe = 1;
    ELSE
        SET @Existe = 0;
END;
GO

IF OBJECT_ID('dbo.sp_Categoria_Insertar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_Insertar;
GO

CREATE PROCEDURE dbo.sp_Categoria_Insertar
    @Nombre VARCHAR(50),
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Categoria (Nombre) VALUES (@Nombre);
    SET @NuevoId = SCOPE_IDENTITY();
END;
GO

IF OBJECT_ID('dbo.sp_Categoria_Actualizar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_Actualizar;
GO

CREATE PROCEDURE dbo.sp_Categoria_Actualizar
    @Id INT,
    @Nombre VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Categoria
    SET Nombre = @Nombre
    WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.sp_Categoria_Eliminar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Categoria_Eliminar;
GO

-- Elimina la categoria solo si no tiene joyas asociadas (integridad referencial).
-- @Resultado: 1 = eliminada, 0 = tiene productos, -1 = no existe.
CREATE PROCEDURE dbo.sp_Categoria_Eliminar
    @Id INT,
    @Resultado INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE Id = @Id)
    BEGIN
        SET @Resultado = -1;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Producto WHERE CategoriaId = @Id)
    BEGIN
        SET @Resultado = 0;
        RETURN;
    END

    DELETE FROM dbo.Categoria WHERE Id = @Id;
    SET @Resultado = 1;
END;
GO

-- ---------------------------------------------------------------------
-- 2. VALIDACIÓN DE CORREO EN EL REGISTRO
-- ---------------------------------------------------------------------

IF OBJECT_ID('dbo.sp_Usuario_ExisteCorreo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Usuario_ExisteCorreo;
GO

CREATE PROCEDURE dbo.sp_Usuario_ExisteCorreo
    @Correo VARCHAR(100),
    @Existe BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = @Correo)
        SET @Existe = 1;
    ELSE
        SET @Existe = 0;
END;
GO

-- ---------------------------------------------------------------------
-- 3. FILTRO POR CATEGORÍA EN EL CATÁLOGO
--    (se agrega el parámetro opcional @CategoriaId; las llamadas
--     existentes siguen funcionando porque su valor por defecto es NULL)
-- ---------------------------------------------------------------------

IF OBJECT_ID('dbo.sp_Producto_Listar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Producto_Listar;
GO

CREATE PROCEDURE dbo.sp_Producto_Listar
    @Buscar VARCHAR(100) = NULL,
    @CategoriaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.Id, p.CategoriaId, c.Nombre AS NombreCategoria,
        p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
    FROM dbo.Producto p
    INNER JOIN dbo.Categoria c ON p.CategoriaId = c.Id
    WHERE (@Buscar IS NULL OR p.Nombre LIKE '%' + @Buscar + '%' OR c.Nombre LIKE '%' + @Buscar + '%')
      AND (@CategoriaId IS NULL OR p.CategoriaId = @CategoriaId)
    ORDER BY p.Id DESC;
END;
GO

IF OBJECT_ID('dbo.sp_Producto_ListarPaginado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Producto_ListarPaginado;
GO

CREATE PROCEDURE dbo.sp_Producto_ListarPaginado
    @Pagina INT = 1,
    @Tamano INT = 6,
    @CategoriaId INT = NULL,
    @Total INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @Total = COUNT(*)
    FROM dbo.Producto
    WHERE (@CategoriaId IS NULL OR CategoriaId = @CategoriaId);

    SELECT
        p.Id, p.CategoriaId, c.Nombre AS NombreCategoria,
        p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
    FROM dbo.Producto p
    INNER JOIN dbo.Categoria c ON p.CategoriaId = c.Id
    WHERE (@CategoriaId IS NULL OR p.CategoriaId = @CategoriaId)
    ORDER BY p.Id DESC
    OFFSET (@Pagina - 1) * @Tamano ROWS
    FETCH NEXT @Tamano ROWS ONLY;
END;
GO

PRINT 'Actualización v2 aplicada correctamente.';
GO
