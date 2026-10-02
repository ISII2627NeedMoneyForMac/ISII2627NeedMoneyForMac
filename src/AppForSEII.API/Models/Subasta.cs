using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Subasta
    {
        //Constructores
        public Subasta(){ }

        public Subasta(int id, DateTime fechaSubasta, double precioSubasta)
        {
            Id = id;
            FechaSubasta = fechaSubasta;
            PrecioSubasta = precioSubasta;
        }

        //Atributos
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime FechaSubasta { get; set; }

        [Required]
         public double PrecioSubasta { get; set; }

        //Relaciones
        public IList<SubastaItem> SubastaItems { get; set; } 
        public List<MetodoPago> MetodosPago { get; set; } 
        public ApplicationUser Usuario { get; set; }

    }
}