using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.ReposicionDTOs
{
    public class ReposicionItemDTO
    {
        public ReposicionItemDTO(int libroId, string titulo, string autor, decimal precioReposicion, int cantidadReposicion)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            PrecioReposicion = precioReposicion;
            CantidadReposicion = cantidadReposicion;
        }

        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public decimal PrecioReposicion { get; set; }

        [Required(ErrorMessage = "La cantidad de reposición es obligatoria")]
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad de reposición debe ser mayor que 1")]
        public int CantidadReposicion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ReposicionItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   PrecioReposicion == dto.PrecioReposicion &&
                   CantidadReposicion == dto.CantidadReposicion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, PrecioReposicion, CantidadReposicion);
        }
    }
}