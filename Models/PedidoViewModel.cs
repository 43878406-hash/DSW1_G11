namespace JoyeriaMorgan.Models;

public class PedidoViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string DireccionEnvio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<DetallePedidoViewModel> Detalles { get; set; } = new();
}

public class DetallePedidoViewModel
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}