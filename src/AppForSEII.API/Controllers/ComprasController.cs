// Permite trabajar con elementos relacionados con las peticiones HTTP.
using Microsoft.AspNetCore.Http;

// Contiene las herramientas necesarias para crear controladores y devolver
// respuestas HTTP como Ok(), NotFound(), BadRequest(), CreatedAtAction(), etc.
using Microsoft.AspNetCore.Mvc;

// Permite utilizar Entity Framework Core para consultar y modificar
// la base de datos de forma asíncrona.
using Microsoft.EntityFrameworkCore;

// Permite utilizar códigos de estado HTTP, como OK, NotFound o Conflict.
using System.Net;

// Importa los DTO relacionados con las compras.
// Los DTO sirven para transportar datos entre el cliente y la API.
using AppForSEII.API.DTOs.CompraDTOs;

// Importa las clases del modelo: Compra, Libro, Usuario, métodos de pago, etc.
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    // Indica que las rutas de este controlador empiezan por:
    // api/Compras
    // [controller] se sustituye automáticamente por el nombre de la clase
    // sin la palabra Controller.
    [Route("api/[controller]")]

    // Indica que es un controlador de API.
    // Activa comportamientos automáticos como la validación del modelo
    // y la respuesta BadRequest cuando falla la validación automática.
    [ApiController]
    public class ComprasController : ControllerBase
    {
        // Referencia al contexto de Entity Framework.
        // Permite acceder a las tablas de la base de datos.
        // readonly significa que solo se asigna en el constructor.
        private readonly ApplicationDbContext _context;

        // Logger utilizado para registrar errores e información.
        private readonly ILogger<ComprasController> _logger;

        // Constructor del controlador.
        // Recibe las dependencias que necesita mediante inyección de dependencias.
        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            // Guarda el contexto recibido para utilizarlo en los métodos.
            _context = context;

            // Guarda el logger recibido para registrar errores.
            _logger = logger;
        }

        /// <summary>
        /// Devuelve el detalle de una compra: datos del cliente, fecha, precio total,
        /// método de pago, código de descuento y libros comprados.
        /// </summary>
        // GET: api/Compras/GetDetailsForCompras?id=1
        [HttpGet]

        // Indica que la ruta se construye utilizando el nombre del método.
        [Route("[action]")]

        // Indica que si la operación tiene éxito (200 OK),
        // el resultado devuelto tendrá la estructura de ComprasDetailDTO.
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.OK)]

        // Indica que también puede devolver un 404 NotFound
        // si la compra solicitada no existe.
        [ProducesResponseType((int)HttpStatusCode.NotFound)]

        // Método asíncrono que recibe el identificador de una compra.
        // Devuelve un ActionResult, que permite devolver tanto datos
        // como diferentes respuestas HTTP.
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            // Busca una compra cuyo Id coincida con el parámetro recibido.
            // Select permite transformar la entidad Compra en un DTO.
            // FirstOrDefaultAsync ejecuta la consulta de forma asíncrona.
            // Si no encuentra ninguna compra, devuelve null.
            ComprasDetailDTO? compra = await _context.Compras

                // Filtra las compras para quedarse únicamente con la que
                // tiene el identificador indicado.
                .Where(c => c.Id == id)

                // Proyecta la entidad Compra a un objeto ComprasDetailDTO.
                // Es decir, selecciona los datos que queremos devolver al cliente.
                .Select(c => new ComprasDetailDTO(

                    // Identificador de la compra.
                    c.Id,

                    // Nombre del usuario asociado a la compra.
                    c.Usuario.Name,

                    // Apellidos del usuario.
                    c.Usuario.Surname,

                    // Dirección del usuario.
                    c.Usuario.Direccion,

                    // Teléfono del usuario.
                    c.Usuario.PhoneNumber,

                    // Fecha en la que se realizó la compra.
                    c.FechaCompra,

                    // Precio total de la compra.
                    c.PrecioTotal,

                    // Comprueba qué tipo de método de pago tiene la compra.
                    // Si es Visa devuelve "Visa".
                    // Si no, comprueba si es GooglePay.
                    // Si tampoco lo es, devuelve "PayPal".
                    c.MetodoPago is Visa ? "Visa" : c.MetodoPago is GooglePay ? "GooglePay" : "PayPal",

                    // Código de descuento utilizado en la compra.
                    c.CodigoDescuento,

                    // Obtiene todos los elementos que forman parte de la compra.
                    // Para cada CompraItem crea un CompraItemDTO.
                    c.CompraItems!

                        // Transforma cada elemento de la compra en un DTO.
                        .Select(ci => new CompraItemDTO(

                            // Identificador del libro.
                            ci.LibroId,

                            // Título del libro.
                            ci.Libro.Titulo,

                            // Nombre del género al que pertenece el libro.
                            ci.Libro.Genero.Nombre,

                            // Nombre de la editorial del libro.
                            ci.Libro.Editorial.Nombre,

                            // Precio de compra de una unidad del libro.
                            ci.Libro.PrecioCompra,

                            // Cantidad de unidades compradas de ese libro.
                            ci.Cantidad))

                        // Convierte los elementos seleccionados en una lista.
                        .ToList()))

                // Ejecuta la consulta y devuelve el primer resultado.
                // Si no existe, devuelve null.
                .FirstOrDefaultAsync();

            // Comprueba si la compra no se ha encontrado.
            if (compra == null)
            {
                // Registra un mensaje de error indicando la fecha y el Id
                // de la compra que no existe.
                _logger.LogError($"{DateTime.Now} Error: la compra con id {id} no existe");

                // Devuelve HTTP 404: Not Found.
                return NotFound();
            }

            // Si la compra existe, devuelve HTTP 200 OK
            // junto con los detalles de la compra.
            return Ok(compra);
        }

         /// <summary>
        /// Crea una compra con los libros seleccionados y descuenta su stock.
        /// </summary>
        // POST: api/Compras/CreateCompra
        [HttpPost]

        // La ruta se construye utilizando el nombre del método.
        [Route("[action]")]

        // Respuesta posible si la compra se crea correctamente: HTTP 201.
        // El cuerpo de la respuesta tendrá un ComprasDetailDTO.
        [ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.Created)]

        // Respuesta posible si hay errores de validación: HTTP 400.
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]

        // Respuesta posible si ocurre un conflicto al guardar: HTTP 409.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]

        // Método asíncrono que recibe un DTO con toda la información necesaria
        // para crear la compra.
        public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate)
        {
            // Las validaciones de CompraForCreateDTO ya se han comprobado
            // antes de entrar al método.
            // Aquí se realizan comprobaciones adicionales.

            // Comprueba que el carrito contiene al menos un libro.
            if (compraForCreate.Items.Count == 0)

                // Añade un error al ModelState asociado a Items.
                // No se puede realizar una compra sin libros.
                ModelState.AddModelError("Items", "Error! Debe seleccionar al menos un libro para comprar");

            // Obtiene los identificadores de todos los libros seleccionados.
            // Select extrae el LibroId de cada elemento del carrito.
            // ToList convierte los resultados en una lista.
            var libroIds = compraForCreate.Items.Select(i => i.LibroId).ToList();

            // Distinct elimina los identificadores repetidos.
            // Si el número de identificadores distintos es diferente
            // del número total de identificadores, existen libros repetidos.
            if (libroIds.Distinct().Count() != libroIds.Count)

                // Añade un error indicando que hay libros repetidos.
                ModelState.AddModelError("Items", "Error! Hay libros repetidos en la compra");

            // Busca al usuario en la base de datos.
            // FirstOrDefaultAsync devuelve el primer usuario que cumple
            // la condición o null si no existe.
            // Se utiliza el nombre de usuario recibido en el DTO.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == compraForCreate.UsuarioUserName);

            // Comprueba que el usuario existe.
            if (user == null)

                // Si no existe, registra un error en ModelState.
                ModelState.AddModelError("Usuario", "Error! El usuario no está registrado");

            // Busca el método de pago en la base de datos utilizando su Id.
            var metodoPago = await _context.MetodoPagos.FirstOrDefaultAsync(m => m.Id == compraForCreate.MetodoPagoId);

            // Comprueba que el método de pago existe.
            if (metodoPago == null)

                // Si no existe, añade un error al ModelState.
                ModelState.AddModelError("MetodoPagoId", "Error! El método de pago no existe");

            // Comprueba si se ha producido algún error durante las validaciones.
            if (ModelState.ErrorCount > 0)

                // Si existe al menos un error, devuelve HTTP 400 BadRequest.
                // ValidationProblemDetails contiene información sobre los errores.
                // Se detiene aquí la ejecución del método.
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Busca en la base de datos los libros cuyos identificadores
            // aparecen en la lista libroIds.
            var libros = await _context.Libros

                // Include carga también el género de cada libro.
                // Permite acceder posteriormente a libro.Genero.
                .Include(l => l.Genero)

                // Include carga también la editorial de cada libro.
                // Permite acceder posteriormente a libro.Editorial.
                .Include(l => l.Editorial)

                // Filtra los libros para obtener únicamente aquellos
                // cuyo Id está incluido en libroIds.
                .Where(l => libroIds.Contains(l.Id))

                // Ejecuta la consulta de manera asíncrona
                // y devuelve los resultados en una lista.
                .ToListAsync();

            // Los datos del formulario (paso 5) se guardan en el usuario.

            // Actualiza el nombre del usuario con el introducido en el formulario.
            user!.Name = compraForCreate.Nombre;

            // Actualiza sus apellidos.
            user.Surname = compraForCreate.Apellidos;

            // Actualiza su dirección.
            user.Direccion = compraForCreate.Direccion;

            // Actualiza su teléfono.
            user.PhoneNumber = compraForCreate.Telefono;

            // Crea un nuevo objeto Compra.
            // DateTime.Now: fecha y hora actual.
            // 0: precio total inicial, que se calculará posteriormente.
            // user.Id: identificador del usuario que realiza la compra.
            // metodoPago!.Id: identificador del método de pago.
            // compraForCreate.CodigoDescuento: código de descuento introducido.
            Compra compra = new Compra(DateTime.Now, 0, user.Id, metodoPago!.Id, compraForCreate.CodigoDescuento);

            // Asocia el usuario a la compra.
            compra.Usuario = user;

            // Asocia el método de pago a la compra.
            compra.MetodoPago = metodoPago;

            // Inicializa la lista de elementos de la compra.
            // Cada CompraItem representa un libro y su cantidad.
            compra.CompraItems = new List<CompraItem>();

            // Recorre todos los elementos seleccionados en el carrito.
            // Cada item contiene el Id del libro y la cantidad que se quiere comprar.
            foreach (var item in compraForCreate.Items)
            {
                // Busca dentro de la lista de libros recuperados
                // aquel cuyo identificador coincide con el del elemento actual.
                // Si no lo encuentra, devuelve null.
                Libro? libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                // Comprueba si el libro no existe en la lista recuperada.
                if (libro == null)
                {
                    // Añade un error indicando que el libro no existe.
                    ModelState.AddModelError("Items", $"Error! El libro con id {item.LibroId} no existe");
                }

                // Si el libro existe, comprueba si su stock es inferior
                // a la cantidad que el cliente quiere comprar.
                else if (libro.Stock < item.Cantidad)
                {
                    // Añade un error indicando que no hay unidades suficientes.
                    // También muestra el título del libro y el stock disponible.
                    ModelState.AddModelError("Items", $"Error! No hay stock suficiente de '{libro.Titulo}' (disponible: {libro.Stock})");
                }

                // Si el libro existe y hay stock suficiente:
                else
                {
                    // Añade un nuevo elemento a la compra.
                    // Libro: libro que se está comprando.
                    // Compra: compra a la que pertenece.
                    // Cantidad: unidades que quiere comprar el cliente.
                    compra.CompraItems.Add(new CompraItem { Libro = libro, Compra = compra, Cantidad = item.Cantidad });

                    // Descuenta del stock del libro la cantidad comprada.
                    libro.Stock -= item.Cantidad;
                }
            }

            // Comprueba si durante el recorrido de los libros se ha producido
            // algún error: libro inexistente o stock insuficiente.
            if (ModelState.ErrorCount > 0)

                // Si hay errores, devuelve HTTP 400 BadRequest
                // con el detalle de los errores.
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Calcula el precio total de la compra.
            // Sum recorre todos los elementos de CompraItems.
            // Para cada uno multiplica:
            // precio de una unidad del libro * cantidad comprada.
            // Finalmente suma todos los resultados.
            compra.PrecioTotal = compra.CompraItems.Sum(ci => ci.Libro.PrecioCompra * ci.Cantidad);

            // Añade la nueva compra al contexto de Entity Framework.
            // Queda preparada para guardarse en la base de datos.
            _context.Add(compra);

            // Bloque try-catch para controlar posibles excepciones
            // durante el guardado en la base de datos.
            try
            {
                // Guarda todos los cambios pendientes en la base de datos.
                // Se guardan la compra, sus elementos y los cambios de stock
                // y de datos del usuario.
                await _context.SaveChangesAsync();
            }

            // Si ocurre una excepción durante el guardado,
            // se captura en la variable ex.
            catch (Exception ex)
            {
                // Registra el mensaje de la excepción en el logger.
                _logger.LogError(ex.Message);

                // Devuelve HTTP 409 Conflict junto con el mensaje de error.
                return Conflict("Error" + ex.Message);
            }

            // Construye el DTO que se devolverá como resultado de la compra.
            // Se utilizan los datos de la compra, del usuario,
            // del método de pago y de los libros comprados.
            var compraDetail = new ComprasDetailDTO(

                // Identificador generado para la compra.
                compra.Id,

                // Nombre del cliente.
                user.Name,

                // Apellidos del cliente.
                user.Surname,

                // Dirección del cliente.
                user.Direccion,

                // Teléfono del cliente.
                user.PhoneNumber,

                // Fecha de realización de la compra.
                compra.FechaCompra,

                // Precio total calculado anteriormente.
                compra.PrecioTotal,

                // Determina el nombre del método de pago.
                // Devuelve Visa, GooglePay o PayPal según el tipo.
                metodoPago is Visa ? "Visa" : metodoPago is GooglePay ? "GooglePay" : "PayPal",

                // Código de descuento de la compra.
                compra.CodigoDescuento,

                // Convierte los elementos de la compra en una lista de DTOs.
                compra.CompraItems.Select(ci => new CompraItemDTO(

                    // Identificador del libro.
                    ci.LibroId,

                    // Título del libro.
                    ci.Libro.Titulo,

                    // Género del libro.
                    ci.Libro.Genero.Nombre,

                    // Editorial del libro.
                    ci.Libro.Editorial.Nombre,

                    // Precio por unidad del libro.
                    ci.Libro.PrecioCompra,

                    // Cantidad comprada.
                    ci.Cantidad)).ToList());

            // Devuelve HTTP 201 Created.
            // CreatedAtAction indica que el recurso se ha creado correctamente
            // y proporciona la referencia a GetDetailsForCompras.
            // new { id = compra.Id } indica el Id de la compra creada.
            // compraDetail es el contenido de la respuesta.
            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }
    }
}