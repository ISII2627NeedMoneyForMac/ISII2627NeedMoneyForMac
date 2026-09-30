using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class GooglePay
    {
        [Key]
        public int Email { get; set; }
    }
}