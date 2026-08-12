using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JoyeriaMorgan.Data;

namespace JoyeriaMorgan.Controllers;

[Authorize]
public class PedidoController : Controller
{
    private readonly IPedidoRepositorio _repo;

    public PedidoController(IPedidoRepositorio repo)
    {
        _repo = repo;
    }

    // GET: /Pedido/MisPedidos
    public IActionResult MisPedidos()
    {
        string? idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out int usuarioId))
        {
            return RedirectToAction("Login", "Login");
        }

        var pedidos = _repo.ListarPorUsuario(usuarioId);
        return View(pedidos);
    }

    // GET: /Pedido/Detalle/10
    public IActionResult Detalle(int id)
    {
        string? idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out int usuarioId))
        {
            return RedirectToAction("Login", "Login");
        }

        var pedido = _repo.ObtenerDetalle(id, usuarioId);
        if (pedido == null) return NotFound();

        return View(pedido);
    }
}