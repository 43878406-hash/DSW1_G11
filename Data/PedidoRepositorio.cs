using System.Data;
using Microsoft.Data.SqlClient;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public class PedidoRepositorio : IPedidoRepositorio
{
    private readonly ConexionBD _bd;

    public PedidoRepositorio(ConexionBD bd)
    {
        _bd = bd;
    }

    public List<PedidoViewModel> ListarPorUsuario(int usuarioId)
    {
        var lista = new List<PedidoViewModel>();
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        const string sql = @"
            SELECT Id, Fecha, Total, DireccionEnvio, Estado
            FROM dbo.Venta
            WHERE UsuarioId = @UsuarioId
            ORDER BY Fecha DESC;";

        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            lista.Add(new PedidoViewModel
            {
                Id = dr.GetInt32(0),
                Fecha = dr.GetDateTime(1),
                Total = dr.GetDecimal(2),
                DireccionEnvio = dr.GetString(3),
                Estado = dr.GetString(4)
            });
        }

        return lista;
    }

    public PedidoViewModel? ObtenerDetalle(int ventaId, int usuarioId)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        // 1. Validamos cabecera perteneciente al usuario
        const string sqlVenta = @"
            SELECT Id, Fecha, Total, DireccionEnvio, Estado
            FROM dbo.Venta
            WHERE Id = @VentaId AND UsuarioId = @UsuarioId;";

        using var cmdVenta = new SqlCommand(sqlVenta, cn);
        cmdVenta.Parameters.AddWithValue("@VentaId", ventaId);
        cmdVenta.Parameters.AddWithValue("@UsuarioId", usuarioId);

        using var drVenta = cmdVenta.ExecuteReader();
        if (!drVenta.Read()) return null;

        var pedido = new PedidoViewModel
        {
            Id = drVenta.GetInt32(0),
            Fecha = drVenta.GetDateTime(1),
            Total = drVenta.GetDecimal(2),
            DireccionEnvio = drVenta.GetString(3),
            Estado = drVenta.GetString(4)
        };
        drVenta.Close();

        // 2. Cargamos el detalle del pedido
        const string sqlDetalle = @"
            SELECT dv.ProductoId, p.Nombre, p.ImagenUrl, dv.Cantidad, dv.PrecioUnitario
            FROM dbo.DetalleVenta dv
            INNER JOIN dbo.Producto p ON dv.ProductoId = p.Id
            WHERE dv.VentaId = @VentaId;";

        using var cmdDet = new SqlCommand(sqlDetalle, cn);
        cmdDet.Parameters.AddWithValue("@VentaId", ventaId);

        using var drDet = cmdDet.ExecuteReader();
        while (drDet.Read())
        {
            pedido.Detalles.Add(new DetallePedidoViewModel
            {
                ProductoId = drDet.GetInt32(0),
                NombreProducto = drDet.GetString(1),
                ImagenUrl = drDet.IsDBNull(2) ? null : drDet.GetString(2),
                Cantidad = drDet.GetInt32(3),
                PrecioUnitario = drDet.GetDecimal(4)
            });
        }

        return pedido;
    }
}