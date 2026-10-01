using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class PayPal : MetodoPago
    {
        // Constructor por defecto
        public PayPal() { }

        // Constructor parametrizado
        public PayPal(int id,string numeroTelefono)
        {
            Id = id;
            NumeroTelefono = numeroTelefono;
        }

        // Atributos

        [Key]
        public int Id { get; set; }
        
        [Required]
        [Phone]
        public string NumeroTelefono { get; set; }
        }
}