using System;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs.ReposicionDTOs
{
    public class ReposicionDetailDTO : ReposicionForCreateDTO
    {
        public ReposicionDetailDTO(int id, DateTime fechaReposicion, string userName, string nombre,
            string apellido1, string apellido2, string telefono, MetodoPagoTipos metodoPago,
            string? comentario, IList<ReposicionItemDTO> reposicionItems)
            : base(userName,
                   nombre,
                   apellido1,
                   apellido2,
                   telefono,
                   metodoPago,
                   comentario,
                   reposicionItems)
        {
            Id = id;
            FechaReposicion = fechaReposicion;
        }

        public int Id { get; set; }

        public DateTime FechaReposicion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ReposicionDetailDTO dto &&
                   base.Equals(obj) &&
                   TotalPrice == dto.TotalPrice &&
                   Id == dto.Id &&
                   CompareDate(FechaReposicion, dto.FechaReposicion);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaReposicion);
        }
    }
}
