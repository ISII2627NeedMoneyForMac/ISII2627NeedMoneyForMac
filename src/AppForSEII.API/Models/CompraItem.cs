using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class CompraItem
    {
        //Constructores
        public CompraItem(){ }

        public CompraItem(int cantidad, int libroId, int compraId)
        {
            Cantidad = cantidad;
            LibroId = libroId;
            CompraId = compraId;
        }

        //Atributos
        [Key]
        public int Id { get; set; }

        public int LibroId { get; set; }

        public int CompraId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 1")]
        public int Cantidad { get; set; }

        //Relaciones       
        public Libro Libro { get; set; }
        public Compra Compra { get; set; }
    }
}