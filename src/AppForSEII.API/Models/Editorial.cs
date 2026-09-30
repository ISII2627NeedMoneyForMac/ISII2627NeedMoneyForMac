using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Editorial
    {
        //Constructores
        public Editorial(){ }

        public Editorial(int id, int nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        //Atributos
        [Key]
        public int Id { get; set; }

        [Required]
        public int Nombre { get; set; }

        //Relaciones
        public List<Libro> Libros { get; set; } 
    }
}