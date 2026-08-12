using System.Data;
using Microsoft.Data.SqlClient;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public class VentaRepositorio : IVentaRepositorio
{
    private readonly ConexionBD _bd;

    public VentaRepositorio(ConexionBD bd)
    {
        _bd = bd;
    }

    public int RegistrarVenta(int usuarioId, string direccionEnvio, List<ItemCarritoViewModel> items)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        // 1. Iniciamos la transacción SQL en ADO.NET
        using SqlTransaction tran = cn.BeginTransaction();

        try
        {
            // 2. Calcular el total del carrito
            decimal totalGeneral = items.Sum(i => i.Subtotal);

            // 3. Registrar la cabecera (Venta) usando Stored Procedure
            using var cmdCabecera = new SqlCommand("dbo.sp_Venta_CrearCabecera", cn, tran);
            cmdCabecera.CommandType = CommandType.StoredProcedure;
            cmdCabecera.Parameters.AddWithValue("@UsuarioId", usuarioId);
            cmdCabecera.Parameters.AddWithValue("@Total", totalGeneral);
            cmdCabecera.Parameters.AddWithValue("@DireccionEnvio", direccionEnvio);

            // Parámetro OUTPUT para capturar el Id de la venta recién creada
            var paramVentaId = new SqlParameter("@VentaId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmdCabecera.Parameters.Add(paramVentaId);

            cmdCabecera.ExecuteNonQuery();

            int nuevaVentaId = (int)paramVentaId.Value;

            // 4. Registrar cada ítem en DetalleVenta y descontar Stock
            foreach (var item in items)
            {
                using var cmdDetalle = new SqlCommand("dbo.sp_Venta_RegistrarDetalleYStock", cn, tran);
                cmdDetalle.CommandType = CommandType.StoredProcedure;
                cmdDetalle.Parameters.AddWithValue("@VentaId", nuevaVentaId);
                cmdDetalle.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", item.Precio);

                cmdDetalle.ExecuteNonQuery();
            }

            // 5. Si todo salió perfecto, confirmamos la transacción
            tran.Commit();
            return nuevaVentaId;
        }
        catch
        {
            // Si ocurre cualquier error, deshacemos todos los INSERT y descuentos de stock
            tran.Rollback();
            throw;
        }
    }
}