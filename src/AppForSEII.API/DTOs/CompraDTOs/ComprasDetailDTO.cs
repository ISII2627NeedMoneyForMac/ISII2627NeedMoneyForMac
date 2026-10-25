using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.CompraDTOs
{
    public class ComprasDetailDTO
    {
        public ComprasDetailDTO()
        {
        }

        public ComprasDetailDTO(int id, string? nombre, string? apellidos, string? direccion, string? telefono,
                                DateTime fechaCompra, decimal precioTotal, string metodoPago,
                                string? codigoDescuento, List<CompraItemDTO> items)
        {
            Id = id;
            Nombre = nombre;
            Apellidos = apellidos;
            Direccion = direccion;
            Telefono = telefono;
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
            MetodoPago = metodoPago;
            CodigoDescuento = codigoDescuento;
            Items = items;
        }

        public int Id { get; set; }

        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(50)]
        public string? Apellidos { get; set; }

        [StringLength(100)]
        public string? Direccion { get; set; }

        [Phone]
        public string? Telefono { get; set; }

        public DateTime FechaCompra { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio total no puede ser negativo")]
        public decimal PrecioTotal { get; set; }

        public string MetodoPago { get; set; } = "MetodoPago";

        [StringLength(10, MinimumLength = 5)]
        public string? CodigoDescuento { get; set; }

        public List<CompraItemDTO> Items { get; set; } = new List<CompraItemDTO>();

        public override bool Equals(object? obj)
        {
            return obj is ComprasDetailDTO dto &&
                   Id == dto.Id &&
                   Nombre == dto.Nombre &&
                   Apellidos == dto.Apellidos &&
                   Direccion == dto.Direccion &&
                   Telefono == dto.Telefono &&
                   FechaCompra == dto.FechaCompra &&
                   PrecioTotal == dto.PrecioTotal &&
                   MetodoPago == dto.MetodoPago &&
                   CodigoDescuento == dto.CodigoDescuento &&
                   Items.SequenceEqual(dto.Items);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Nombre, Apellidos, Direccion, Telefono, FechaCompra, PrecioTotal,
                                    HashCode.Combine(MetodoPago, CodigoDescuento));
        }
    }
}