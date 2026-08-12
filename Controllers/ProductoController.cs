using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace JoyeriaMorgan.Controllers;

/// <summary>
/// Catalogo publico de joyas: solo consulta.
/// El alta, edicion y baja de productos vive en AdminController,
/// que esta protegido con [Authorize(Roles = "Admin")].
/// </summary>
public class ProductoController : Controller
{
    private readonly IProductoRepositorio _repo;
    private readonly ICategoriaRepositorio _categoriaRepo;
    private readonly ILogger<ProductoController> _logger;

    public ProductoController(
        IProductoRepositorio repo,
        ICategoriaRepositorio categoriaRepo,
        ILogger<ProductoController> logger)
    {
        _repo = repo;
        _categoriaRepo = categoriaRepo;
        _logger = logger;
    }

    // GET: /Producto?buscar=anillo&categoriaId=1&pagina=1
    public IActionResult Index(string? buscar, int? categoriaId, int pagina = 1)
    {
        const int tamano = 6;

        ViewData["Title"] = "Catálogo de Joyas";
        ViewBag.Buscar = buscar;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = 1;
        ViewBag.Categorias = new List<CategoriaViewModel>();

        try
        {
            List<ProductoViewModel> productos;
            int total;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                // La busqueda por texto devuelve todas las coincidencias sin paginar.
                productos = _repo.Listar(buscar, categoriaId);
                total = productos.Count;
                ViewBag.TotalPaginas = 1;
            }
            else
            {
                productos = _repo.ListarPaginado(pagina, tamano, out total, categoriaId);
                ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamano);
            }

            ViewBag.TotalResultados = total;
            ViewBag.Categorias = _categoriaRepo.Listar();

            return View(productos);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al cargar el catálogo.");
            TempData["Error"] = "No se pudo cargar el catálogo porque no hay conexión con la base de datos. "
                              + "Verifica que SQL Server esté encendido y que la base JoyeriaMorganDB exista "
                              + "(ejecuta morgan_db.sql).";
            return View(new List<ProductoViewModel>());
        }
    }

    // GET: /Producto/Detalle/5
    public IActionResult Detalle(int id)
    {
        try
        {
            var producto = _repo.ObtenerPorId(id);
            return producto == null ? NotFound() : View(producto);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al cargar el detalle de la joya {Id}.", id);
            TempData["Error"] = "No se pudo cargar la joya porque no hay conexión con la base de datos.";
            return RedirectToAction(nameof(Index));
        }
    }
}
