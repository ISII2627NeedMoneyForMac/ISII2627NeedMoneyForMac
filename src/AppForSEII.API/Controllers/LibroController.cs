using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.API.Controllers
{
    // Define la ruta base del controlador: api/Libros.
    [Route("api/[controller]")]
    // Indica que esta clase es un controlador de una API.
    [ApiController]
    public class LibrosController : ControllerBase
    {
        // Contexto que permite acceder a la base de datos mediante Entity Framework.
        private readonly ApplicationDbContext _context;

        // Logger utilizado para registrar errores.
        private readonly ILogger<LibrosController> _logger;

        // Constructor: recibe las dependencias necesarias.
        // El contexto permite consultar la base de datos y el logger registrar información.
        public LibrosController(ApplicationDbContext context, ILogger<LibrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

/// <summary>
/// Devuelve los libros sin stock (Stock == 0) para reponer, opcionalmente
/// filtrados por autor y género, ordenados por título.
/// </summary>
// GET: devuelve los libros que necesitan reposición.
[HttpGet]
// La ruta incluye el nombre de la acción: api/Libros/GetLibrosParaReponer.
[Route("[action]")]
// Indica que la respuesta correcta es HTTP 200 con una lista de LibroParaReponerDTO.
[ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
public async Task<ActionResult> GetLibrosParaReponer(string? autor, string? generoLibro)
{
    // Consultamos los libros de la base de datos.
    IList<LibroParaReponerDTO> libros = await _context.Libros
        // Aplicamos tres condiciones:
        // 1. El stock debe ser exactamente 0.
        // 2. Si se ha indicado un autor, debe coincidir con el autor del libro.
        // 3. Si se ha indicado un género, debe coincidir con el género del libro.
        // string.IsNullOrEmpty permite que los filtros de autor y género sean opcionales.
        .Where(l => l.Stock == 0
            && (string.IsNullOrEmpty(autor) || l.Autor.Equals(autor))
            && (string.IsNullOrEmpty(generoLibro) || l.Genero.Nombre.Equals(generoLibro)))

        // Ordenamos los libros alfabéticamente por su título.
        .OrderBy(l => l.Titulo)

        // Transformamos cada entidad Libro en un LibroParaReponerDTO.
        // Así devolvemos únicamente los datos necesarios para la reposición.
        .Select(l => new LibroParaReponerDTO(
            l.Id,
            l.Titulo,
            l.Genero.Nombre,
            l.FechaLanzamiento,
           l.PrecioReposicion, // O la propiedad correspondiente al precio de reposición en tu entidad Libro
            l.Autor))

        // Ejecutamos la consulta de forma asíncrona y obtenemos una lista.
        .ToListAsync();

    // Devolvemos HTTP 200 OK junto con la lista de libros encontrados.
    return Ok(libros);
}


               /// <summary>
        /// Devuelve los libros disponibles para comprar (stock > 0), opcionalmente
        /// filtrados por género y rango de precio, ordenados por título.
        /// </summary>
        // GET: devuelve los libros que se pueden comprar.
        [HttpGet]
        // La ruta incluye el nombre de la acción: api/Libros/GetLibrosParaComprar.
        [Route("[action]")]
        // Indica que puede devolver HTTP 200 con una lista de libros.
        [ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
        // Indica que puede devolver HTTP 400 si los filtros no son válidos.
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetLibrosParaComprar(string? generoLibro, decimal? precioMin, decimal? precioMax)
        {
          // Comprobamos que, si se han indicado ambos precios,
          // el precio mínimo no sea superior al precio máximo.
          if (precioMin != null && precioMax != null && precioMin > precioMax)
            {
                // Añadimos un error al ModelState indicando que el rango de precios es incorrecto.
                ModelState.AddModelError("precioMin&precioMax", "precioMin debe ser menor o igual que precioMax");

                // Registramos el error en el logger.
                _logger.LogError($"{DateTime.Now} Error: precioMin debe ser menor o igual que precioMax");

                // Devolvemos HTTP 400 Bad Request con los detalles del error.
                // No continuamos ejecutando la consulta.
                return BadRequest(new ValidationProblemDetails(ModelState));
            }


            // Consultamos los libros disponibles para comprar.
            IList<LibroParaComprarDTO> libros = await _context.Libros
                // Aplicamos los filtros:
                // 1. El libro debe tener stock mayor que 0.
                // 2. Si se indica un género, debe coincidir.
                // 3. Si se indica precio mínimo, el precio debe ser mayor o igual.
                // 4. Si se indica precio máximo, el precio debe ser menor o igual.
                // Los filtros de género y precio son opcionales.
                .Where(l => l.Stock > 0
                    && (generoLibro == null || l.Genero.Nombre.Equals(generoLibro))
                    && (precioMin == null || l.PrecioCompra >= precioMin)
                    && (precioMax == null || l.PrecioCompra <= precioMax))

                // Ordenamos los libros alfabéticamente por título.
                .OrderBy(l => l.Titulo)

                // Convertimos cada entidad Libro en un LibroParaComprarDTO.
                .Select(l => new LibroParaComprarDTO(
                    l.Id,
                    l.Titulo,
                    l.PrecioCompra,
                    l.FechaLanzamiento,
                    l.Genero.Nombre,
                    l.Editorial.Nombre))

                // Ejecutamos la consulta y obtenemos una lista.
                .ToListAsync();

            // Devolvemos HTTP 200 OK con los libros que cumplen los filtros.
            return Ok(libros);
        }

        /// <summary>
        /// Devuelve los libros que están en subasta, con el precio de puja
        /// y la descripción de cada ítem, ordenados por título.
        /// </summary>
        // GET: devuelve los libros que se pueden subastar.
        [HttpGet]
        // La ruta incluye el nombre de la acción: api/Libros/GetLibrosParaSubastar.
        [Route("[action]")]
        // Indica que la respuesta correcta es HTTP 200 con una lista de LibroParaSubastarDTO.
        [ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IList<LibroParaSubastarDTO>>> GetLibrosParaSubastar(
            // [FromQuery] indica que estos parámetros se reciben desde la URL.
            // Por ejemplo: ?titulo=Dune&editorial=Planeta
            [FromQuery] string? titulo,
            [FromQuery] string? editorial)
        {
            // Creamos una consulta inicial con los libros que tienen stock disponible.
            // Todavía no se ejecuta la consulta contra la base de datos.
            var query = _context.Libros.Where(l => l.Stock > 0);

        // Flujo Alternativo 1: Filtrar por Título y Editorial
        // Si se ha indicado un título que no sea null, vacío o espacios,
        // añadimos un filtro para buscar títulos que contengan ese texto.
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            query = query.Where(l => l.Titulo.Contains(titulo));
        }

        // Si se ha indicado una editorial, añadimos otro filtro.
        // Contains permite encontrar editoriales cuyo nombre contenga el texto indicado.
        if (!string.IsNullOrWhiteSpace(editorial))
        {
            query = query.Where(l => l.Editorial.Nombre.Contains(editorial));
        }

    // Ejecutamos la consulta después de aplicar los filtros opcionales.
    IList<LibroParaSubastarDTO> libros = await query

        // Ordenamos los resultados por título.
        .OrderBy(l => l.Titulo)

        // Transformamos cada libro en un LibroParaSubastarDTO.
        // Solo incluimos los datos necesarios para mostrarlo en la subasta.
        .Select(l => new LibroParaSubastarDTO(
            l.Id,
            l.Titulo,
            l.Autor,
            l.Editorial.Nombre,
            l.TipoLibro))

        // Ejecutamos la consulta de forma asíncrona y la convertimos en lista.
        .ToListAsync();

    // Devolvemos HTTP 200 OK con los libros encontrados.
    return Ok(libros);
}
    }

}