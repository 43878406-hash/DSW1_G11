using System.Text.Json;
using JoyeriaMorgan.Models;

namespace JoyeriaMorgan.Helpers;

public static class SessionExtensions
{
    // Clave unica bajo la que se guarda el carrito en la Session.
    public const string ClaveCarrito = "CarritoJoyeriamorgan";

    public static void SetObjeto<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObjeto<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    // Suma las unidades del carrito. Lo usa el _Layout para pintar el globito
    // con el numero de joyas sobre el icono de la bolsa.
    public static int ContarItemsCarrito(this ISession session)
    {
        var carrito = session.GetObjeto<List<ItemCarritoViewModel>>(ClaveCarrito);
        return carrito?.Sum(i => i.Cantidad) ?? 0;
    }
}
