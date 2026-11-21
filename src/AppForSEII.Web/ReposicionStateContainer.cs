using AppForSEII.Web.API;

namespace AppForSEII.Web
{
    public class ReposicionStateContainer
    {
        public ReposicionForCreateDTO Reposicion { get; private set; } = new ReposicionForCreateDTO()
        {
            ReposicionItems = new List<ReposicionItemDTO>()
        };

        public void AddLibroToReposicion(LibroParaReponerDTO libro)
        {
            if (!Reposicion.ReposicionItems.Any(ri => ri.LibroId == libro.Id))
                Reposicion.ReposicionItems.Add(new ReposicionItemDTO()
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Autor = libro.Autor,
                    PrecioReposicion = libro.PrecioReposicion,
                    CantidadReposicion = 2   // el DTO exige mínimo 2
                });
        }

        public void RemoveReposicionItem(ReposicionItemDTO item)
        {
            Reposicion.ReposicionItems.Remove(item);
        }

        public void ClearReposicionCart()
        {
            Reposicion.ReposicionItems.Clear();
        }

        public void ReposicionProcessed()
        {
            // la reposición ya se ha creado: empezamos con un objeto vacío
            Reposicion = new ReposicionForCreateDTO()
            {
                ReposicionItems = new List<ReposicionItemDTO>()
            };
        }
    }
}