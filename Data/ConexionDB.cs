using Microsoft.Data.SqlClient;

namespace JoyeriaMorgan.Data;

public class ConexionBD
{
    private readonly string _cadena;

    public ConexionBD(IConfiguration configuration)
    {
        _cadena = configuration.GetConnectionString("TiendaDB")
                    ?? throw new InvalidOperationException("No se encontro la cadena 'TiendaDB' en appsettings.json");
    }

    public SqlConnection ObtenerConexion() => new SqlConnection(_cadena);

    public bool ProbarConexion(out string mensaje)
    {
        try
        {
            using var cn = ObtenerConexion();
            cn.Open();
            mensaje = $"Conexion exitosa a '{cn.Database}' en el servidor '{cn.DataSource}'";
            return true;
        }
        catch (Exception ex)
        {
            mensaje = $"Error de conexion: {ex.Message}";
            return false;
        }
    }

    public int ContarProductos()
    {
        using var cn = ObtenerConexion();
        cn.Open();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Producto", cn);
        return (int)cmd.ExecuteScalar();
    }
}
