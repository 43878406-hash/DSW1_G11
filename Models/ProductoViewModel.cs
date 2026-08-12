using System.ComponentModel.DataAnnotations;

namespace JoyeriaMorgan.Models;

public class ProductoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione una categoria")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    public string? NombreCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la joya es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser mayor a S/ 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, 10000, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    [Display(Name = "Imagen")]
    public string? ImagenUrl { get; set; }
}