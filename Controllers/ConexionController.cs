
using JoyeriaMorgan.Data;
using Microsoft.AspNetCore.Mvc;

namespace JoyeriaMorgan.Controllers;

public class ConexionController : Controller
{
    private readonly ConexionBD _bd;

    public ConexionController(ConexionBD bd)
    {
        _bd = bd;
    }

    // GET: /Conexion
    public IActionResult Index()
    {
        bool ok = _bd.ProbarConexion(out string mensaje);

        ViewBag.Exito = ok;
        ViewBag.Mensaje = mensaje;

        if (ok)
        {
            try
            {
                ViewBag.Total = _bd.ContarProductos();
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje += $" | Pero fallo la consulta: {ex.Message}";
            }
        }

        return View();
    }

}