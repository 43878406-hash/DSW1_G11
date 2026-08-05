using System.ComponentModel.DataAnnotations;

namespace JoyeriaMorgan.Models;

public class UsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido")]
    public string Correo { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cliente";
}