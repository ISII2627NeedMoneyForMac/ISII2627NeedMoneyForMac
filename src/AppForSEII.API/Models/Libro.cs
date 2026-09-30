using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Libro
    {
        //Constructores
        public Libro() { }

        public Libro(string titulo, string autor, DateTime fechaLanzamiento, decimal precioCompra, decimal precioReposicion, int stock)
        {
            Titulo = titulo;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            PrecioReposicion = precioReposicion;
            Stock = stock;
        }

        //Atributos
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Autor { get; set; } = string.Empty;

        [Required]
        public DateTime FechaLanzamiento { get; set; }

        [Required]
      
        public decimal PrecioCompra { get; set; }

        [Required]
       
        public decimal PrecioReposicion { get; set; }

        [Required]
        public int Stock { get; set; }


        //Relaciones
        public Genero Genero { get; set; }

        public Editorial Editorial { get; set; }

        public List<CompraItem>? CompraItems { get; set; }
    }
}