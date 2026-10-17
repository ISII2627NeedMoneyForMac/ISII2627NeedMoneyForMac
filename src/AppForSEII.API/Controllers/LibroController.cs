using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LibrosController> _logger;

        public LibrosController(ApplicationDbContext context, ILogger<LibrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca libros por título, autor y/o tipo. Todos los filtros son opcionales.
        /// Sirve de base para las pantallas de compra, subasta y reposición.
        /// </summary>

    }
}