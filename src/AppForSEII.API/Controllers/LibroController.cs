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


               /// <summary>
        /// Devuelve los libros disponibles para comprar (stock > 0), opcionalmente
        /// filtrados por género y rango de precio, ordenados por título.
        /// </summary>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaComprar(string? generoLibro, decimal? precioMin, decimal? precioMax)
        {
            IList<LibroParaComprarDTO> libros = await _context.Libros
                .Where(l => l.Stock > 0
                    && (generoLibro == null || l.Genero.Nombre.Equals(generoLibro))
                    && (precioMin == null || l.PrecioCompra >= precioMin)
                    && (precioMax == null || l.PrecioCompra <= precioMax))
                .OrderBy(l => l.Titulo)
                .Select(l => new LibroParaComprarDTO(
                    l.Id,
                    l.Titulo,
                    l.PrecioCompra,
                    l.FechaLanzamiento,
                    l.Genero.Nombre,
                    l.Editorial.Nombre))
                .ToListAsync();

            return Ok(libros);
        }

        /// <summary>
        /// Devuelve los libros que están en subasta, con el precio de puja
        /// y la descripción de cada ítem, ordenados por título.
        /// </summary>
        [HttpGet]
[Route("[action]")]
[ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]
public async Task<ActionResult<IList<LibroParaSubastarDTO>>> GetLibrosParaSubastar(
    [FromQuery] string? titulo,
    [FromQuery] string? editorial)
{
    var query = _context.Libros.AsQueryable();

    // Flujo Alternativo 1: Filtrar por Título y Editorial
    if (!string.IsNullOrWhiteSpace(titulo))
    {
        query = query.Where(l => l.Titulo.Contains(titulo));
    }

    if (!string.IsNullOrWhiteSpace(editorial))
    {
        query = query.Where(l => l.Editorial.Nombre.Contains(editorial));
    }

    IList<LibroParaSubastarDTO> libros = await query
        .OrderBy(l => l.Titulo)
        .Select(l => new LibroParaSubastarDTO(
            l.Id,
            l.Titulo,
            l.Autor,
            l.Editorial.Nombre,
            l.TipoLibro))
        .ToListAsync();

    return Ok(libros);
}
    }

}