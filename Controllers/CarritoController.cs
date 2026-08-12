using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JoyeriaMorgan.Data;
using JoyeriaMorgan.Helpers;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Controllers;

public class CarritoController : Controller
{
    // Se califica el namespace completo porque ASP.NET Core tiene su propia
    // clase SessionExtensions y el nombre corto resulta ambiguo.
    private const string ClaveCarrito = JoyeriaMorgan.Helpers.SessionExtensions.ClaveCarrito;
    private readonly IProductoRepositorio _productoRepo;
    private readonly IVentaRepositorio _ventaRepo;

    public CarritoController(IProductoRepositorio productoRepo, IVentaRepositorio ventaRepo)
    {
        _productoRepo = productoRepo;
        _ventaRepo = ventaRepo;
    }

    // GET: /Carrito
    public IActionResult Index()
    {
        var carrito = ObtenerCarritoDeSession();
        return View(carrito);
    }

    // GET o POST: /Carrito/Agregar/5
    public IActionResult Agregar(int id, int cantidad = 1)
    {
        var joya = _productoRepo.ObtenerPorId(id);
        if (joya == null || joya.Stock < cantidad)
        {
            TempData["Error"] = "El producto no existe o no cuenta con stock suficiente.";
            return RedirectToAction("Index", "Producto");
        }

        var carrito = ObtenerCarritoDeSession();
        var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == id);

        if (itemExistente != null)
        {
            itemExistente.Cantidad += cantidad;
        }
        else
        {
            carrito.Add(new ItemCarritoViewModel
            {
                ProductoId = joya.Id,
                Nombre = joya.Nombre,
                Precio = joya.Precio,
                Cantidad = cantidad,
                ImagenUrl = joya.ImagenUrl
            });
        }

        GuardarCarritoEnSession(carrito);
        TempData["Exito"] = $"'{joya.Nombre}' se añadió a tu bolsa de compra.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Carrito/Eliminar/5
    [HttpPost]
    public IActionResult Eliminar(int id)
    {
        var carrito = ObtenerCarritoDeSession();
        var item = carrito.FirstOrDefault(i => i.ProductoId == id);
        if (item != null)
        {
            carrito.Remove(item);
            GuardarCarritoEnSession(carrito);
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Carrito/Vaciar
    public IActionResult Vaciar()
    {
        HttpContext.Session.Remove(ClaveCarrito);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Carrito/Checkout
    [HttpPost]
    [Authorize] // Exige estar logueado para confirmar la compra
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(string direccionEnvio)
    {
        var carrito = ObtenerCarritoDeSession();
        if (!carrito.Any())
        {
            TempData["Error"] = "Tu bolsa de compra está vacía.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(direccionEnvio))
        {
            TempData["Error"] = "Por favor, ingresa una dirección de envío.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Obtener el ID del usuario logueado desde sus Claims
            string? usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
            {
                return RedirectToAction("Login", "Login");
            }

            // Ejecutar la transacción en SQL Server
            int nuevaOrdenId = _ventaRepo.RegistrarVenta(usuarioId, direccionEnvio, carrito);

            // Vaciar el carrito temporal tras una compra exitosa
            HttpContext.Session.Remove(ClaveCarrito);

            // Redirigir a la vista de éxito
            return RedirectToAction(nameof(Confirmacion), new { ordenId = nuevaOrdenId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ocurrió un problema al procesar tu pedido: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Carrito/Confirmacion?ordenId=10
    [Authorize]
    public IActionResult Confirmacion(int ordenId)
    {
        ViewBag.OrdenId = ordenId;
        return View();
    }

    // --- Métodos Helper Privados para manejar Session ---
    private List<ItemCarritoViewModel> ObtenerCarritoDeSession()
    {
        return HttpContext.Session.GetObjeto<List<ItemCarritoViewModel>>(ClaveCarrito)
               ?? new List<ItemCarritoViewModel>();
    }

    private void GuardarCarritoEnSession(List<ItemCarritoViewModel> carrito)
    {
        HttpContext.Session.SetObjeto(ClaveCarrito, carrito);
    }
}