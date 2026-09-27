using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Subasta
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaSubasta { get; set; }

        [Column(TypeName = "decimal(18,2)")]
         public decimal PrecioSubasta { get; set; }

    }
}