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
                    EsVisa = r.MetodosPago!.Any(m => m is Visa),
                    EsGooglePay = r.MetodosPago!.Any(m => m is GooglePay),
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
        }
        }