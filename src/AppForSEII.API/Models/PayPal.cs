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
        public PayPal(string numeroTelefono)
        {
            NumeroTelefono = numeroTelefono;
        }

        // Atributos
        [Required]
        [Phone]
        public string NumeroTelefono { get; set; }
    }
}
