USE master;
GO

CREATE DATABASE JoyeriaMorganDB;


USE JoyeriaMorganDB;
GO


-- TABLA 1: Categoria
CREATE TABLE dbo.Categoria (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

-- TABLA 2: Usuario
CREATE TABLE dbo.Usuario (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(100) NOT NULL UNIQUE,
    Clave VARCHAR(256) NOT NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Cliente'
);

-- TABLA 3: Producto
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

-- TABLA 4: Venta
CREATE TABLE dbo.Venta (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL CHECK (Total >= 0),
    DireccionEnvio VARCHAR(200) NOT NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'Completado',
    CONSTRAINT FK_Venta_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario(Id)
);

-- TABLA 5: DetalleVenta
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

-- =====================================================================
-- DATOS INICIALES
-- =====================================================================

-- Insertar Categorías
INSERT INTO dbo.Categoria (Nombre) VALUES 
('Anillos'), ('Collares'), ('Pulseras'), ('Aretes');

-- Insertar Usuarios de prueba
INSERT INTO dbo.Usuario (Nombre, Correo, Clave, Rol) VALUES 
('Administrador Morgan', 'admin@morgan.com', 'admin123', 'Admin'),
('Anasofia Martinez', 'cliente@gmail.com', 'cliente123', 'Cliente');

-- Insertar Joyas de prueba
INSERT INTO dbo.Producto (CategoriaId, Nombre, Descripcion, Precio, Stock, ImagenUrl) VALUES 
(1, 'Anillo Solitario Oro 18k', 'Anillo de oro amarillo con circonio central', 1250.00, 10, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTmD8J9obHixmonP2IlE6EicAD6OUn6k_g7Owl5hZYBmXwYXDtxAmwLpfA&s=10'),
(1, 'Anillo Compromiso Plata 925', 'Anillo fino de plata bañada en rodio', 280.00, 15, 'https://cdn-media.glamira.com/media/product/newgeneration/view/2/sku/gwd-h1002u-WOMEN/womenstone/diamond-zirconia_AAAAA/alloycolour/white/width/w8/profile/prA.jpg'),
(2, 'Collar Perla Elegance', 'Collar de plata con perla cultivada de río', 350.00, 8, 'https://image.made-in-china.com/202f0j00QjlbpFYWarqu/43cm-High-Grade-Accessories-Elegance-Style-Natural-White-Pearl-Necklace.webp'),
(3, 'Pulsera Tejida Oro Rosa', 'Pulsera ajustable diseño exclusivo en oro rosa', 890.00, 5, 'https://napoleonejoyas.co/cdn/shop/files/5051282522997722394.jpg?v=1774559471'),
(4, 'Aretes Gota de Cristal', 'Aretes colgantes de plata 925 con cristal austriaco', 190.00, 12, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSGQyNOrXjFVXhdv33Dc7WAcCHtZ5QpSPz92YMoM54FOwRaLLYXEYBQZWH2&s=10');
GO

-- =====================================================================
-- PROCEDIMIENTO ALMACENADO: LISTAR CATEGORÍAS
-- =====================================================================

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre FROM dbo.Categoria ORDER BY Nombre ASC;
END;
GO

-- =====================================================================
-- PROCEDIMIENTO ALMACENADO: CRUD PRODUCTOS
-- =====================================================================

-- SP: Listar productos
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Listar
    @Buscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.Id, p.CategoriaId, c.Nombre AS NombreCategoria, 
        p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
    FROM dbo.Producto p
    INNER JOIN dbo.Categoria c ON p.CategoriaId = c.Id
    WHERE (@Buscar IS NULL OR p.Nombre LIKE '%' + @Buscar + '%' OR c.Nombre LIKE '%' + @Buscar + '%')
    ORDER BY p.Id DESC;
END;
GO

-- SP: Listar productos con Paginación
CREATE OR ALTER PROCEDURE dbo.sp_Producto_ListarPaginado
    @Pagina INT = 1,
    @Tamano INT = 6,
    @Total INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;    
 
    SELECT @Total = COUNT(*) FROM dbo.Producto;

    SELECT 
        p.Id, p.CategoriaId, c.Nombre AS NombreCategoria, 
        p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
    FROM dbo.Producto p
    INNER JOIN dbo.Categoria c ON p.CategoriaId = c.Id
    ORDER BY p.Id DESC
    OFFSET (@Pagina - 1) * @Tamano ROWS 
    FETCH NEXT @Tamano ROWS ONLY;
END;
GO

-- SP: Obtener Producto por su ID
CREATE OR ALTER PROCEDURE dbo.sp_Producto_ObtenerPorId
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

-- SP: Insertar Producto
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Insertar
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

-- SP: Actualizar Producto
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Actualizar
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

-- SP: Eliminar Producto
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Producto WHERE Id = @Id;
END;
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: VENTAS Y CHECKOUT
-- =====================================================================

-- SP: Crear la cabecera de la orden y devolver su ID
CREATE OR ALTER PROCEDURE dbo.sp_Venta_Registrar
    @UsuarioId INT,
    @Total DECIMAL(10,2),
    @DireccionEnvio VARCHAR(200),
    @NewVentaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Venta (UsuarioId, Fecha, Total, DireccionEnvio, Estado)
    VALUES (@UsuarioId, GETDATE(), @Total, @DireccionEnvio, 'Completado');
    
    SET @NewVentaId = SCOPE_IDENTITY();
END;
GO

-- SP: Insertar un ítem comprado y descontar stock del inventario
CREATE OR ALTER PROCEDURE dbo.sp_DetalleVenta_InsertarYDescontarStock
    @VentaId INT,
    @ProductoId INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    -- 1. Insertar el detalle del pedido
    INSERT INTO dbo.DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario)
    VALUES (@VentaId, @ProductoId, @Cantidad, @PrecioUnitario);

    -- 2. Descontar el stock en la tabla Producto
    UPDATE dbo.Producto
    SET Stock = Stock - @Cantidad
    WHERE Id = @ProductoId;
END;
GO

-- =====================================================================
-- PROCEDIMIENTOS ALMACENADOS: HISTORIAL DE ÓRDENES
-- =====================================================================

-- SP: Historial de pedidos de un cliente en específico
CREATE OR ALTER PROCEDURE dbo.sp_Venta_ListarPorCliente
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        v.Id, v.Fecha, v.Total, v.DireccionEnvio, v.Estado,
        COUNT(d.Id) AS TotalItems
    FROM dbo.Venta v
    LEFT JOIN dbo.DetalleVenta d ON v.Id = d.VentaId
    WHERE v.UsuarioId = @UsuarioId
    GROUP BY v.Id, v.Fecha, v.Total, v.DireccionEnvio, v.Estado
    ORDER BY v.Fecha DESC;
END;
GO

-- SP: Historial general para el Administrador (Incluye nombre/correo del comprador)
CREATE OR ALTER PROCEDURE dbo.sp_Venta_ListarAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        v.Id, v.Fecha, v.Total, v.DireccionEnvio, v.Estado,
        u.Nombre AS NombreCliente, u.Correo AS CorreoCliente
    FROM dbo.Venta v
    INNER JOIN dbo.Usuario u ON v.UsuarioId = u.Id
    ORDER BY v.Fecha DESC;
END;
GO

-- SP: Ver los ítems completos dentro de una orden en específico
CREATE OR ALTER PROCEDURE dbo.sp_Venta_ObtenerDetalles
    @VentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        d.Id, d.VentaId, d.ProductoId, p.Nombre AS NombreProducto, 
        d.Cantidad, d.PrecioUnitario, (d.Cantidad * d.PrecioUnitario) AS Subtotal
    FROM dbo.DetalleVenta d
    INNER JOIN dbo.Producto p ON d.ProductoId = p.Id
    WHERE d.VentaId = @VentaId;
END;
GO

----

-- SP: Validar inicio de sesión por correo y clave
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Login
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

-- SP: Registrar un nuevo cliente (siempre nace con rol 'Cliente')
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Registrar
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

-- SP: Registra la cabecera de la orden y devuelve el Id generado
CREATE OR ALTER PROCEDURE dbo.sp_Venta_CrearCabecera
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

-- SP: Registra el ítem en DetalleVenta y descuenta el Stock en Producto
CREATE OR ALTER PROCEDURE dbo.sp_Venta_RegistrarDetalleYStock
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
    
    -- 2. Descontar stock del almacén
    UPDATE dbo.Producto
    SET Stock = Stock - @Cantidad
    WHERE Id = @ProductoId;
END;
GO