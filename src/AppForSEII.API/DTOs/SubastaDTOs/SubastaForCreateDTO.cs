using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs.SubastaDTOs
{
    public class SubastaForCreateDTO
    {
        public SubastaForCreateDTO(string userName, string nombre, string apellidos, string telefono,
            MetodoPagoSubastaTipos metodoPago, string? descripcion, IList<SubastaItemDTO> subastaItems)
        {
            UsuarioUserName = userName;
            Nombre = nombre ;
            Apellidos = apellidos;
            Telefono = telefono ;
            MetodoPago = metodoPago;
            Descripcion = descripcion;
            SubastaItems = subastaItems;
        }

        public SubastaForCreateDTO()
        {
            SubastaItems = new List<SubastaItemDTO>();
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
        [Phone]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public MetodoPagoSubastaTipos MetodoPago { get; set; } = MetodoPagoSubastaTipos.Visa;

        // Opcional: entre 20 y 100 caracteres según la especificación del Caso de Uso 3
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]    
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }

        public IList<SubastaItemDTO> SubastaItems { get; set; } = new List<SubastaItemDTO>();

        public decimal TotalPrice
        {
            get
            {
                return (decimal)SubastaItems.Sum(si => si.PrecioPuja);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is SubastaForCreateDTO dto &&
                   UsuarioUserName == dto.UsuarioUserName &&
                   Nombre == dto.Nombre &&
                   Apellidos == dto.Apellidos &&
                   Telefono == dto.Telefono &&
                   MetodoPago == dto.MetodoPago &&
                   Descripcion == dto.Descripcion &&
                   SubastaItems.SequenceEqual(dto.SubastaItems) &&
                   TotalPrice == dto.TotalPrice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UsuarioUserName, Nombre, Apellidos, Telefono, MetodoPago, Descripcion);
        }
    }

    public enum MetodoPagoSubastaTipos
    {
        Visa,
        GooglePay,
        PayPal
    }
}