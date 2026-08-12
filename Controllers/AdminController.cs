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
    private readonly ICategoriaRepositorio _categoriaRepo;

    public AdminController(IProductoRepositorio repo, ICategoriaRepositorio categoriaRepo)
    {
        _repo = repo;
        _categoriaRepo = categoriaRepo;
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
            ViewBag.TotalPaginas = 1;
        }
        else
        {
            productos = _repo.ListarPaginado(pagina, tamano, out total);
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamano);
        }

        // Un solo viaje a la base nos da las dos cifras del resumen:
        // cuantas categorias hay y cuantas joyas suman entre todas.
        var categorias = _categoriaRepo.ListarConConteo();
        ViewBag.TotalCategorias = categorias.Count;
        ViewBag.TotalJoyas = categorias.Sum(c => c.TotalProductos);

        ViewBag.Buscar = buscar;
        ViewBag.Pagina = pagina;
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
        var categorias = _categoriaRepo.Listar();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
    }
}
