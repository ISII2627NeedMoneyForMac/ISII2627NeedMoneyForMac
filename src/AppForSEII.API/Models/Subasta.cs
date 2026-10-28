using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Subasta
    {
        // Constructores
        public Subasta()
        {
            SubastaItems = new List<SubastaItem>();
        }

        public Subasta(int id, DateTime fechaSubasta, double precioSubasta) : this()
        {
            Id = id;
            FechaSubasta = fechaSubasta;
            PrecioSubasta = precioSubasta;
        }

        // Atributos
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime FechaSubasta { get; set; }

        [Required]
        public double PrecioSubasta { get; set; }

        // Relaciones
        public IList<SubastaItem> SubastaItems { get; set; } = new List<SubastaItem>();
        public MetodoPago? MetodoPago { get; set; }
        public ApplicationUser? Usuario { get; set; }
    }
}