using System.ComponentModel.DataAnnotations;

namespace JoyeriaMorgan.Models;

public class CategoriaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    [Display(Name = "Nombre de la categoría")]
    public string Nombre { get; set; } = string.Empty;

    // Cantidad de joyas asociadas. Se usa para mostrar el conteo en la tabla
    // y para impedir que se elimine una categoria que todavia tiene productos.
    [Display(Name = "Joyas registradas")]
    public int TotalProductos { get; set; }
}
