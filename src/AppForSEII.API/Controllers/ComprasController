using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.DTOs.CompraDTOs;
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Devuelve el detalle de una compra: datos del cliente, fecha, precio total,
        /// método de pago, código de descuento y libros comprados.
        /// </summary>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            ComprasDetailDTO? compra = await _context.Compras
                .Where(c => c.Id == id)
                .Select(c => new ComprasDetailDTO(
                    c.Id,
                    c.Usuario.Name,
                    c.Usuario.Surname,
                    c.Usuario.Direccion,
                    c.Usuario.PhoneNumber,
                    c.FechaCompra,
                    c.PrecioTotal,
                    c.MetodoPago is Visa ? "Visa" : c.MetodoPago is GooglePay ? "GooglePay" : "PayPal",
                    c.CodigoDescuento,
                    c.CompraItems!
                        .Select(ci => new CompraItemDTO(
                            ci.LibroId,
                            ci.Libro.Titulo,
                            ci.Libro.Genero.Nombre,
                            ci.Libro.Editorial.Nombre,
                            ci.Libro.PrecioCompra,
                            ci.Cantidad))
                        .ToList()))
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                _logger.LogError($"{DateTime.Now} Error: la compra con id {id} no existe");
                return NotFound();
            }

            return Ok(compra);
        }

         /// <summary>
        /// Crea una compra con los libros seleccionados y descuenta su stock.
        /// </summary>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate)
        {
            // las validaciones de CompraForCreateDTO ya se han comprobado antes de entrar al método
            if (compraForCreate.Items.Count == 0)
                ModelState.AddModelError("Items", "Error! Debe seleccionar al menos un libro para comprar");

            var libroIds = compraForCreate.Items.Select(i => i.LibroId).ToList();
            if (libroIds.Distinct().Count() != libroIds.Count)
                ModelState.AddModelError("Items", "Error! Hay libros repetidos en la compra");

            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == compraForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("Usuario", "Error! El usuario no está registrado");

            var metodoPago = await _context.MetodoPagos.FirstOrDefaultAsync(m => m.Id == compraForCreate.MetodoPagoId);
            if (metodoPago == null)
                ModelState.AddModelError("MetodoPagoId", "Error! El método de pago no existe");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var libros = await _context.Libros
                .Include(l => l.Genero)
                .Include(l => l.Editorial)
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();

            // los datos del formulario (paso 5) se guardan en el usuario
            user!.Name = compraForCreate.Nombre;
            user.Surname = compraForCreate.Apellidos;
            user.Direccion = compraForCreate.Direccion;
            user.PhoneNumber = compraForCreate.Telefono;

            Compra compra = new Compra(DateTime.Now, 0, user.Id, metodoPago!.Id, compraForCreate.CodigoDescuento);
            compra.Usuario = user;
            compra.MetodoPago = metodoPago;
            compra.CompraItems = new List<CompraItem>();

            foreach (var item in compraForCreate.Items)
            {
                Libro? libro = libros.FirstOrDefault(l => l.Id == item.LibroId);
                if (libro == null)
                {
                    ModelState.AddModelError("Items", $"Error! El libro con id {item.LibroId} no existe");
                }
                else if (libro.Stock < item.Cantidad)
                {
                    ModelState.AddModelError("Items", $"Error! No hay stock suficiente de '{libro.Titulo}' (disponible: {libro.Stock})");
                }
                else
                {
                    compra.CompraItems.Add(new CompraItem { Libro = libro, Compra = compra, Cantidad = item.Cantidad });
                    libro.Stock -= item.Cantidad;
                }
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            compra.PrecioTotal = compra.CompraItems.Sum(ci => ci.Libro.PrecioCompra * ci.Cantidad);

            _context.Add(compra);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error" + ex.Message);
            }

            var compraDetail = new ComprasDetailDTO(
                compra.Id, user.Name, user.Surname, user.Direccion, user.PhoneNumber,
                compra.FechaCompra, compra.PrecioTotal,
                metodoPago is Visa ? "Visa" : metodoPago is GooglePay ? "GooglePay" : "PayPal",
                compra.CodigoDescuento,
                compra.CompraItems.Select(ci => new CompraItemDTO(
                    ci.LibroId, ci.Libro.Titulo, ci.Libro.Genero.Nombre,
                    ci.Libro.Editorial.Nombre, ci.Libro.PrecioCompra, ci.Cantidad)).ToList());

            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }
    }
}