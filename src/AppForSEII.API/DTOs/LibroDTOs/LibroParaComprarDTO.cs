using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO()
        {
        }

        public LibroParaComprarDTO(int id, string titulo, decimal precioCompra, DateTime fechaLanzamiento, string genero, string editorial)
        {
            Id = id;
            Titulo = titulo;
            PrecioCompra = precioCompra;
            FechaLanzamiento = fechaLanzamiento;
            Genero = genero;
            Editorial = editorial;
        }

        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo")]
        public decimal PrecioCompra { get; set; }

        public DateTime FechaLanzamiento { get; set; }

        [StringLength(100, ErrorMessage = "El género debe tener un máximo de 100 caracteres")]
        public string Genero { get; set; } = "Género";

        [StringLength(100, ErrorMessage = "La editorial debe tener un máximo de 100 caracteres")]
        public string Editorial { get; set; } = "Editorial";

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   PrecioCompra == dto.PrecioCompra &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   Genero == dto.Genero &&
                   Editorial == dto.Editorial;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, PrecioCompra, FechaLanzamiento, Genero, Editorial);
        }
    }
}