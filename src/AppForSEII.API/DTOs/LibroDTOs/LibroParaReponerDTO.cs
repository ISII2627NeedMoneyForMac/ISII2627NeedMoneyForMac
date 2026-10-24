using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaReponerDTO
    {
        public LibroParaReponerDTO()
        {
        }
        public LibroParaReponerDTO(int id, string titulo, string autor, decimal precioReposicion)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            PrecioReposicion = precioReposicion;
        }

        public LibroParaReponerDTO(int id, string titulo, string genero, DateTime fechaLanzamiento, decimal precioReposicion, string autor)
        {
            Id = id;
            Titulo = titulo;
            Genero = genero;
            FechaLanzamiento = fechaLanzamiento;
            PrecioReposicion = precioReposicion;
            Autor = autor;
        }

        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(50, ErrorMessage = "El género debe tener un máximo de 50 caracteres")]
        public string Genero { get; set; } = "Género";

        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; } = DateTime.Today;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de reposición debe ser mayor que 0")]
        public decimal PrecioReposicion { get; set; } = 0.01m;

        [StringLength(100, ErrorMessage = "El autor debe tener un máximo de 100 caracteres")]
        public string Autor { get; set; } = "Autor";

        public override bool Equals(object? obj)
        {
            return obj is LibroParaReponerDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Genero == dto.Genero &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   PrecioReposicion == dto.PrecioReposicion &&
                   Autor == dto.Autor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Genero, FechaLanzamiento, PrecioReposicion, Autor);
        }
    }
}
