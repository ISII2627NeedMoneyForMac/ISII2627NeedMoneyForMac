using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class ReposicionItem
    {
        [Required]
        public int ReposicionId { get; set; }

        [Required]
        public int LibroId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadReposicion { get; set; }
    }
}