using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.CompraDTOs
{
    public class CompraItemDTO
    {
        public CompraItemDTO()
        {
        }

        public CompraItemDTO(int libroId, string titulo, string genero, string editorial, decimal precioCompra, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            PrecioCompra = precioCompra;
            Cantidad = cantidad;
        }

        public int LibroId { get; set; }

        [StringLength(100, ErrorMessage = "El título debe tener un máximo de 100 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(100, ErrorMessage = "El género debe tener un máximo de 100 caracteres")]
        public string Genero { get; set; } = "Género";

        [StringLength(100, ErrorMessage = "La editorial debe tener un máximo de 100 caracteres")]
        public string Editorial { get; set; } = "Editorial";

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public decimal PrecioCompra { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad mínima de compra es 1")]
        public int Cantidad { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is CompraItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Genero == dto.Genero &&
                   Editorial == dto.Editorial &&
                   PrecioCompra == dto.PrecioCompra &&
                   Cantidad == dto.Cantidad;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Genero, Editorial, PrecioCompra, Cantidad);
        }
    }
}