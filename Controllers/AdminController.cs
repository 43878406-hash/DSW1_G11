using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Controllers;

[Authorize(Roles = "Admin")] // Protegido para administradores
public class AdminController : Controller
{
    private readonly IProductoRepositorio _repo;

    public AdminController(IProductoRepositorio repo)
    {
        _repo = repo;
    }

    // GET: /Admin o /Admin/Index
    [HttpGet]
    [Route("Admin")]
    [Route("Admin/Index")]
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        const int tamano = 10;
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
        ViewData["Title"] = "Gestión de Inventario";

        return View(productos);
    }

    // GET: /Admin/Registrar
    [HttpGet]
    public IActionResult Registrar()
    {
        CargarCategoriasViewBag();
        return View();
    }

    // POST: /Admin/Registrar
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
        TempData["Exito"] = $"Joya '{modelo.Nombre}' registrada correctamente en el catálogo.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Editar/5
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var joya = _repo.ObtenerPorId(id);
        if (joya == null) return NotFound();

        CargarCategoriasViewBag();
        return View(joya);
    }

    // POST: /Admin/Editar
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

    // POST: /Admin/Eliminar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _repo.Eliminar(id);
        TempData["Exito"] = "Joya eliminada del inventario.";
        return RedirectToAction(nameof(Index));
    }

    private void CargarCategoriasViewBag()
    {
        var categorias = _repo.ListarCategorias();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
    }
}