using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using JoyeriaMorgan.Data;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Controllers;

public class LoginController : Controller
{
    private readonly IUsuarioRepositorio _repo;

    public LoginController(IUsuarioRepositorio repo)
    {
        _repo = repo;
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

        var usuario = _repo.Login(correo, clave);
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
        return RedirectToAction("Index", "Producto");
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
            _repo.Registrar(modelo);
            TempData["Exito"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }
        catch
        {
            ViewBag.Error = "El correo ingresado ya se encuentra registrado.";
            return View(modelo);
        }
    }

    // GET: /Salir
    [Route("Salir")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Producto");
    }

    // GET: /AccesoDenegado
    [Route("AccesoDenegado")]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}