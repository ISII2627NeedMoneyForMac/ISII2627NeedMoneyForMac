using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioTotal { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string? CodigoDescuento { get; set; }

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        [Required]
        [ForeignKey(nameof(MetodoPago))]
        public int MetodoPagoId { get; set; }
        public MetodoPago? MetodoPago { get; set; }
    }
}