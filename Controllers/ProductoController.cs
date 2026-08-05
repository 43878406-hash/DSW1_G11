using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JoyeriaMorgan.Controllers;

public class ProductoController : Controller
{
    private readonly IProductoRepositorio _repo;

    public ProductoController(IProductoRepositorio repo)
    {
        _repo = repo;
    }

    // GET: /Producto?buscar=anillo&pagina=1
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        const int tamano = 6;
        List<ProductoViewModel> productos;
        int total;

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            productos = _repo.Listar(buscar);
            total = productos.Count;
        }
        else
        {
            productos = _repo.ListarPaginado(pagina, tamano, out total);
        }

        ViewBag.Buscar = buscar;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamano);
        ViewData["Title"] = "Catálogo de Joyas";

        return View(productos);
    }

    // GET: /Producto/Detalle/5
    public IActionResult Detalle(int id)
    {
        var producto = _repo.ObtenerPorId(id);
        return producto == null ? NotFound() : View(producto);
    }

    // GET: /Producto/Registrar
    [HttpGet]
    public IActionResult Registrar()
    {
        CargarCategoriasViewBag();
        return View();
    }

    // POST: /Producto/Registrar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(ProductoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            CargarCategoriasViewBag();
            return View(modelo);
        }

        _repo.Insertar(modelo);
        TempData["Exito"] = $"Joya '{modelo.Nombre}' registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Producto/Editar/5
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var producto = _repo.ObtenerPorId(id);
        if (producto == null) return NotFound();

        CargarCategoriasViewBag();
        return View(producto);
    }

    // POST: /Producto/Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(ProductoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            CargarCategoriasViewBag();
            return View(modelo);
        }

        _repo.Actualizar(modelo);
        TempData["Exito"] = $"Joya '{modelo.Nombre}' actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Producto/Eliminar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _repo.Eliminar(id);
        TempData["Exito"] = "Joya eliminada del catálogo.";
        return RedirectToAction(nameof(Index));
    }


    private void CargarCategoriasViewBag()
    {
        var categorias = _repo.ListarCategorias();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
    }
}