using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public interface IUsuarioRepositorio
{
    UsuarioViewModel? Login(string correo, string clave);
    bool ExisteCorreo(string correo);
    void Registrar(UsuarioViewModel usuario);
}
