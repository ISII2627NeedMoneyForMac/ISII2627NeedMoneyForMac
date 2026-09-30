using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Visa
    {
        [Required]
        [CreditCard]
        public string NumeroTarjeta { get; set; } = string.Empty;
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
         [System.ComponentModel.DataAnnotations.Display(Name = "FechaCaducidad")]
        public DateTime FechaCaducidad { get; set; }
    }
}