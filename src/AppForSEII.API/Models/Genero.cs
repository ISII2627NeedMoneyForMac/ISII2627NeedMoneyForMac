using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Genero
    {
        public Genero(){ }

        //Constructores
        public Genero(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        //Atributos
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        //Relaciones
        public List<Libro> Libros { get; set; }
    }
}