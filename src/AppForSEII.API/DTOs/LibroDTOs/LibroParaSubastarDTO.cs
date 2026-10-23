using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaSubastarDTO
    {
        public LibroParaSubastarDTO()
        {
        }

        public LibroParaSubastarDTO(int id, string titulo, string autor, string genero, DateTime fechaLanzamiento, int precioPuja, string tipolibro, string? descripcion)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Genero = genero;
            FechaLanzamiento = fechaLanzamiento;
            PrecioPuja = precioPuja;
            TipoLibro = tipolibro;
            Descripcion = descripcion;
        }

        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(100, ErrorMessage = "El autor debe tener un máximo de 100 caracteres")]
        public string Autor { get; set; } = "Autor";

        [StringLength(100, ErrorMessage = "El género debe tener un máximo de 100 caracteres")]
        public string Genero { get; set; } = "Género";

        public DateTime FechaLanzamiento { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El precio de puja no puede ser negativo")]
        public int PrecioPuja { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El tipo de libro debe tener un máximo de 100 caracteres")]
        public string TipoLibro { get; set; } = "Tipo de Libro";

        [StringLength(500, ErrorMessage = "La descripción debe tener un máximo de 500 caracteres")]
        public string? Descripcion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   Genero == dto.Genero &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   PrecioPuja == dto.PrecioPuja &&
                   TipoLibro == dto.TipoLibro &&
                   Descripcion == dto.Descripcion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, Genero, FechaLanzamiento, PrecioPuja, TipoLibro, Descripcion);
        }
    }
}