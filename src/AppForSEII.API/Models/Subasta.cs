using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Subasta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
         [System.ComponentModel.DataAnnotations.Display(Name = "FechaSubasta")]
        public DateTime FechaSubasta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
         public decimal PrecioSubasta { get; set; }

    }
}