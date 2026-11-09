// Para usar HttpStatusCode (OK, NotFound, Created, BadRequest, Conflict...) en los ProducesResponseType
using System.Net;
// DTOs (objetos de transferencia de datos) de Subasta: lo que entra y sale de la API
using AppForSEII.API.DTOs.SubastaDTOs;
// Entidades del dominio: Subasta, SubastaItem, MetodoPago, Visa, PayPal, GooglePay...
using AppForSEII.API.Models;
// Clases de MVC: ControllerBase, ActionResult, ValidationProblemDetails, atributos [Route], [HttpGet]...
using Microsoft.AspNetCore.Mvc;
// Entity Framework: Include, AsNoTracking, FirstOrDefaultAsync, ToListAsync...
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AppForSEII.API.Controllers
{
    // La ruta base será "api/Subastas" ([controller] se sustituye por el nombre de la clase sin "Controller")
    [Route("api/[controller]")]
    // Indica que es un controlador de API: valida automáticamente los DataAnnotations de los DTOs
    // y devuelve 400 si no se cumplen, antes incluso de entrar al método
    [ApiController]
    // Hereda de ControllerBase (controlador sin soporte de vistas, típico de API)
    public class SubastasController : ControllerBase
    {
        // Contexto de base de datos (acceso a las tablas). readonly: solo se asigna en el constructor
        private readonly ApplicationDbContext _context;
        // Logger para escribir mensajes de error/información
        private readonly ILogger<SubastasController> _logger;

        // Constructor: ASP.NET inyecta (inyección de dependencias) el contexto y el logger
        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            // Guardamos el contexto recibido en el campo privado
            _context = context;
            // Guardamos el logger recibido en el campo privado
            _logger = logger;
        }

        // Este método responde a peticiones HTTP GET
        [HttpGet]
        // Ruta: api/Subastas/GetDetailsForSubastas ([action] = nombre del método)
        [Route("[action]")]
        // Documenta (Swagger) que puede devolver 200 OK con un SubastaDetailDTO
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]
        // Documenta que puede devolver 404 si no existe la subasta
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        // Método asíncrono que recibe el id de la subasta (por query string: ?id=1) y devuelve un ActionResult
        public async Task<ActionResult> GetDetailsForSubastas(int id)
        {
            // Consulta a la BD para obtener la subasta con todos sus datos relacionados
            var subasta = await _context.Subastas
                // No hace seguimiento de cambios: más rápido, ya que solo vamos a leer
                .AsNoTracking()
                // Carga también el usuario que creó la subasta
                .Include(s => s.Usuario)
                // Carga también el método de pago
                .Include(s => s.MetodoPago)
                // Carga la lista de items (líneas) de la subasta
                .Include(s => s.SubastaItems)
                    // De cada item, carga el libro
                    .ThenInclude(si => si.Libro)
                        // De cada libro, carga su editorial
                        .ThenInclude(l => l.Editorial)
                // Devuelve la primera subasta con ese id, o null si no existe
                .FirstOrDefaultAsync(s => s.Id == id);

            // Si no se ha encontrado la subasta...
            if (subasta == null)
            {
                // ...se registra un error en el log...
                _logger.LogError($"Error: Subasta with id {id} does not exist");
                // ...y se devuelve 404 Not Found
                return NotFound();
            }

            // Transformamos cada SubastaItem (entidad) en un SubastaItemDTO (lo que se enviará al cliente)
            var items = subasta.SubastaItems
                // Por cada item creamos un DTO con: id del libro, título, nombre de editorial, autor y precio de la puja
                .Select(si => new SubastaItemDTO(si.Libro.Id, si.Libro.Titulo,
                    si.Libro.Editorial.Nombre, si.Libro.Autor, si.PrecioPuja))
                // Convertimos el resultado en una lista
                .ToList();

            // Construimos el DTO de detalle de la subasta
            var detail = new SubastaDetailDTO(subasta.Id,
                // Nombre del usuario; si el usuario es null (?.) o su nombre es null (??) usamos cadena vacía
                subasta.Usuario?.Name ?? string.Empty,
                // Apellidos del usuario (o cadena vacía)
                subasta.Usuario?.Surname ?? string.Empty,
                // Teléfono del usuario (o cadena vacía)
                subasta.Usuario?.PhoneNumber ?? string.Empty,
                // Convertimos la clase de método de pago (Visa, PayPal...) en el enum del DTO
                ObtenerTipoMetodoPago(subasta.MetodoPago),
                // Fecha en que se creó la subasta
                subasta.FechaSubasta,
                // La descripción se guarda en cada item; cogemos la del primero (o null si no hay items)
                subasta.SubastaItems.Select(si => si.Descripcion).FirstOrDefault(),
                // Lista de items ya convertidos a DTO
                items);

            // Devolvemos 200 OK con el detalle en el cuerpo de la respuesta
            return Ok(detail);
        }

        // Este método responde a peticiones HTTP POST (crear)
        [HttpPost]
        // Ruta: api/Subastas/CreateSubasta
        [Route("[action]")]
        // Puede devolver 201 Created con un SubastaDetailDTO
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        // Puede devolver 400 Bad Request con los errores de validación
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        // Puede devolver 409 Conflict con un mensaje de texto si falla el guardado
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        // Recibe los datos de la subasta a crear (en el cuerpo de la petición)
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            // Los DataAnnotations (campos obligatorios, descripción 20-100, puja > 0) ya se validan antes de entrar

            // Flujo alternativo 4: debe haber al menos un libro en la lista de pujas
            // Si la lista de items está vacía...
            if (subastaForCreate.SubastaItems.Count == 0)
                // ...añadimos un error al ModelState (clave "SubastaItems", mensaje del error)
                ModelState.AddModelError("SubastaItems", "Error! Debes incluir al menos un libro en la subasta");

            // Comprobamos libros repetidos: si el número de LibroId distintos es distinto del total de items...
            if (subastaForCreate.SubastaItems.Select(si => si.LibroId).Distinct().Count()
                != subastaForCreate.SubastaItems.Count)
                // ...es que hay algún libro repetido: añadimos el error
                ModelState.AddModelError("SubastaItems", "Error! Hay libros repetidos en la subasta");

            
            if (subastaForCreate.Nombre.Equals("Paco") || subastaForCreate.Nombre.Equals("Pepe"))
            {
                ModelState.AddModelError("Nombre", "¡Error!, los Pacos y los Pepes no pueden pujar");
            }

            // PrecioPuja es int en la entidad: solo enteros entre 1 e int.MaxValue
            // Recorremos cada item para validar su precio de puja
            foreach (var item in subastaForCreate.SubastaItems)
            {
                // Si el precio es menor que 1 o mayor que el máximo de un int...
                if (item.PrecioPuja < 1 || item.PrecioPuja > int.MaxValue)
                    // ...error de rango (indicando el id del libro y el máximo)
                    ModelState.AddModelError("SubastaItems",
                        $"Error! El precio de la puja del libro '{item.LibroId}' debe estar entre 1 y {int.MaxValue}");
                // Si está en rango pero tiene decimales (el número no coincide con su parte entera)...
                else if (item.PrecioPuja != Math.Floor(item.PrecioPuja))
                    // ...error de que debe ser entero
                    ModelState.AddModelError("SubastaItems",
                        $"Error! El precio de la puja del libro '{item.LibroId}' debe ser un número entero");
            }

            // Buscamos en la BD al usuario cuyo UserName coincide con el recibido
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(au => au.UserName == subastaForCreate.UsuarioUserName);
            // Si no existe (null)...
            if (user == null)
                // ...añadimos error con la clave "SubastaApplicationUser"
                ModelState.AddModelError("SubastaApplicationUser", "Error! UserName is not registered");

            // Si se ha acumulado algún error hasta aquí...
            if (ModelState.ErrorCount > 0)
                // ...devolvemos 400 con TODOS los errores a la vez (no solo el primero)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Sacamos la lista de ids de libros que vienen en la petición
            var libroIds = subastaForCreate.SubastaItems.Select(si => si.LibroId).ToList();
            // Consultamos esos libros en la BD
            var libros = await _context.Libros
                // Incluimos la editorial (para poder devolver su nombre)
                .Include(l => l.Editorial)
                // Solo los libros cuyo id esté en la lista pedida
                .Where(l => libroIds.Contains(l.Id))
                // Ejecutamos la consulta y obtenemos una lista
                .ToListAsync();

            // Creamos la nueva entidad Subasta
            var subasta = new Subasta
            {
                // La fecha la pone el servidor: ahora mismo
                FechaSubasta = DateTime.Now,
                // Asociamos el usuario encontrado antes
                Usuario = user,
                // Creamos el objeto de método de pago según el enum recibido (Visa, GooglePay o PayPal)
                MetodoPago = CrearMetodoPago(subastaForCreate.MetodoPago)
            };

            // Lista donde iremos guardando los items ya en formato DTO para la respuesta
            var itemsDetail = new List<SubastaItemDTO>();

            // Recorremos cada item pedido por el cliente
            foreach (var item in subastaForCreate.SubastaItems)
            {
                // Buscamos en los libros cargados de la BD el que tiene ese id (null si no existe)
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                // Si el libro no existe en la BD...
                if (libro == null)
                {
                    // ...añadimos error
                    ModelState.AddModelError("SubastaItems", $"Error! El libro con id '{item.LibroId}' no existe");
                }
                // Si existe pero no tiene stock...
                else if (libro.Stock <= 0)
                {
                    // Flujo alternativo 0: solo se subastan libros con stock > 0
                    ModelState.AddModelError("SubastaItems", $"Error! El libro '{libro.Titulo}' no está disponible para subasta");
                }
                // Si existe y tiene stock: todo correcto
                else
                {
                    // Añadimos un nuevo SubastaItem a la subasta
                    subasta.SubastaItems.Add(new SubastaItem
                    {
                        // Libro al que se refiere la puja
                        Libro = libro,
                        // Subasta a la que pertenece este item
                        Subasta = subasta,
                        // Precio de la puja convertido de double a int (ya validado como entero)
                        PrecioPuja = (int)item.PrecioPuja,
                        // Descripción general de la subasta, copiada en cada item
                        Descripcion = subastaForCreate.Descripcion
                    });

                    // Se usan los datos de la BD, no los enviados por el cliente
                    // Añadimos el item en formato DTO para la respuesta
                    itemsDetail.Add(new SubastaItemDTO(libro.Id, libro.Titulo,
                        libro.Editorial.Nombre, libro.Autor, (int)item.PrecioPuja));
                }
            }
            
            

            // Si en el bucle anterior se añadieron errores (libro inexistente o sin stock)...
            if (ModelState.ErrorCount > 0)
                // ...devolvemos 400 con los errores
                return BadRequest(new ValidationProblemDetails(ModelState));

            // El precio total de la subasta es la suma de las pujas de todos sus items
            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);

            // los datos del formulario (paso 5) se guardan en el usuario, igual que en Compras
            // (user! indica al compilador que no es null: ya lo comprobamos antes)
            user!.Name = subastaForCreate.Nombre;
            // Guardamos los apellidos en el usuario
            user.Surname = subastaForCreate.Apellidos;
            // Guardamos el teléfono en el usuario
            user.PhoneNumber = subastaForCreate.Telefono;
            // Marcamos la subasta (y sus items) para insertarla en la BD
            _context.Add(subasta);

            // Intentamos guardar; try/catch por si la BD falla
            try
            {
                // Se guardan la subasta y sus items (y los cambios del usuario) en una sola transacción
                await _context.SaveChangesAsync();
            }
            // Si ocurre cualquier excepción al guardar...
            catch (Exception ex)
            {
                // ...se registra en el log...
                _logger.LogError(ex.Message);
                // ...y se devuelve 409 Conflict con un mensaje
                return Conflict("Error! There was an error while saving your subasta, please try again later: "
                    + ex.Message);
            }

            // Construimos el DTO de respuesta con los datos de la subasta recién creada
            var subastaDetail = new SubastaDetailDTO(subasta.Id,       // id generado por la BD
                subastaForCreate.Nombre,                               // nombre del formulario
                subastaForCreate.Apellidos,                            // apellidos del formulario
                subastaForCreate.Telefono,                             // teléfono del formulario
                subastaForCreate.MetodoPago, subasta.FechaSubasta,     // método de pago y fecha
                subastaForCreate.Descripcion, itemsDetail);            // descripción e items

            // Devolvemos 201 Created con: la acción para consultarla (GetDetailsForSubastas),
            // el id en la ruta y el detalle de la subasta en el cuerpo
            return CreatedAtAction(nameof (GetDetailsForSubastas), new { id = subasta.Id }, subastaDetail);
        }

        // enum del DTO -> clase de la entidad
        // Método auxiliar estático: según el valor del enum crea el objeto concreto de método de pago
        private static MetodoPago CrearMetodoPago(MetodoPagoSubastaTipos tipo) => tipo switch
        {
            // Si es Visa, creamos un objeto Visa
            MetodoPagoSubastaTipos.Visa => new Visa(),
            // Si es GooglePay, creamos un GooglePay
            MetodoPagoSubastaTipos.GooglePay => new GooglePay(),
            // Si es PayPal, creamos un PayPal
            MetodoPagoSubastaTipos.PayPal => new PayPal(),
            // Cualquier otro valor (no existente en el enum) lanza excepción
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        // clase de la entidad -> enum del DTO
        // Método auxiliar inverso: según el tipo del objeto devuelve el valor del enum
        private static MetodoPagoSubastaTipos ObtenerTipoMetodoPago(MetodoPago? metodoPago) => metodoPago switch
        {
            // Si el objeto es de tipo GooglePay -> enum GooglePay
            GooglePay => MetodoPagoSubastaTipos.GooglePay,
            // Si es PayPal -> enum PayPal
            PayPal => MetodoPagoSubastaTipos.PayPal,
            // En cualquier otro caso (Visa o null) -> Visa por defecto
            _ => MetodoPagoSubastaTipos.Visa
        };
    }
}