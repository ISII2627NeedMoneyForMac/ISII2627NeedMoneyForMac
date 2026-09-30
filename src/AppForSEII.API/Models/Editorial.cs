using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Editorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Nombre { get; set; }
    }
}