using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class CompraItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 1")]
        public int Cantidad { get; set; }

        [Required]
        [ForeignKey(nameof(Libro))]
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        [Required]
        [ForeignKey(nameof(Compra))]
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }
    }
}