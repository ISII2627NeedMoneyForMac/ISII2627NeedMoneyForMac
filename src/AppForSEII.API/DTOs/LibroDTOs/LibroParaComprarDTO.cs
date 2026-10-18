using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO()
        {
        }

        public LibroParaComprarDTO(int id, string titulo, string autor, string genero, DateTime fechaLanzamiento, decimal precioCompra)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Genero = genero;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
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
        [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo")]
        public decimal PrecioCompra { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   Genero == dto.Genero &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   PrecioCompra == dto.PrecioCompra;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, Genero, FechaLanzamiento, PrecioCompra);
        }
    }
}