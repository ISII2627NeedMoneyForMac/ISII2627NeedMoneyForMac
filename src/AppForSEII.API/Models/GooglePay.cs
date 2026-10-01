using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class GooglePay
    {
        public GooglePay()
        {
        }

        public GooglePay(int email)
        {
            Email = email;
        }

        [Key]
        public int Email { get; set; }
    }
}