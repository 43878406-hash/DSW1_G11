USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'JoyeriaMorganDB')
BEGIN
    ALTER DATABASE JoyeriaMorganDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE JoyeriaMorganDB;
END;
GO

CREATE DATABASE JoyeriaMorganDB;
GO

USE JoyeriaMorganDB;
GO


CREATE TABLE dbo.Categoria (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE dbo.Usuario (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(100) NOT NULL UNIQUE,
    Clave VARCHAR(256) NOT NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Cliente'
);

CREATE TABLE dbo.Producto (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoriaId INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(255) NULL,
    Precio DECIMAL(10,2) NOT NULL CHECK (Precio > 0),
    Stock INT NOT NULL DEFAULT 0 CHECK (Stock >= 0),
    ImagenUrl VARCHAR(255) NULL,
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (CategoriaId) REFERENCES dbo.Categoria(Id)
);

CREATE TABLE dbo.Venta (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL CHECK (Total >= 0),
    DireccionEnvio VARCHAR(200) NOT NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'Completado',
    CONSTRAINT FK_Venta_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario(Id)
);

CREATE TABLE dbo.DetalleVenta (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(10,2) NOT NULL CHECK (PrecioUnitario > 0),
    CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (VentaId) REFERENCES dbo.Venta(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id)
);
GO


INSERT INTO dbo.Categoria (Nombre) VALUES 
('Anillos'), ('Collares'), ('Aretes'), ('Pulseras');

INSERT INTO dbo.Usuario (Nombre, Correo, Clave, Rol) VALUES 
('Administrador Morgan', 'admin@morgan.com', 'admin123', 'Admin'),
('Anasofia Martinez', 'cliente@gmail.com', 'cliente123', 'Cliente');

DECLARE @IdAnillos INT = (SELECT TOP 1 Id FROM dbo.Categoria WHERE Nombre LIKE '%Anillo%');
DECLARE @IdCollares INT = (SELECT TOP 1 Id FROM dbo.Categoria WHERE Nombre LIKE '%Collar%');
DECLARE @IdAretes INT = (SELECT TOP 1 Id FROM dbo.Categoria WHERE Nombre LIKE '%Arete%');
DECLARE @IdPulseras INT = (SELECT TOP 1 Id FROM dbo.Categoria WHERE Nombre LIKE '%Pulsera%');

INSERT INTO dbo.Producto (CategoriaId, Nombre, Descripcion, Precio, Stock, ImagenUrl) VALUES 
-- Anillos
(@IdAnillos, 'Anillo Solitario Oro 18k', 'Anillo de oro amarillo con circonio central', 1250.00, 10, 'https://images.unsplash.com/photo-1605100804763-247f67b3557e?q=80&w=600&auto=format&fit=crop'),
(@IdAnillos, 'Anillo de Compromiso Corte Princesa', 'Anillo en oro blanco de 18k con diamante central de 0.5 quilates', 3500.00, 5, 'https://images.unsplash.com/photo-1611591437281-460bfbe1220a?q=80&w=600&auto=format&fit=crop'),
(@IdAnillos, 'Anillo Zafiro y Diamantes', 'Majestuoso anillo estilo vintage en oro amarillo con un zafiro azul central', 4200.00, 3, 'https://images.unsplash.com/photo-1596944924616-7b38e7cfac36?q=80&w=600&auto=format&fit=crop'),

-- Collares
(@IdCollares, 'Collar Perla Elegance', 'Collar de plata con perla cultivada de río', 350.00, 8, 'https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?q=80&w=600&auto=format&fit=crop'),
(@IdCollares, 'Gargantilla de Oro Minimalista', 'Elegante y sutil cadena de oro amarillo 18k', 1450.00, 8, 'https://images.unsplash.com/photo-1611652022419-a9419f74343d?q=80&w=600&auto=format&fit=crop'),

-- Aretes
(@IdAretes, 'Broqueles de Rubí', 'Pendientes discretos y elegantes en oro de 18k con rubí rojo', 1800.00, 6, 'https://images.unsplash.com/photo-1630019852942-f89202989a59?q=80&w=600&auto=format&fit=crop'),
(@IdAretes, 'Aretes Largos Swarovski', 'Diseño contemporáneo en plata de ley 925 con cristales Swarovski', 450.00, 20, 'https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?q=80&w=600&auto=format&fit=crop'),

-- Pulseras
(@IdPulseras, 'Pulsera Tejida Oro Rosa', 'Pulsera ajustable diseño exclusivo en oro rosa', 890.00, 5, 'https://cdn-media.glamira.com/media/product/newgeneration/view/1/sku/rhett/alloycolour/red.jpg'),
(@IdPulseras, 'Pulsera Tennis de Circonias', 'Brillante pulsera estilo tennis fabricada en plata rodinada', 650.00, 12, 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=600&auto=format&fit=crop');
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: CATEGORÍAS Y PRODUCTOS
-- =====================================================================

CREATE PROCEDURE dbo.sp_Categoria_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre FROM dbo.Categoria ORDER BY Nombre ASC;
END;
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

CREATE PROCEDURE dbo.sp_Producto_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.Id, p.CategoriaId, c.Nombre AS NombreCategoria, 
        p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
    FROM dbo.Producto p
    INNER JOIN dbo.Categoria c ON p.CategoriaId = c.Id
    WHERE p.Id = @Id;
END;
GO

CREATE PROCEDURE dbo.sp_Producto_Insertar
    @CategoriaId INT,
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(255),
    @Precio DECIMAL(10,2),
    @Stock INT,
    @ImagenUrl VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Producto (CategoriaId, Nombre, Descripcion, Precio, Stock, ImagenUrl)
    VALUES (@CategoriaId, @Nombre, @Descripcion, @Precio, @Stock, @ImagenUrl);
END;
GO

CREATE PROCEDURE dbo.sp_Producto_Actualizar
    @Id INT,
    @CategoriaId INT,
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(255),
    @Precio DECIMAL(10,2),
    @Stock INT,
    @ImagenUrl VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Producto
    SET CategoriaId = @CategoriaId,
        Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock,
        ImagenUrl = @ImagenUrl
    WHERE Id = @Id;
END;
GO

CREATE PROCEDURE dbo.sp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Producto WHERE Id = @Id;
END;
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: USUARIOS Y AUTENTICACIÓN
-- =====================================================================

CREATE PROCEDURE dbo.sp_Usuario_Login
    @Correo VARCHAR(100),
    @Clave VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Correo, Clave, Rol
    FROM dbo.Usuario
    WHERE Correo = @Correo AND Clave = @Clave;
END;
GO

CREATE PROCEDURE dbo.sp_Usuario_Registrar
    @Nombre VARCHAR(100),
    @Correo VARCHAR(100),
    @Clave VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Usuario (Nombre, Correo, Clave, Rol)
    VALUES (@Nombre, @Correo, @Clave, 'Cliente');
END;
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: PROCESO DE VENTA Y HISTORIAL
-- =====================================================================

CREATE PROCEDURE dbo.sp_Venta_CrearCabecera
    @UsuarioId INT,
    @Total DECIMAL(10,2),
    @DireccionEnvio VARCHAR(200),
    @VentaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Venta (UsuarioId, Fecha, Total, DireccionEnvio, Estado)
    VALUES (@UsuarioId, GETDATE(), @Total, @DireccionEnvio, 'Completado');
    
    SET @VentaId = SCOPE_IDENTITY();
END;
GO

CREATE PROCEDURE dbo.sp_Venta_RegistrarDetalleYStock
    @VentaId INT,
    @ProductoId INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Insertar en DetalleVenta
    INSERT INTO dbo.DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario)
    VALUES (@VentaId, @ProductoId, @Cantidad, @PrecioUnitario);
    
    -- 2. Descontar stock
    UPDATE dbo.Producto
    SET Stock = Stock - @Cantidad
    WHERE Id = @ProductoId;
END;
GO

CREATE PROCEDURE dbo.sp_Venta_ListarPorCliente
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        v.Id, v.Fecha, v.Total, v.DireccionEnvio, v.Estado
    FROM dbo.Venta v
    WHERE v.UsuarioId = @UsuarioId
    ORDER BY v.Fecha DESC;
END;
GO

-- SP CLAVE PARA VER EL DETALLE DE LA ORDEN COMPRADA
CREATE PROCEDURE dbo.sp_Venta_ObtenerDetalles
    @VentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        d.Id, 
        d.VentaId, 
        d.ProductoId, 
        p.Nombre AS NombreProducto, 
        p.ImagenUrl,
        d.Cantidad, 
        d.PrecioUnitario, 
        (d.Cantidad * d.PrecioUnitario) AS Subtotal
    FROM dbo.DetalleVenta d
    INNER JOIN dbo.Producto p ON d.ProductoId = p.Id
    WHERE d.VentaId = @VentaId;
END;
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: MANTENIMIENTO DE CATEGORÍAS (CRUD)
-- =====================================================================

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

-- =====================================================================
-- PROCEDIMIENTO ADICIONAL: VALIDACIÓN DE CORREO EN EL REGISTRO
-- =====================================================================

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