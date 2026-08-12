using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Controllers;

public class LoginController : Controller
{
    // Codigos que devuelve SQL Server cuando se viola una restriccion UNIQUE.
    private const int ErrorSqlClavePrimariaDuplicada = 2627;
    private const int ErrorSqlIndiceUnicoDuplicado = 2601;

    private readonly IUsuarioRepositorio _repo;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IUsuarioRepositorio repo, ILogger<LoginController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET: /Login
    [HttpGet]
    [Route("Login")]
    public IActionResult Login() => View();

    // POST: /Login
    [HttpPost]
    [Route("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string correo, string clave)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            ViewBag.Error = "Debe ingresar correo y contraseña.";
            return View();
        }

        UsuarioViewModel? usuario;
        try
        {
            usuario = _repo.Login(correo, clave);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Fallo el acceso a la base de datos al iniciar sesión.");
            ViewBag.Error = "No se pudo conectar con la base de datos. Verifica que SQL Server esté encendido "
                          + "y que la base JoyeriaMorganDB exista (ejecuta morgan_db.sql).";
            return View();
        }

        if (usuario == null)
        {
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        TempData["Exito"] = $"¡Bienvenido(a), {usuario.Nombre}!";

        // Un administrador entra directo a su panel; un cliente, al catálogo.
        return usuario.Rol == "Admin"
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Producto");
    }

    // GET: /Registro
    [HttpGet]
    [Route("Registro")]
    public IActionResult Registrar() => View();

    // POST: /Registro
    [HttpPost]
    [Route("Registro")]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(UsuarioViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            // 1. Validamos el correo antes de intentar el INSERT, para poder
            //    mostrar el mensaje en el campo exacto que tiene el problema.
            if (_repo.ExisteCorreo(modelo.Correo))
            {
                ModelState.AddModelError(nameof(modelo.Correo), "Este correo ya se encuentra registrado.");
                return View(modelo);
            }

            _repo.Registrar(modelo);
            TempData["Exito"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }
        catch (SqlException ex) when (ex.Number is ErrorSqlClavePrimariaDuplicada or ErrorSqlIndiceUnicoDuplicado)
        {
            // 2. Red de seguridad: otro usuario pudo registrar el mismo correo
            //    entre la validacion anterior y el INSERT.
            ModelState.AddModelError(nameof(modelo.Correo), "Este correo ya se encuentra registrado.");
            return View(modelo);
        }
        catch (SqlException ex)
        {
            // 3. Cualquier otro error de base de datos se informa como lo que
            //    realmente es, en lugar de disfrazarlo de "correo duplicado".
            _logger.LogError(ex, "Fallo el acceso a la base de datos al registrar la cuenta.");
            ViewBag.Error = "No se pudo conectar con la base de datos. Verifica que SQL Server esté encendido "
                          + "y que la base JoyeriaMorganDB exista (ejecuta morgan_db.sql).";
            return View(modelo);
        }
    }

    // GET: /Salir
    [Route("Salir")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Exito"] = "Sesión cerrada correctamente.";
        return RedirectToAction("Index", "Producto");
    }

    // GET: /AccesoDenegado
    [Route("AccesoDenegado")]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
