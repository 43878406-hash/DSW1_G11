using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

/// <summary>
/// Resultado de intentar eliminar una categoria.
/// Los valores coinciden con el parametro OUTPUT @Resultado
/// del procedimiento dbo.sp_Categoria_Eliminar.
/// </summary>
public enum ResultadoEliminacion
{
    NoEncontrada = -1,
    TieneProductosAsociados = 0,
    Eliminada = 1
}

public interface ICategoriaRepositorio
{
    /// <summary>Listado simple, para combos y filtros del catálogo.</summary>
    List<CategoriaViewModel> Listar();

    /// <summary>Listado del mantenimiento, incluye cuántas joyas tiene cada categoría.</summary>
    List<CategoriaViewModel> ListarConConteo(string? buscar = null);

    CategoriaViewModel? ObtenerPorId(int id);

    /// <summary>Valida que el nombre no se repita. <paramref name="idExcluir"/> ignora la propia fila al editar.</summary>
    bool ExisteNombre(string nombre, int? idExcluir = null);

    int Insertar(CategoriaViewModel categoria);

    void Actualizar(CategoriaViewModel categoria);

    ResultadoEliminacion Eliminar(int id);
}
