using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public interface IPedidoRepositorio
{
    List<PedidoViewModel> ListarPorUsuario(int usuarioId);
    PedidoViewModel? ObtenerDetalle(int ventaId, int usuarioId);
}