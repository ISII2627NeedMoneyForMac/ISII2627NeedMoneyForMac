using AppForSEII.Web.API;   // namespace donde NSwag generó el cliente y los DTO

namespace AppForSEII.Web
{
    // Guarda la lista de pujas mientras el usuario navega entre páginas
    public class SubastaStateContainer
    {
        // Subasta que se enviará al POST (CreateSubasta) en la siguiente tarea
        public SubastaForCreateDTO Subasta { get; private set; } = new SubastaForCreateDTO()
        {
            SubastaItems = new List<SubastaItemDTO>()
        };

        // Precio total de la subasta = suma de las pujas de cada libro
        public double PrecioTotal => Subasta.SubastaItems.Sum(si => si.PrecioPuja);

        // Añade un libro a la lista de pujas; si ya está, no lo duplica
        public void AddLibroToSubasta(LibroParaSubastarDTO libro)
        {
            if (!Subasta.SubastaItems.Any(si => si.LibroId == libro.Id))
                Subasta.SubastaItems.Add(new SubastaItemDTO()
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Editorial = libro.Editorial,
                    Autor = libro.Autor,
                    PrecioPuja = 1   // el DTO exige una puja mínima de 1
                });
        }

        // Elimina un libro de la lista de pujas
        public void RemoveSubastaItem(SubastaItemDTO item)
        {
            Subasta.SubastaItems.Remove(item);
        }

        // Vacía la lista de pujas
        public void ClearSubastaCart()
        {
            Subasta.SubastaItems.Clear();
        }

        // La subasta ya se ha creado: empezamos con un objeto vacío
        public void SubastaProcessed()
        {
            Subasta = new SubastaForCreateDTO()
            {
                SubastaItems = new List<SubastaItemDTO>()
            };
        }
    }
}