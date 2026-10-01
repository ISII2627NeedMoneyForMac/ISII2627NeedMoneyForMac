using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class MetodoPago
    {
        public MetodoPago()
        {
        }

        public MetodoPago(int id)
        {
            Id = id;
        }

        [Key]
        public int Id { get; set; }

        public List<Compra> Compras { get; set; }
        public List<Reposicion> Reposiciones { get; set; }
        public List<Subasta> Subastas { get; set; }

    }
}