using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public interface IProductoRepositorio
{
    List<ProductoViewModel> Listar(string? buscar = null);
    List<ProductoViewModel> ListarPaginado(int pagina, int tamano, out int total);
    ProductoViewModel? ObtenerPorId(int id);
    void Insertar(ProductoViewModel producto);
    void Actualizar(ProductoViewModel producto);
    void Eliminar(int id);
    List<CategoriaViewModel> ListarCategorias();
}