using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class GooglePay: MetodoPago
    {
        public GooglePay()
        {
        }

        public GooglePay(string email)
        {
            Email = email;
        }

        [Key]
        public string Email { get; set; } = string.Empty;
    }
}