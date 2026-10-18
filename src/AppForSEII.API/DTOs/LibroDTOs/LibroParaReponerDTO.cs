using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaReponerDTO
    {
        public LibroParaReponerDTO()
        {
        }

        public LibroParaReponerDTO(int id, string titulo, string autor, int cantidadReposicion)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            CantidadReposicion = cantidadReposicion;
        }

        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(100, ErrorMessage = "El autor debe tener un máximo de 100 caracteres")]
        public string Autor { get; set; } = "Autor";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad mínima a reponer es 1")]
        public int CantidadReposicion { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaReponerDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   CantidadReposicion == dto.CantidadReposicion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, CantidadReposicion);
        }
    }
}