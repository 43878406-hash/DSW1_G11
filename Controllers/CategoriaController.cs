using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Controllers;

/// <summary>
/// Mantenimiento (CRUD) de las categorías de joyas.
/// Solo accesible para usuarios con rol Admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class CategoriaController : Controller
{
    // Codigos que devuelve SQL Server cuando se viola la restriccion UNIQUE
    // del campo Nombre de la tabla Categoria.
    private const int ErrorSqlClavePrimariaDuplicada = 2627;
    private const int ErrorSqlIndiceUnicoDuplicado = 2601;

    private readonly ICategoriaRepositorio _repo;
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(ICategoriaRepositorio repo, ILogger<CategoriaController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: /Categoria?buscar=anillo
    [HttpGet]
    public IActionResult Index(string? buscar)
    {
        ViewData["Title"] = "Mantenimiento de Categorías";
        ViewBag.Buscar = buscar;

        var categorias = _repo.ListarConConteo(buscar);
        return View(categorias);
    }

    // GET: /Categoria/Crear
    [HttpGet]
    public IActionResult Crear()
    {
        ViewData["Title"] = "Nueva Categoría";
        return View(new CategoriaViewModel());
    }

    // POST: /Categoria/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(CategoriaViewModel modelo)
    {
        ViewData["Title"] = "Nueva Categoría";

        if (!ModelState.IsValid) return View(modelo);

        try
        {
            if (_repo.ExisteNombre(modelo.Nombre.Trim()))
            {
                ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe una categoría con ese nombre.");
                return View(modelo);
            }

            _repo.Insertar(modelo);
            TempData["Exito"] = $"Categoría '{modelo.Nombre.Trim()}' creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is ErrorSqlClavePrimariaDuplicada or ErrorSqlIndiceUnicoDuplicado)
        {
            ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe una categoría con ese nombre.");
            return View(modelo);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al crear la categoría.");
            ViewBag.Error = "No se pudo guardar la categoría por un problema con la base de datos.";
            return View(modelo);
        }
    }

    // GET: /Categoria/Editar/5
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var categoria = _repo.ObtenerPorId(id);
        if (categoria == null) return NotFound();

        ViewData["Title"] = "Editar Categoría";
        return View(categoria);
    }

    // POST: /Categoria/Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(CategoriaViewModel modelo)
    {
        ViewData["Title"] = "Editar Categoría";

        if (!ModelState.IsValid) return View(modelo);

        try
        {
            // Al editar excluimos la propia fila para que no choque consigo misma.
            if (_repo.ExisteNombre(modelo.Nombre.Trim(), modelo.Id))
            {
                ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe otra categoría con ese nombre.");
                return View(modelo);
            }

            _repo.Actualizar(modelo);
            TempData["Exito"] = $"Categoría '{modelo.Nombre.Trim()}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is ErrorSqlClavePrimariaDuplicada or ErrorSqlIndiceUnicoDuplicado)
        {
            ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe otra categoría con ese nombre.");
            return View(modelo);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al actualizar la categoría {Id}.", modelo.Id);
            ViewBag.Error = "No se pudo actualizar la categoría por un problema con la base de datos.";
            return View(modelo);
        }
    }

    // POST: /Categoria/Eliminar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            // El procedimiento almacenado protege la integridad referencial:
            // no borra la categoria si todavia tiene joyas asociadas.
            switch (_repo.Eliminar(id))
            {
                case ResultadoEliminacion.Eliminada:
                    TempData["Exito"] = "Categoría eliminada correctamente.";
                    break;

                case ResultadoEliminacion.TieneProductosAsociados:
                    TempData["Error"] = "No se puede eliminar la categoría porque tiene joyas asociadas. "
                                      + "Reasigna o elimina primero esas joyas del inventario.";
                    break;

                case ResultadoEliminacion.NoEncontrada:
                    TempData["Error"] = "La categoría que intentas eliminar ya no existe.";
                    break;
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al eliminar la categoría {Id}.", id);
            TempData["Error"] = "No se pudo eliminar la categoría por un problema con la base de datos.";
        }

        return RedirectToAction(nameof(Index));
    }
}
