public class Reposicion
    {
    public Reposicion()
    {
    }

    public Reposicion(int id, DateTime fechaReposicion, decimal precioTotal, string? comentario)
    {
        Id = id;
        FechaReposicion = fechaReposicion;
        PrecioTotal = precioTotal;
        Comentario = comentario;
    }

    [Key]
        public int Id { get; set; }

        public DateTime FechaReposicion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioTotal { get; set; }

        [StringLength(500)]
        public string? Comentario { get; set; }
        public ApplicationUser Usuario { get; set; }

        public List<MetodoPago> MetodosPago { get; set; } 

        public ICollection<ReposicionItem>? ReposicionItems { get; set; }
    }