using System.Data;
using JoyeriaMorgan.Models;
using Microsoft.Data.SqlClient;


namespace JoyeriaMorgan.Data;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ConexionBD _bd;

    public UsuarioRepositorio(ConexionBD bd)
    {
        _bd = bd;
    }

    public UsuarioViewModel? Login(string correo, string clave)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Usuario_Login", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Correo", correo);
        cmd.Parameters.AddWithValue("@Clave", clave);

        using var dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            return new UsuarioViewModel
            {
                Id = dr.GetInt32(dr.GetOrdinal("Id")),
                Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                Correo = dr.GetString(dr.GetOrdinal("Correo")),
                Clave = dr.GetString(dr.GetOrdinal("Clave")),
                Rol = dr.GetString(dr.GetOrdinal("Rol"))
            };
        }
        return null;
    }

    public bool ExisteCorreo(string correo)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Usuario_ExisteCorreo", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Correo", correo);

        var paramExiste = new SqlParameter("@Existe", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(paramExiste);

        cmd.ExecuteNonQuery();

        return paramExiste.Value != DBNull.Value && (bool)paramExiste.Value;
    }

    public void Registrar(UsuarioViewModel u)
    {
        using var cn = _bd.ObtenerConexion();
        cn.Open();

        using var cmd = new SqlCommand("dbo.sp_Usuario_Registrar", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Nombre", u.Nombre);
        cmd.Parameters.AddWithValue("@Correo", u.Correo);
        cmd.Parameters.AddWithValue("@Clave", u.Clave);

        cmd.ExecuteNonQuery();
    }
}