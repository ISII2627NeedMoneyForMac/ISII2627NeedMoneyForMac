using AppForSEII.Web.API;   // namespace donde NSwag generó el cliente y los DTO

namespace AppForSEII.Web
{
    // Guarda el carrito de la compra mientras el usuario navega entre páginas
    public class CompraStateContainer
    {
        // Compra que se enviará al POST (CreateCompra) en la siguiente tarea
        public CompraForCreateDTO Compra { get; private set; } = new CompraForCreateDTO { Items = new List<CompraItemDTO>() };

        // Precio total = suma de (precio * cantidad) de cada libro del carrito
        public double PrecioTotal => Compra.Items.Sum(i => i.PrecioCompra * i.Cantidad);

        // Añade un libro al carrito (con cantidad 1); si ya está, no lo duplica
        public void AddLibroToCompra(LibroParaComprarDTO libro)
        {
            if (!Compra.Items.Any(i => i.LibroId == libro.Id))
                Compra.Items.Add(new CompraItemDTO
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Genero = libro.Genero,
                    Editorial = libro.Editorial,
                    PrecioCompra = libro.PrecioCompra,
                    Cantidad = 1
                });
        }

        public void RemoveCompraItem(CompraItemDTO item) => Compra.Items.Remove(item);

        // La cantidad mínima es 1
        public void ChangeCantidad(CompraItemDTO item, int cantidad) => item.Cantidad = cantidad < 1 ? 1 : cantidad;

        public void ClearCarrito() => Compra = new CompraForCreateDTO { Items = new List<CompraItemDTO>() };
    }
}