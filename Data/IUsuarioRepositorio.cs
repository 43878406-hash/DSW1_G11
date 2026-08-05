using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Data;

public interface IUsuarioRepositorio
{
    UsuarioViewModel? Login(string correo, string clave);
    void Registrar(UsuarioViewModel usuario);
}