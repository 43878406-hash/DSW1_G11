
using System.Data;
using JoyeriaMorgan.Models;
using Microsoft.Data.SqlClient;

namespace JoyeriaMorgan.Data;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly ConexionBD _bd;

    public ProductoRepositorio(ConexionBD bd)
    {
        _bd = bd;
    }

    public List<ProductoViewModel> Listar(string? buscar = null)
    {
        var lista = new List<ProductoViewModel>();
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_Listar", cn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? DBNull.Value : buscar);

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            lista.Add(MapearProducto(dr));
        }
        return lista;
    }

    public List<ProductoViewModel> ListarPaginado(int pagina, int tamano, out int total)
    {
        var lista = new List<ProductoViewModel>();
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_ListarPaginado", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Pagina", pagina);
        cmd.Parameters.AddWithValue("@Tamano", tamano);

        var paramTotal = new SqlParameter("@Total", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(paramTotal);

        using (var dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                lista.Add(MapearProducto(dr));
            }
        }

        total = (int)(paramTotal.Value != DBNull.Value ? paramTotal.Value : 0);
        return lista;
    }

    public ProductoViewModel? ObtenerPorId(int id)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_ObtenerPorId", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        using var dr = cmd.ExecuteReader();
        return dr.Read() ? MapearProducto(dr) : null;
    }

    public void Insertar(ProductoViewModel p)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_Insertar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@CategoriaId", p.CategoriaId);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@Descripcion", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Precio", p.Precio);
        cmd.Parameters.AddWithValue("@Stock", p.Stock);
        cmd.Parameters.AddWithValue("@ImagenUrl", (object?)p.ImagenUrl ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public void Actualizar(ProductoViewModel p)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_Actualizar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", p.Id);
        cmd.Parameters.AddWithValue("@CategoriaId", p.CategoriaId);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@Descripcion", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Precio", p.Precio);
        cmd.Parameters.AddWithValue("@Stock", p.Stock);
        cmd.Parameters.AddWithValue("@ImagenUrl", (object?)p.ImagenUrl ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Producto_Eliminar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        cmd.ExecuteNonQuery();
    }

    public List<CategoriaViewModel> ListarCategorias()
    {
        var categorias = new List<CategoriaViewModel>();
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Categoria_Listar", cn);
        cmd.CommandType = CommandType.StoredProcedure;

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            categorias.Add(new CategoriaViewModel
            {
                Id = dr.GetInt32(dr.GetOrdinal("Id")),
                Nombre = dr.GetString(dr.GetOrdinal("Nombre"))
            });
        }
        return categorias;
    }


    private static ProductoViewModel MapearProducto(SqlDataReader dr)
    {
        return new ProductoViewModel
        {
            Id = dr.GetInt32(dr.GetOrdinal("Id")),
            CategoriaId = dr.GetInt32(dr.GetOrdinal("CategoriaId")),
            NombreCategoria = dr.GetString(dr.GetOrdinal("NombreCategoria")),
            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
            Descripcion = dr.IsDBNull(dr.GetOrdinal("Descripcion")) ? null : dr.GetString(dr.GetOrdinal("Descripcion")),
            Precio = dr.GetDecimal(dr.GetOrdinal("Precio")),
            Stock = dr.GetInt32(dr.GetOrdinal("Stock")),
            ImagenUrl = dr.IsDBNull(dr.GetOrdinal("ImagenUrl")) ? null : dr.GetString(dr.GetOrdinal("ImagenUrl"))
        };
    }
}