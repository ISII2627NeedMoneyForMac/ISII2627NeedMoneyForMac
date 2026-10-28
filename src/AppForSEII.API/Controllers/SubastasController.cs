using System.Net;
using AppForSEII.API.Data;
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
        public async Task<ActionResult> GetDetailsForSubastas(int id, CancellationToken cancellationToken)
        {
            var datos = await _context.Subastas
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.FechaSubasta,
                    Nombre = s.Usuario!.Name,
                    Apellidos = s.Usuario!.Surname,
                    Telefono = s.Usuario!.PhoneNumber,
                    Descripcion = s.SubastaItems!
                        .Select(si => si.Descripcion)
                        .FirstOrDefault(),
                    Items = s.SubastaItems!
                        .Select(si => new SubastaItemDTO(
                            si.LibroId,
                            si.Libro.Titulo,
                            si.Libro.Autor,
                            si.PrecioPuja))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (datos == null)
            {
                _logger.LogError("Error: Subasta with id {SubastaId} does not exist", id);
                return NotFound();
            }

            var subasta = new SubastaDetailDTO(
                datos.Id,
                datos.Nombre ?? "",
                datos.Apellidos ?? "",
                datos.Telefono ?? "",
                datos.FechaSubasta,
                datos.Descripcion,
                datos.Items);

            return Ok(subasta);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateSubasta(
            SubastaForCreateDTO subastaForCreate,
            CancellationToken cancellationToken)
        {
            if (subastaForCreate.SubastaItems.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(subastaForCreate.SubastaItems),
                    "Error! Debes incluir al menos un libro para subastar");
            }

            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(au => au.UserName == subastaForCreate.UserName, cancellationToken);

            if (user == null)
            {
                ModelState.AddModelError(
                    "SubastaApplicationUser",
                    "Error! El nombre de usuario no está registrado");
            }

            if (subastaForCreate.SubastaItems
                .GroupBy(item => item.LibroId)
                .Any(group => group.Count() > 1))
            {
                ModelState.AddModelError(
                    nameof(subastaForCreate.SubastaItems),
                    "Error! No se puede incluir el mismo libro más de una vez");
            }

            if (subastaForCreate.SubastaItems.Any(item => item.PrecioPuja % 1 != 0))
            {
                ModelState.AddModelError(
                    nameof(subastaForCreate.SubastaItems),
                    "Error! El precio de la puja debe ser un número entero");
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var libroIds = subastaForCreate.SubastaItems
                .Select(item => item.LibroId)
                .ToList();

            var libros = await _context.Libros
                .Where(libro => libroIds.Contains(libro.Id))
                .ToListAsync(cancellationToken);

            var subasta = new Subasta
            {
                FechaSubasta = DateTime.Now,
                PrecioSubasta = (double)subastaForCreate.TotalPrice,
                Usuario = user!,
            MetodoPago = subastaForCreate.MetodoPago switch
            {
                     MetodoPagoTipos.Visa => new Visa(),
                     MetodoPagoTipos.GooglePay => new GooglePay(),
                    _ => new PayPal()
                    },
                SubastaItems = new List<SubastaItem>()
            };

            foreach (var item in subastaForCreate.SubastaItems)
            {
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError(
                        nameof(subastaForCreate.SubastaItems),
                        $"Error! El libro con id {item.LibroId} no existe");
                    continue;
                }

                subasta.SubastaItems.Add(new SubastaItem
                {
                    LibroId = libro.Id,
                    Subasta = subasta,
                    PrecioPuja = (int)item.PrecioPuja,
                    Descripcion = subastaForCreate.Descripcion
                });

                item.Titulo = libro.Titulo;
                item.Autor = libro.Autor;
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            user!.Name = subastaForCreate.Nombre;
            user.Surname = subastaForCreate.Apellidos;
            user.PhoneNumber = subastaForCreate.Telefono;

            _context.Subastas.Add(subasta);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving subasta for user {UserName}", subastaForCreate.UserName);
                return Conflict("Error! Ha ocurrido un error al guardar la subasta, inténtalo de nuevo más tarde");
            }

            var subastaDetail = new SubastaDetailDTO(
                subasta.Id,
                subastaForCreate.Nombre,
                subastaForCreate.Apellidos,
                subastaForCreate.Telefono,
                subasta.FechaSubasta,
                subastaForCreate.Descripcion,
                subastaForCreate.SubastaItems);

            return CreatedAtAction(
                nameof(GetDetailsForSubastas),
                new { id = subasta.Id },
                subastaDetail);
        }
    }
}