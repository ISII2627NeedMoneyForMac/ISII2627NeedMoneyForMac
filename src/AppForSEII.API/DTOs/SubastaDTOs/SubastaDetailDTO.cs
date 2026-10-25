using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.API.DTOs.SubastaDTOs
{
    public class SubastaDetailDTO
    {
        public SubastaDetailDTO(int id, string nombre, string apellidos, string telefono,
            DateTime fechaSubasta, string? descripcion, IList<SubastaItemDTO> subastaItems)
        {
            Id = id;
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            FechaSubasta = fechaSubasta;
            Descripcion = descripcion;
            SubastaItems = subastaItems ?? throw new ArgumentNullException(nameof(subastaItems));
        }

        public SubastaDetailDTO()
        {
            SubastaItems = new List<SubastaItemDTO>();
        }

        public int Id { get; set; }

        public string Nombre { get; set; } = "Nombre";

        public string Apellidos { get; set; } = "Apellidos";

        public string Telefono { get; set; } = "600000000";

        public DateTime FechaSubasta { get; set; }

        public string? Descripcion { get; set; }

        public IList<SubastaItemDTO> SubastaItems { get; set; } = new List<SubastaItemDTO>();

        public decimal TotalPrice
        {
            get
            {
                return (decimal)SubastaItems.Sum(si => si.PrecioPuja);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is SubastaDetailDTO dto &&
                   Id == dto.Id &&
                   Nombre == dto.Nombre &&
                   Apellidos == dto.Apellidos &&
                   Telefono == dto.Telefono &&
                   FechaSubasta == dto.FechaSubasta &&
                   Descripcion == dto.Descripcion &&
                   SubastaItems.SequenceEqual(dto.SubastaItems) &&
                   TotalPrice == dto.TotalPrice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Nombre, Apellidos, Telefono, FechaSubasta, Descripcion);
        }
    }
}