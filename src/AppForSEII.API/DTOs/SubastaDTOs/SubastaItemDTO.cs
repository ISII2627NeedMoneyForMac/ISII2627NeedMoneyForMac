using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.SubastaDTOs
{
    public class SubastaItemDTO
    {
        public SubastaItemDTO() { }

        public SubastaItemDTO(int libroId, string titulo, string autor, double precioPuja)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            PrecioPuja = precioPuja;
        }

        [Required]
        public int LibroId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Autor { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de la puja debe ser mayor que 0")]
        public double PrecioPuja { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   PrecioPuja == dto.PrecioPuja;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, PrecioPuja);
        }
    }
}