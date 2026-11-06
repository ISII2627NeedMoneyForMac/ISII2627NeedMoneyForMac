using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.DTOs.CompraDTOs;
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    // Indica la ruta base del controlador: api/Compras.
    [Route("api/[controller]")]
    // Habilita las funcionalidades propias de una API REST y la validación automática del modelo.
    [ApiController]
    public class ComprasController : ControllerBase
    {
        // Contexto que permite acceder a la base de datos mediante Entity Framework.
        private readonly ApplicationDbContext _context;

        // Logger utilizado para registrar errores e información del controlador.
        private readonly ILogger<ComprasController> _logger;

        // Constructor: recibe las dependencias necesarias para trabajar.
        // El contexto permite consultar y modificar la base de datos.
        // El logger permite registrar errores.
        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Devuelve el detalle de una compra: datos del cliente, fecha, precio total,
        /// método de pago, código de descuento y libros comprados.
        /// </summary>
        // GET: obtiene los detalles de una compra.
        [HttpGet]
        // La ruta final incluye el nombre de la acción: api/Compras/GetDetailsForCompras.
        [Route("[action]")]
        // Indica que puede devolver un DTO de detalles con HTTP 200.
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.OK)]
        // Indica que puede devolver HTTP 404 si la compra no existe.
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            // Buscamos la compra cuyo identificador coincida con el recibido.
            // Where filtra las compras por su ID.
            // Select transforma la entidad Compra en un ComprasDetailDTO.
            // FirstOrDefaultAsync ejecuta la consulta de forma asíncrona y devuelve
            // el primer resultado o null si no encuentra ninguna compra.
            ComprasDetailDTO? compra = await _context.Compras
                .Where(c => c.Id == id)
                .Select(c => new ComprasDetailDTO(
                    // Identificador de la compra.
                    c.Id,

                    // Datos personales del usuario que realizó la compra.
                    c.Usuario.Name,
                    c.Usuario.Surname,
                    c.Usuario.Direccion,
                    c.Usuario.PhoneNumber,

                    // Fecha y precio total de la compra.
                    c.FechaCompra,
                    c.PrecioTotal,

                    // Identificamos el tipo de método de pago mediante el tipo de objeto.
                    // Si es Visa devuelve "Visa".
                    // Si no es Visa pero es GooglePay devuelve "GooglePay".
                    // En cualquier otro caso devuelve "PayPal".
                    c.MetodoPago is Visa ? "Visa" : c.MetodoPago is GooglePay ? "GooglePay" : "PayPal",

                    // Código de descuento utilizado, si existe.
                    c.CodigoDescuento,

                    // Obtenemos los elementos que forman parte de la compra.
                    // Para cada CompraItem creamos un CompraItemDTO con los datos del libro.
                    c.CompraItems!
                        .Select(ci => new CompraItemDTO(
                            ci.LibroId,
                            ci.Libro.Titulo,
                            ci.Libro.Genero.Nombre,
                            ci.Libro.Editorial.Nombre,
                            ci.Libro.PrecioCompra,
                            ci.Cantidad))
                        // Convertimos los elementos en una lista.
                        .ToList()))
                // Ejecutamos la consulta y obtenemos el primer resultado o null.
                .FirstOrDefaultAsync();

            // Si no se ha encontrado la compra, el resultado será null.
            if (compra == null)
            {
                // Registramos el error indicando la fecha y el ID que no se ha encontrado.
                _logger.LogError($"{DateTime.Now} Error: la compra con id {id} no existe");

                // Devolvemos HTTP 404 Not Found.
                return NotFound();
            }

            // Si existe la compra, devolvemos HTTP 200 OK junto con su DTO.
            return Ok(compra);
        }

         /// <summary>
        /// Crea una compra con los libros seleccionados y descuenta su stock.
        /// </summary>
        // POST: recibe los datos necesarios para crear una compra.
        [HttpPost]
        // La ruta final incluye el nombre de la acción: api/Compras/CreateCompra.
        [Route("[action]")]
        // Posibles respuestas del método:
        // 201: compra creada correctamente.
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.Created)]
        // 400: los datos de la compra no son válidos.
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        // 409: se ha producido un conflicto al guardar los datos.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate)
        {
            // las validaciones de CompraForCreateDTO ya se han comprobado antes de entrar al método
            // Comprobamos que se haya seleccionado al menos un libro.
            if (compraForCreate.Items.Count == 0)
                // Si no hay libros, añadimos un error al ModelState.
                ModelState.AddModelError("Items", "Error! Debe seleccionar al menos un libro para comprar");

            // Obtenemos una lista con los identificadores de los libros solicitados.
            var libroIds = compraForCreate.Items.Select(i => i.LibroId).ToList();

            // Distinct elimina los identificadores repetidos.
            // Si el número de IDs distintos es diferente al número original,
            // significa que hay algún libro repetido.
            if (libroIds.Distinct().Count() != libroIds.Count)
                // Añadimos el error correspondiente al ModelState.
                ModelState.AddModelError("Items", "Error! Hay libros repetidos en la compra");

            // Buscamos al usuario que realiza la compra utilizando su nombre de usuario.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == compraForCreate.UsuarioUserName);

            // Si no encontramos al usuario, añadimos un error.
            if (user == null)
                ModelState.AddModelError("Usuario", "Error! El usuario no está registrado");

            // Buscamos el método de pago por su identificador.
            var metodoPago = await _context.MetodoPagos.FirstOrDefaultAsync(m => m.Id == compraForCreate.MetodoPagoId);

            // Si no existe ese método de pago, añadimos un error.
            if (metodoPago == null)
                ModelState.AddModelError("MetodoPagoId", "Error! El método de pago no existe");

            // Comprobamos si alguna de las validaciones anteriores ha producido errores.
            if (ModelState.ErrorCount > 0)
                // Si hay errores, devolvemos HTTP 400 con los detalles de validación.
                // No continuamos con la creación de la compra.
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Recuperamos de la base de datos los libros cuyos IDs están en libroIds.
            var libros = await _context.Libros
                // Include carga también el género de cada libro.
                .Include(l => l.Genero)
                // Include carga también la editorial de cada libro.
                .Include(l => l.Editorial)
                // Where selecciona únicamente los libros solicitados.
                .Where(l => libroIds.Contains(l.Id))
                // Ejecutamos la consulta de forma asíncrona y obtenemos una lista.
                .ToListAsync();

            // los datos del formulario (paso 5) se guardan en el usuario
            // Actualizamos los datos personales del usuario con los recibidos en el formulario.
            user!.Name = compraForCreate.Nombre;
            user.Surname = compraForCreate.Apellidos;
            user.Direccion = compraForCreate.Direccion;
            user.PhoneNumber = compraForCreate.Telefono;

            // Creamos una nueva compra.
            // DateTime.Now establece la fecha actual.
            // El precio empieza en 0 y se calculará más adelante.
            // También se indican el usuario, el método de pago y el descuento.
            Compra compra = new Compra(DateTime.Now, 0, user.Id, metodoPago!.Id, compraForCreate.CodigoDescuento);

            // Asociamos el usuario a la compra.
            compra.Usuario = user;

            // Asociamos el método de pago a la compra.
            compra.MetodoPago = metodoPago;

            // Inicializamos la lista de elementos de la compra.
            compra.CompraItems = new List<CompraItem>();

            // Recorremos todos los elementos que el usuario quiere comprar.
            foreach (var item in compraForCreate.Items)
            {
                // Buscamos en la lista de libros recuperados el libro correspondiente al ID del elemento.
                // FirstOrDefault devuelve el libro encontrado o null si no aparece.
                Libro? libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                // Si el libro no aparece en la lista, significa que no existe.
                if (libro == null)
                {
                    // Añadimos un error indicando el ID del libro inexistente.
                    ModelState.AddModelError("Items", $"Error! El libro con id {item.LibroId} no existe");
                }
                // Si existe, comprobamos que tenga stock suficiente para cubrir la cantidad solicitada.
                else if (libro.Stock < item.Cantidad)
                {
                    // Si no hay suficientes unidades, añadimos un error.
                    // También indicamos el título y el stock disponible.
                    ModelState.AddModelError("Items", $"Error! No hay stock suficiente de '{libro.Titulo}' (disponible: {libro.Stock})");
                }
                else
                {
                    // Si el libro existe y tiene stock suficiente:
                    // Creamos un CompraItem y lo asociamos al libro y a la compra.
                    compra.CompraItems.Add(new CompraItem { Libro = libro, Compra = compra, Cantidad = item.Cantidad });

                    // Descontamos del stock la cantidad que se está comprando.
                    libro.Stock -= item.Cantidad;
                }
            }

            // Comprobamos si durante el recorrido de los libros se ha producido algún error.
            if (ModelState.ErrorCount > 0)
                // Si hay errores, devolvemos HTTP 400 con los detalles.
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Calculamos el precio total sumando:
            // precio de cada libro multiplicado por la cantidad comprada.
            // Sum calcula el resultado para todos los elementos de CompraItems.
            compra.PrecioTotal = compra.CompraItems.Sum(ci => ci.Libro.PrecioCompra * ci.Cantidad);

            // Añadimos la compra al contexto para que Entity Framework la guarde.
            _context.Add(compra);

            // Intentamos guardar los cambios en la base de datos.
            try
            {
                await _context.SaveChangesAsync();
            }
            // Si se produce una excepción durante el guardado, entramos aquí.
            catch (Exception ex)
            {
                // Registramos el mensaje de la excepción.
                _logger.LogError(ex.Message);

                // Devolvemos HTTP 409 Conflict con el mensaje del error.
                return Conflict("Error" + ex.Message);
            }

            // Construimos el DTO con los detalles de la compra recién creada.
            // Este DTO será la información que se devolverá al cliente.
            var compraDetail = new ComprasDetailDTO(
                // Datos principales de la compra y del usuario.
                compra.Id, user.Name, user.Surname, user.Direccion, user.PhoneNumber,

                // Fecha y precio total calculado.
                compra.FechaCompra, compra.PrecioTotal,

                // Identificamos el método de pago utilizado.
                // Si es Visa devuelve "Visa".
                // Si no es Visa pero es GooglePay devuelve "GooglePay".
                // En cualquier otro caso devuelve "PayPal".
                metodoPago is Visa ? "Visa" : metodoPago is GooglePay ? "GooglePay" : "PayPal",

                // Código de descuento utilizado.
                compra.CodigoDescuento,

                // Convertimos cada CompraItem en un CompraItemDTO con los datos del libro.
                compra.CompraItems.Select(ci => new CompraItemDTO(
                    ci.LibroId, ci.Libro.Titulo, ci.Libro.Genero.Nombre,
                    ci.Libro.Editorial.Nombre, ci.Libro.PrecioCompra, ci.Cantidad)).ToList());

            // Devolvemos HTTP 201 Created.
            // CreatedAtAction indica que los detalles de la compra pueden consultarse
            // mediante la acción GetDetailsForCompras, utilizando el ID recién generado.
            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }
    }
}