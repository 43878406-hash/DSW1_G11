using System.ComponentModel.DataAnnotations;

namespace JoyeriaMorgan.Models;

public class UsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(256, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    // Solo se usa en el formulario de registro; no se guarda en la base de datos.
    [Compare(nameof(Clave), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarClave { get; set; }

    public string Rol { get; set; } = "Cliente";
}
