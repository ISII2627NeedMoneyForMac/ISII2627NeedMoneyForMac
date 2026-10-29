using System.Net;
using AppForSEII.API.DTOs.SubastaDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubastasController> _logger;

        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetSubasta(int id)
        {
            var subasta = await _context.Subastas
                .AsNoTracking()
                .Include(s => s.Usuario)
                .Include(s => s.MetodoPago)
                .Include(s => s.SubastaItems)
                    .ThenInclude(si => si.Libro)
                        .ThenInclude(l => l.Editorial)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subasta == null)
            {
                _logger.LogError($"Error: Subasta with id {id} does not exist");
                return NotFound();
            }

            var items = subasta.SubastaItems
                .Select(si => new SubastaItemDTO(si.Libro.Id, si.Libro.Titulo,
                    si.Libro.Editorial.Nombre, si.Libro.Autor, si.PrecioPuja))
                .ToList();

            var detail = new SubastaDetailDTO(subasta.Id,
                subasta.Usuario?.Name ?? string.Empty,
                subasta.Usuario?.Surname ?? string.Empty,
                subasta.Usuario?.PhoneNumber ?? string.Empty,
                ObtenerTipoMetodoPago(subasta.MetodoPago),
                subasta.FechaSubasta,
                subasta.SubastaItems.Select(si => si.Descripcion).FirstOrDefault(),
                items);

            return Ok(detail);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            // Los DataAnnotations (campos obligatorios, descripción 20-100, puja > 0) ya se validan antes de entrar

            // Flujo alternativo 4: debe haber al menos un libro en la lista de pujas
            if (subastaForCreate.SubastaItems.Count == 0)
                ModelState.AddModelError("SubastaItems", "Error! Debes incluir al menos un libro en la subasta");

            if (subastaForCreate.SubastaItems.Select(si => si.LibroId).Distinct().Count()
                != subastaForCreate.SubastaItems.Count)
                ModelState.AddModelError("SubastaItems", "Error! Hay libros repetidos en la subasta");

            // PrecioPuja es int en la entidad: no se admiten decimales
            foreach (var item in subastaForCreate.SubastaItems)
            {
                if (item.PrecioPuja != Math.Floor(item.PrecioPuja))
                    ModelState.AddModelError("SubastaItems",
                        $"Error! El precio de la puja del libro '{item.LibroId}' debe ser un número entero");
            }

            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(au => au.UserName == subastaForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("SubastaApplicationUser", "Error! UserName is not registered");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var libroIds = subastaForCreate.SubastaItems.Select(si => si.LibroId).ToList();
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();

            var subasta = new Subasta
            {
                FechaSubasta = DateTime.Now,
                Usuario = user,
                MetodoPago = CrearMetodoPago(subastaForCreate.MetodoPago)
            };

            var itemsDetail = new List<SubastaItemDTO>();

            foreach (var item in subastaForCreate.SubastaItems)
            {
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError("SubastaItems", $"Error! El libro con id '{item.LibroId}' no existe");
                }
                else if (libro.Stock <= 0)
                {
                    // Flujo alternativo 0: solo se subastan libros con stock > 0
                    ModelState.AddModelError("SubastaItems", $"Error! El libro '{libro.Titulo}' no está disponible para subasta");
                }
                else
                {
                    subasta.SubastaItems.Add(new SubastaItem
                    {
                        Libro = libro,
                        Subasta = subasta,
                        PrecioPuja = (int)item.PrecioPuja,
                        Descripcion = subastaForCreate.Descripcion
                    });

                    // Se usan los datos de la BD, no los enviados por el cliente
                    itemsDetail.Add(new SubastaItemDTO(libro.Id, libro.Titulo,
                        libro.Editorial.Nombre, libro.Autor, (int)item.PrecioPuja));
                }
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);

            _context.Add(subasta);

            try
            {
                // Se guardan la subasta y sus items
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error! There was an error while saving your subasta, please try again later: "
                    + ex.Message);
            }

            var subastaDetail = new SubastaDetailDTO(subasta.Id,
                user!.Name ?? string.Empty,
                user.Surname ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                subastaForCreate.MetodoPago, subasta.FechaSubasta,
                subastaForCreate.Descripcion, itemsDetail);

            return CreatedAtAction("GetSubasta", new { id = subasta.Id }, subastaDetail);
        }

        // enum del DTO -> clase de la entidad
        private static MetodoPago CrearMetodoPago(MetodoPagoSubastaTipos tipo) => tipo switch
        {
            MetodoPagoSubastaTipos.Visa => new Visa(),
            MetodoPagoSubastaTipos.GooglePay => new GooglePay(),
            MetodoPagoSubastaTipos.PayPal => new PayPal(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        // clase de la entidad -> enum del DTO
        private static MetodoPagoSubastaTipos ObtenerTipoMetodoPago(MetodoPago? metodoPago) => metodoPago switch
        {
            GooglePay => MetodoPagoSubastaTipos.GooglePay,
            PayPal => MetodoPagoSubastaTipos.PayPal,
            _ => MetodoPagoSubastaTipos.Visa
        };
    }
}