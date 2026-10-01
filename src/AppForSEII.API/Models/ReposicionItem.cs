using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class ReposicionItem
    {
        public ReposicionItem()
        {
        }

        public ReposicionItem(int reposicionId, int libroId, int cantidadReposicion)
        {
            ReposicionId = reposicionId;
            LibroId = libroId;
            CantidadReposicion = cantidadReposicion;
        }

        [Required]
        public int ReposicionId { get; set; }

        [Required]
        public int LibroId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadReposicion { get; set; }

                //Relaciones       
        public Libro Libro { get; set; }
        public Reposicion Reposicion { get; set; }
    }
}