
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public interface IVentaRepositorio
{
    int RegistrarVenta(int usuarioId, string direccionEnvio, List<ItemCarritoViewModel> items);
}