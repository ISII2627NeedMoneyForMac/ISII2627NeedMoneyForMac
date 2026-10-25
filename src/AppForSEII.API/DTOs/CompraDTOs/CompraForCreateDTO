using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.CompraDTOs
{
    public class CompraForCreateDTO
    {
        public CompraForCreateDTO()
        {
        }

        public CompraForCreateDTO(string usuarioUserName, string nombre, string apellidos, string direccion,
                                  string telefono, int metodoPagoId, string? codigoDescuento, List<CompraItemDTO> items)
        {
            UsuarioUserName = usuarioUserName;
            Nombre = nombre;
            Apellidos = apellidos;
            Direccion = direccion;
            Telefono = telefono;
            MetodoPagoId = metodoPagoId;
            CodigoDescuento = codigoDescuento;
            Items = items;
        }

        [Required]
        public string UsuarioUserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public int MetodoPagoId { get; set; }

        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }

        [Required]
        public List<CompraItemDTO> Items { get; set; } = new List<CompraItemDTO>();

        public override bool Equals(object? obj)
        {
            return obj is CompraForCreateDTO dto &&
                   UsuarioUserName == dto.UsuarioUserName &&
                   Nombre == dto.Nombre &&
                   Apellidos == dto.Apellidos &&
                   Direccion == dto.Direccion &&
                   Telefono == dto.Telefono &&
                   MetodoPagoId == dto.MetodoPagoId &&
                   CodigoDescuento == dto.CodigoDescuento &&
                   Items.SequenceEqual(dto.Items);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UsuarioUserName, Nombre, Apellidos, Direccion, Telefono, MetodoPagoId, CodigoDescuento);
        }
    }
}