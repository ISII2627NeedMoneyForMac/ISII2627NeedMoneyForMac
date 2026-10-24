using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs.ReposicionDTOs
{
    public class ReposicionForCreateDTO
    {
        public ReposicionForCreateDTO(string userName, string nombre, string apellido1, string apellido2,
            string telefono, MetodoPagoTipos metodoPago, string? comentario, IList<ReposicionItemDTO> reposicionItems)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellido1 = apellido1 ?? throw new ArgumentNullException(nameof(apellido1));
            Apellido2 = apellido2 ?? throw new ArgumentNullException(nameof(apellido2));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            Comentario = comentario;
            ReposicionItems = reposicionItems ?? throw new ArgumentNullException(nameof(reposicionItems));
        }

        public ReposicionForCreateDTO()
        {
            ReposicionItems = new List<ReposicionItemDTO>();
        }

        [EmailAddress]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica el nombre de usuario")]
        public string UserName { get; set; } = "admin@uclm.es";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu nombre")]
        [StringLength(50, ErrorMessage = "El nombre debe tener un máximo de 50 caracteres")]
        public string Nombre { get; set; } = "Nombre";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu primer apellido")]
        [StringLength(50, ErrorMessage = "El primer apellido debe tener un máximo de 50 caracteres")]
        public string Apellido1 { get; set; } = "Apellido1";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu segundo apellido")]
        [StringLength(50, ErrorMessage = "El segundo apellido debe tener un máximo de 50 caracteres")]
        public string Apellido2 { get; set; } = "Apellido2";

        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, indica tu teléfono")]
        [StringLength(20, ErrorMessage = "El teléfono debe tener un máximo de 20 caracteres")]
        public string Telefono { get; set; } = "600000000";

        [Required]
        public MetodoPagoTipos MetodoPago { get; set; } = MetodoPagoTipos.Visa;

        // Opcional: si se informa, debe tener entre 20 y 100 caracteres
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres")]
        public string? Comentario { get; set; }

        public IList<ReposicionItemDTO> ReposicionItems { get; set; } = new List<ReposicionItemDTO>();

        public decimal TotalPrice
        {
            get
            {
                return ReposicionItems.Sum(ri => ri.PrecioReposicion * ri.CantidadReposicion);
            }
        }

        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }

        public override bool Equals(object? obj)
        {
            return obj is ReposicionForCreateDTO dto &&
                   UserName == dto.UserName &&
                   Nombre == dto.Nombre &&
                   Apellido1 == dto.Apellido1 &&
                   Apellido2 == dto.Apellido2 &&
                   Telefono == dto.Telefono &&
                   MetodoPago == dto.MetodoPago &&
                   Comentario == dto.Comentario &&
                   ReposicionItems.SequenceEqual(dto.ReposicionItems) &&
                   TotalPrice == dto.TotalPrice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserName, Nombre, Apellido1, Apellido2, Telefono, MetodoPago, Comentario);
        }
    }

    public enum MetodoPagoTipos
    {
        Visa,
        GooglePay,
        PayPal
    }
}
