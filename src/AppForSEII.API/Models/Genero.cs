   using System.ComponentModel.DataAnnotations;

   namespace AppForSEII.API.Models
   {
       public class Genero
       {
           [Key]
           public int Id { get; set; }

           [Required]
           [StringLength(50)]
           public string Nombre { get; set; } = string.Empty;
       }
   }