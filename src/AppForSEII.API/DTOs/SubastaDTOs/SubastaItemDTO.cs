using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.SubastaDTOs
{
    public class SubastaItemDTO
    {
        public SubastaItemDTO() { }

        public SubastaItemDTO(int libroId, string titulo, string editorial, string autor, double precioPuja)
        {
            LibroId = libroId;
            Titulo = titulo;
            Editorial = editorial;
            Autor = autor;
            PrecioPuja = precioPuja;
        }

        public int LibroId { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(100, ErrorMessage = "La editorial debe tener un máximo de 100 caracteres")]
        public string Editorial { get; set; } = "Editorial";
        
        [StringLength(100, ErrorMessage = "El autor debe tener un máximo de 100 caracteres")]
        public string Autor { get; set; } = "Autor";

    
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de la puja debe ser mayor que 0")]
        public double PrecioPuja { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Editorial == dto.Editorial &&
                   Autor == dto.Autor &&
                   PrecioPuja == dto.PrecioPuja;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Editorial, Autor, PrecioPuja);
        }
    }
}