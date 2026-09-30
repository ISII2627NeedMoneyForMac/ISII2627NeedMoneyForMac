using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Compra
    {
        //Constructores
        public Compra()
        {
        }

        public Compra(DateTime fechaCompra, decimal precioTotal, string usuarioId, int metodoPagoId, string? codigoDescuento = null)
        {
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
            UsuarioId = usuarioId;
            MetodoPagoId = metodoPagoId;
            CodigoDescuento = codigoDescuento;
        }

        //Atributos
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioTotal { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public int MetodoPagoId { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string? CodigoDescuento { get; set; }


        //Relaciones
     
        public ApplicationUser Usuario { get; set; }

        public List<MetodoPago> MetodosPago { get; set; } 

        public ICollection<CompraItem>? CompraItems { get; set; }
    }
}