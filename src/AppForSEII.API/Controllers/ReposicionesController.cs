using System.Net;
using AppForSEII.API.DTOs.ReposicionDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReposicionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReposicionesController> _logger;

        public ReposicionesController(ApplicationDbContext context, ILogger<ReposicionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForReposiciones(int id)
        {
            // MetodoPago es una jerarquía (Visa, GooglePay, PayPal): el tipo se deduce de la clase concreta
            var datos = await _context.Reposiciones
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.FechaReposicion,
                    r.Comentario,
                    UserName = r.Usuario!.UserName,
                    Nombre = r.Usuario!.Name,
                    Apellidos = r.Usuario!.Surname,
                    Telefono = r.Usuario!.PhoneNumber,
                    EsVisa = r.MetodoPago is Visa,
                    EsGooglePay = r.MetodoPago is GooglePay,
                    Items = r.ReposicionItems!
                        .Select(ri => new ReposicionItemDTO(
                            ri.LibroId,
                            ri.Libro.Titulo,
                            ri.Libro.Autor,
                            ri.Libro.PrecioReposicion,
                            ri.CantidadReposicion))
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (datos == null)
            {
                _logger.LogError($"Error: Reposicion with id {id} does not exist");
                return NotFound();
            }

            var tipo = datos.EsVisa
                ? MetodoPagoTipos.Visa
                : (datos.EsGooglePay ? MetodoPagoTipos.GooglePay : MetodoPagoTipos.PayPal);

            // ApplicationUser guarda los apellidos juntos (Surname): el primero hasta el primer espacio, el resto es el segundo
            var apellidos = (datos.Apellidos ?? "").Trim().Split(' ', 2);
            var apellido1 = apellidos[0];
            var apellido2 = apellidos.Length > 1 ? apellidos[1] : "";

            var reposicion = new ReposicionDetailDTO(
                datos.Id,
                datos.FechaReposicion,
                datos.UserName ?? "",
                datos.Nombre ?? "",
                apellido1,
                apellido2,
                datos.Telefono ?? "",
                tipo,
                datos.Comentario,
                datos.Items);

            return Ok(reposicion);
        }

 [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateReposicion(ReposicionForCreateDTO reposicionForCreate)
        {
            // Las validaciones definidas en ReposicionForCreateDTO (datos obligatorios, comentario
            // entre 20 y 100 caracteres, cantidad mayor que 1...) se comprueban antes de ejecutar el método

            // Flujo alternativo 0: si todos los libros tienen stock > 0, no hay nada que reponer
            if (!_context.Libros.Any(l => l.Stock == 0))
                ModelState.AddModelError("Libros", "No hay libros disponibles para reponer");

            // Flujo alternativo 2 (respaldo en servidor): no se ha seleccionado ningún libro
            if (reposicionForCreate.ReposicionItems.Count == 0)
                ModelState.AddModelError("ReposicionItems", "Error! Debes incluir al menos un libro para reponer");

            // Flujo alternativo 4 (respaldo en servidor): falta algún dato obligatorio
            if (string.IsNullOrWhiteSpace(reposicionForCreate.Nombre) ||
                string.IsNullOrWhiteSpace(reposicionForCreate.Apellido1) ||
                string.IsNullOrWhiteSpace(reposicionForCreate.Telefono))
                ModelState.AddModelError("DatosObligatorios", "Error! Faltan datos obligatorios por rellenar");

            var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == reposicionForCreate.UserName);
            if (user == null)
                ModelState.AddModelError("ReposicionApplicationUser", "Error! El nombre de usuario no está registrado");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var libroIds = reposicionForCreate.ReposicionItems.Select(ri => ri.LibroId).ToList<int>();

            var libros = _context.Libros
                .Where(l => libroIds.Contains(l.Id))
                .ToList();

            // el método de pago concreto depende del tipo elegido (Visa, GooglePay o PayPal)
            MetodoPago metodoPago = reposicionForCreate.MetodoPago switch
            {
                MetodoPagoTipos.Visa => new Visa(),
                MetodoPagoTipos.GooglePay => new GooglePay(),
                _ => new PayPal()
            };

            var reposicion = new Reposicion
            {
                FechaReposicion = DateTime.Now,
                Comentario = reposicionForCreate.Comentario,
                Usuario = user,
                MetodoPago = metodoPago,
                ReposicionItems = new List<ReposicionItem>()
            };

            foreach (var item in reposicionForCreate.ReposicionItems)
            {
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError("ReposicionItems", $"Error! El libro con id {item.LibroId} no existe");
                }
                else
                {
                    // la reposición todavía no existe en la base de datos y no tiene un Id válido,
                    // así que relacionamos el item con el objeto reposicion
                    reposicion.ReposicionItems!.Add(new ReposicionItem
                    {
                        LibroId = libro.Id,
                        Reposicion = reposicion,
                        CantidadReposicion = item.CantidadReposicion
                    });

                    // completamos el item con los datos reales del libro
                    item.Titulo = libro.Titulo;
                    item.Autor = libro.Autor;
                    item.PrecioReposicion = libro.PrecioReposicion;
                }
            }

            // si algún libro no existe
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Flujo alternativo 3: el total se calcula con los items finales del carrito
            // (así siempre coincide con los libros que quedan tras eliminar alguno)
            reposicion.PrecioTotal = reposicionForCreate.ReposicionItems
                .Sum(i => i.PrecioReposicion * i.CantidadReposicion);

            // datos que el usuario introduce en la reposición
            user!.Name = reposicionForCreate.Nombre;
            user.Surname = $"{reposicionForCreate.Apellido1} {reposicionForCreate.Apellido2}";
            user.PhoneNumber = reposicionForCreate.Telefono;

            // al reponer, el stock de cada libro aumenta con la cantidad repuesta
            foreach (var item in reposicionForCreate.ReposicionItems)
            {
                var libro = libros.First(l => l.Id == item.LibroId);
                libro.Stock += item.CantidadReposicion;
            }

            _context.Add(reposicion);

            try
            {
                // guardamos en la base de datos la reposición y sus items
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("Reposicion", "Error! Ha ocurrido un error al guardar la reposición, inténtalo de nuevo más tarde");
                return Conflict("Error" + ex.Message);
            }

            // devolvemos el detalle de la reposición (el recibo)
            var reposicionDetail = new ReposicionDetailDTO(
                reposicion.Id,
                reposicion.FechaReposicion,
                reposicionForCreate.UserName,
                reposicionForCreate.Nombre,
                reposicionForCreate.Apellido1,
                reposicionForCreate.Apellido2,
                reposicionForCreate.Telefono,
                reposicionForCreate.MetodoPago,
                reposicionForCreate.Comentario,
                reposicionForCreate.ReposicionItems);

            return CreatedAtAction("GetDetailsForReposiciones", new { id = reposicion.Id }, reposicionDetail);
        }
    }
}