using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class SubastaItem
    {
        [Required]
        public int SubastaId { get; set; }

        [Required]
        public int LibroId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int PrecioPuja { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}