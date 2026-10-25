using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaSubastarDTO
    {
        public LibroParaSubastarDTO()
        {
        }

        public LibroParaSubastarDTO(int id, string titulo, string autor, string editorial, string tipoLibro)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            TipoLibro = tipoLibro;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "El autor debe tener un máximo de 100 caracteres")]
        public string Autor { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "La editorial debe tener un máximo de 100 caracteres")]
        public string Editorial { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "El tipo de libro debe tener un máximo de 100 caracteres")]
        public string TipoLibro { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   Editorial == dto.Editorial &&
                   TipoLibro == dto.TipoLibro;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, Editorial, TipoLibro);
        }
    }
}