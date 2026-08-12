using System.Data;
using JoyeriaMorgan.Models;
using Microsoft.Data.SqlClient;

namespace JoyeriaMorgan.Data;

/// <summary>
/// Acceso a datos de Categoria con ADO.NET y procedimientos almacenados.
/// Se resuelve por inyeccion de dependencias (ver Program.cs).
/// </summary>
public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly ConexionBD _bd;

    public CategoriaRepositorio(ConexionBD bd)
    {
        _bd = bd;
    }

    public List<CategoriaViewModel> Listar()
    {
        var lista = new List<CategoriaViewModel>();

        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_Listar", cn);
        cmd.CommandType = CommandType.StoredProcedure;

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            lista.Add(new CategoriaViewModel
            {
                Id = dr.GetInt32(dr.GetOrdinal("Id")),
                Nombre = dr.GetString(dr.GetOrdinal("Nombre"))
            });
        }

        return lista;
    }

    public List<CategoriaViewModel> ListarConConteo(string? buscar = null)
    {
        var lista = new List<CategoriaViewModel>();

        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_ListarConConteo", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? DBNull.Value : buscar);

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            lista.Add(MapearCategoria(dr));
        }

        return lista;
    }

    public CategoriaViewModel? ObtenerPorId(int id)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_ObtenerPorId", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        using var dr = cmd.ExecuteReader();
        return dr.Read() ? MapearCategoria(dr) : null;
    }

    public bool ExisteNombre(string nombre, int? idExcluir = null)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_ExisteNombre", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Nombre", nombre);
        cmd.Parameters.AddWithValue("@IdExcluir", (object?)idExcluir ?? DBNull.Value);

        var paramExiste = new SqlParameter("@Existe", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(paramExiste);

        cmd.ExecuteNonQuery();

        return paramExiste.Value != DBNull.Value && (bool)paramExiste.Value;
    }

    public int Insertar(CategoriaViewModel categoria)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_Insertar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());

        var paramNuevoId = new SqlParameter("@NuevoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(paramNuevoId);

        cmd.ExecuteNonQuery();

        return paramNuevoId.Value != DBNull.Value ? (int)paramNuevoId.Value : 0;
    }

    public void Actualizar(CategoriaViewModel categoria)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_Actualizar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", categoria.Id);
        cmd.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());

        cmd.ExecuteNonQuery();
    }

    public ResultadoEliminacion Eliminar(int id)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_Eliminar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        var paramResultado = new SqlParameter("@Resultado", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(paramResultado);

        cmd.ExecuteNonQuery();

        // El SP no borra nada si la categoria todavia tiene joyas asociadas.
        return paramResultado.Value != DBNull.Value
            ? (ResultadoEliminacion)(int)paramResultado.Value
            : ResultadoEliminacion.NoEncontrada;
    }

    private static CategoriaViewModel MapearCategoria(SqlDataReader dr)
    {
        return new CategoriaViewModel
        {
            Id = dr.GetInt32(dr.GetOrdinal("Id")),
            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
            TotalProductos = dr.GetInt32(dr.GetOrdinal("TotalProductos"))
        };
    }
}
