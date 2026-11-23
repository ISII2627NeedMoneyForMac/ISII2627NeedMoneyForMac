using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{

    [PrimaryKey(nameof(SubastaId), nameof(LibroId))]
    public class SubastaItem
    {
        //Constructores
        public SubastaItem(){ }

        public SubastaItem(int subastaId, int libroId, int precioPuja, string? descripcion)
        {
            SubastaId = subastaId;
            LibroId = libroId;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

       // Constructor con las relaciones: recibe el libro y la subasta a la que pertenece la puja
        public SubastaItem(Libro libro, Subasta subasta, int precioPuja, string? descripcion)
        {
            Libro = libro;
            LibroId = libro.Id;
            Subasta = subasta;
            SubastaId = subasta.Id;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        //Atribubtos
        
        public int SubastaId { get; set; }


        public int LibroId { get; set; }

        [Required]
        public int PrecioPuja { get; set; }

        public string? Descripcion { get; set; }

        //Relaciones
        public Libro Libro { get; set; } = null!;
        public Subasta Subasta { get; set; } = null!;
    }
}