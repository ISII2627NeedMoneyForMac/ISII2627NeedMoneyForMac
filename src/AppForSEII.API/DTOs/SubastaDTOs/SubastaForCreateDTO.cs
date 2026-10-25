using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs.SubastaDTOs
{
    public class SubastaForCreateDTO
    {
        public SubastaForCreateDTO(string userName, string nombre, string apellidos, string telefono,
            MetodoPagoTipos metodoPago, string? descripcion, IList<SubastaItemDTO> subastaItems)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            Descripcion = descripcion;
            SubastaItems = subastaItems ?? throw new ArgumentNullException(nameof(subastaItems));
        }

        public SubastaForCreateDTO()
        {
            SubastaItems = new List<SubastaItemDTO>();
        }

        [EmailAddress]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica el nombre de usuario")]
        public string UserName { get; set; } = "usuario@uclm.es";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu nombre")]
        [StringLength(50, ErrorMessage = "El nombre debe tener un máximo de 50 caracteres")]
        public string Nombre { get; set; } = "Nombre";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tus apellidos")]
        [StringLength(100, ErrorMessage = "Los apellidos deben tener un máximo de 100 caracteres")]
        public string Apellidos { get; set; } = "Apellidos";

        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu teléfono")]
        [StringLength(20, ErrorMessage = "El teléfono debe tener un máximo de 20 caracteres")]
        public string Telefono { get; set; } = "600000000";

        [Required]
        public MetodoPagoTipos MetodoPago { get; set; } = MetodoPagoTipos.Visa;

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
                   UserName == dto.UserName &&
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
            return HashCode.Combine(UserName, Nombre, Apellidos, Telefono, MetodoPago, Descripcion);
        }
    }

    public enum MetodoPagoTipos
    {
        Visa,
        GooglePay,
        PayPal
    }
}