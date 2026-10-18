using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.DTOs.LibroDTOs;

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
        /// Devuelve los ítems de reposición, con el libro al que corresponde cada uno
        /// y la cantidad que hay que reponer.
        /// </summary>
[HttpGet]
[Route("[action]")]
[ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
public async Task<ActionResult> GetLibrosParaReponer()
{
    IList<LibroParaReponerDTO> libros = await _context.ReposicionItems
        .OrderByDescending(ri => ri.CantidadReposicion)
        .Select(ri => new LibroParaReponerDTO(
            ri.LibroId,
            ri.Libro.Titulo,
            ri.Libro.Autor,
            ri.CantidadReposicion))
        .ToListAsync();

    return Ok(libros);
}

    }
}