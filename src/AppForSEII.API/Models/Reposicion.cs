public class Reposicion
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaReposicion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioTotal { get; set; }

        [StringLength(500)]
        public string? Comentario { get; set; }
    }