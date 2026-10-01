using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class SubastaItem
    {
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
        //Constructores
        public SubastaItem(){ }

        public SubastaItem(int subastaId, int libroId, int precioPuja, string? descripcion)
        {
            SubastaId = subastaId;
            LibroId = libroId;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        //Atribubtos
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
        [Required]
        public int SubastaId { get; set; }

        [Required]
        public int LibroId { get; set; }

        [Required]
        public int PrecioPuja { get; set; }

        public string? Descripcion { get; set; }

        //Relaciones
        public Libro Libro { get; set; }
        public Subasta Subasta { get; set; }
    }
}