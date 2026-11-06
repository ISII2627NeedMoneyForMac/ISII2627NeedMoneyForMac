// Da acceso a HttpStatusCode (OK, NotFound, Created...) usado en ProducesResponseType
using System.Net;
// Importa los DTOs (objetos de transferencia de datos) de Reposicion
using AppForSEII.API.DTOs.ReposicionDTOs;
// Importa las entidades del modelo (Reposicion, Libro, Visa, GooglePay, PayPal...)
using AppForSEII.API.Models;
// Aporta ControllerBase, ActionResult, [HttpGet], [HttpPost], etc.
using Microsoft.AspNetCore.Mvc;
// Aporta los métodos de EF Core como FirstOrDefaultAsync y SaveChangesAsync
using Microsoft.EntityFrameworkCore;

// Espacio de nombres donde vive el controlador
namespace AppForSEII.API.Controllers
{
    // La URL base será "api/Reposiciones" ([controller] se sustituye por el nombre sin "Controller")
    [Route("api/[controller]")]
    // Activa comportamientos automáticos de API (validación del modelo, inferencia de parámetros...)
    [ApiController]
    // Controlador de API (sin vistas) que gestiona las reposiciones
    public class ReposicionesController : ControllerBase
    {
        // Contexto de EF Core para acceder a la base de datos
        private readonly ApplicationDbContext _context;
        // Logger para registrar errores e información
        private readonly ILogger<ReposicionesController> _logger;

        // Constructor: ASP.NET inyecta el contexto y el logger (inyección de dependencias)
        public ReposicionesController(ApplicationDbContext context, ILogger<ReposicionesController> logger)
        {
            // Guarda el contexto en el campo privado
            _context = context;
            // Guarda el logger en el campo privado
            _logger = logger;
        }

// Este método responde a peticiones HTTP GET
[HttpGet]
// La ruta añade el nombre del método: GET api/Reposiciones/GetDetailsForReposiciones?id=5
[Route("[action]")]
// Documenta para Swagger que puede devolver 200 con un ReposicionDetailDTO
[ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.OK)]
// Documenta que también puede devolver 404 sin cuerpo
[ProducesResponseType((int)HttpStatusCode.NotFound)]
// Método asíncrono que recibe el id de la reposición y devuelve un resultado HTTP
public async Task<ActionResult> GetDetailsForReposiciones(int id)
{
    // MetodoPago es una jerarquía (Visa, GooglePay, PayPal): el tipo se deduce de la clase concreta
    // Consulta la tabla Reposiciones
    var datos = await _context.Reposiciones
        // Filtra por la reposición con el id recibido
        .Where(r => r.Id == id)
        // Proyección: solo se traen las columnas necesarias en un objeto anónimo
        .Select(r => new
        {
            // Id de la reposición
            r.Id,
            // Fecha en la que se hizo la reposición
            r.FechaReposicion,
            // Comentario de la reposición
            r.Comentario,
            // Nombre de usuario (el "!" indica al compilador que Usuario no es null)
            UserName = r.Usuario!.UserName,
            // Nombre del usuario
            Nombre = r.Usuario!.Name,
            // Apellidos del usuario (guardados juntos en un solo campo)
            Apellidos = r.Usuario!.Surname,
            // Teléfono del usuario
            Telefono = r.Usuario!.PhoneNumber,
            // true si el método de pago concreto es de tipo Visa (EF lo traduce a SQL con el discriminador)
            EsVisa = r.MetodoPago is Visa,
            // true si el método de pago concreto es de tipo GooglePay
            EsGooglePay = r.MetodoPago is GooglePay,
            // Líneas de la reposición
            Items = r.ReposicionItems!
                // Por cada línea crea un DTO con los datos del libro y la cantidad
                .Select(ri => new ReposicionItemDTO(
                    // Id del libro
                    ri.LibroId,
                    // Título del libro
                    ri.Libro.Titulo,
                    // Autor del libro
                    ri.Libro.Autor,
                    // Precio de reposición del libro
                    ri.Libro.PrecioReposicion,
                    // Cantidad repuesta
                    ri.CantidadReposicion))
                // Convierte el resultado a lista
                .ToList()
        })
        // Ejecuta la consulta y devuelve el primer resultado o null si no existe
        .FirstOrDefaultAsync();

    // Si no se encontró la reposición
    if (datos == null)
    {
        // Registra el error en el log
        _logger.LogError($"Error: Reposicion with id {id} does not exist");
        // Devuelve 404 Not Found
        return NotFound();
    }

    // Variable donde guardaremos el tipo de método de pago como enum
    MetodoPagoTipos tipo;
    // Si el método de pago es Visa
    if (datos.EsVisa)
    {
        // El tipo es Visa
        tipo = MetodoPagoTipos.Visa;
    }
    // Si no, pero es GooglePay
    else if (datos.EsGooglePay)
    {
        // El tipo es GooglePay
        tipo = MetodoPagoTipos.GooglePay;
    }
    // Si no es ninguno de los anteriores
    else
    {
        // Se asume PayPal
        tipo = MetodoPagoTipos.PayPal;
    }

    // ApplicationUser guarda los apellidos juntos (Surname): el primero hasta el primer espacio, el resto es el segundo
    // Divide los apellidos por el primer espacio en un máximo de 2 partes
    var apellidos = datos.Apellidos.Split(' ', 2);
    // La primera parte es el primer apellido
    var apellido1 = apellidos[0];
    // Por defecto el segundo apellido está vacío
    var apellido2 = "";
    // Si hubo una segunda parte
    if (apellidos.Length > 1)
    {
        // La segunda parte es el segundo apellido
        apellido2 = apellidos[1];
    }

    // Construye el DTO de respuesta ("datos.Telefono ?? """ usa cadena vacía si el teléfono es null)
    var reposicion = new ReposicionDetailDTO(datos.Id, datos.FechaReposicion, datos.UserName, datos.Nombre, apellido1, apellido2, datos.Telefono ?? "", tipo, datos.Comentario, datos.Items);

    // Devuelve 200 OK con el DTO en formato JSON
    return Ok(reposicion);
}
        // Este método responde a peticiones HTTP POST
 [HttpPost]
        // Ruta: POST api/Reposiciones/CreateReposicion
        [Route("[action]")]
        // Documenta que puede devolver 201 Created con el recibo
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.Created)]
        // Documenta que puede devolver 400 con los errores de validación
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        // Documenta que puede devolver 409 con un mensaje de texto
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        // Recibe en el cuerpo de la petición (JSON) los datos de la reposición a crear
        public async Task<ActionResult> CreateReposicion(ReposicionForCreateDTO reposicionForCreate)
        {
            // Las validaciones definidas en ReposicionForCreateDTO (datos obligatorios, comentario
            // entre 20 y 100 caracteres, cantidad mayor que 1...) se comprueban antes de ejecutar el método

            // Flujo alternativo 0: si todos los libros tienen stock > 0, no hay nada que reponer
            // Si NINGÚN libro tiene stock 0, añade un error al ModelState
            if (!_context.Libros.Any(l => l.Stock == 0))
                ModelState.AddModelError("Libros", "No hay libros disponibles para reponer");

            // Flujo alternativo 2 (respaldo en servidor): no se ha seleccionado ningún libro
            // Si la lista de items está vacía, añade un error
            if (reposicionForCreate.ReposicionItems.Count == 0)
                ModelState.AddModelError("ReposicionItems", "Error! Debes incluir al menos un libro para reponer");

            // Flujo alternativo 4 (respaldo en servidor): falta algún dato obligatorio
            // Si nombre, primer apellido o teléfono están vacíos o son solo espacios, añade un error
            if (string.IsNullOrWhiteSpace(reposicionForCreate.Nombre) ||
                string.IsNullOrWhiteSpace(reposicionForCreate.Apellido1) ||
                string.IsNullOrWhiteSpace(reposicionForCreate.Telefono))
                ModelState.AddModelError("DatosObligatorios", "Error! Faltan datos obligatorios por rellenar");

            // Busca en la BD el usuario por su nombre de usuario
            var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == reposicionForCreate.UserName);
            // Si el usuario no existe, añade un error
            if (user == null)
                ModelState.AddModelError("ReposicionApplicationUser", "Error! El nombre de usuario no está registrado");

            // Si se acumuló algún error de validación
            if (ModelState.ErrorCount > 0)
                // Devuelve 400 Bad Request con todos los errores en formato estándar
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Obtiene la lista de ids de los libros solicitados
            var libroIds = reposicionForCreate.ReposicionItems.Select(ri => ri.LibroId).ToList<int>();

            // Carga de la BD, en una sola consulta, los libros cuyos ids están en la lista (SQL: IN (...))
            var libros = _context.Libros
                .Where(l => libroIds.Contains(l.Id))
                .ToList();

            // el método de pago concreto depende del tipo elegido (Visa, GooglePay o PayPal)
            // Switch expression: crea la clase concreta según el enum recibido
            MetodoPago metodoPago = reposicionForCreate.MetodoPago switch
            {
                // Si es Visa, crea un objeto Visa
                MetodoPagoTipos.Visa => new Visa(),
                // Si es GooglePay, crea un objeto GooglePay
                MetodoPagoTipos.GooglePay => new GooglePay(),
                // Cualquier otro caso (PayPal), crea un objeto PayPal
                _ => new PayPal()
            };

            // Crea la entidad Reposicion
            var reposicion = new Reposicion
            {
                // Fecha y hora actuales
                FechaReposicion = DateTime.Now,
                // Comentario escrito por el usuario
                Comentario = reposicionForCreate.Comentario,
                // Usuario que realiza la reposición
                Usuario = user,
                // Método de pago creado arriba
                MetodoPago = metodoPago,
                // Lista vacía de items que se irá rellenando
                ReposicionItems = new List<ReposicionItem>()
            };

            // Recorre cada item (libro + cantidad) recibido
            foreach (var item in reposicionForCreate.ReposicionItems)
            {
                // Busca el libro del item en la lista ya cargada en memoria
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                // Si el libro no existe en la BD
                if (libro == null)
                {
                    // Añade un error indicando qué id no existe
                    ModelState.AddModelError("ReposicionItems", $"Error! El libro con id {item.LibroId} no existe");
                }
                // Si el libro existe
                else
                {
                    // la reposición todavía no existe en la base de datos y no tiene un Id válido,
                    // así que relacionamos el item con el objeto reposicion
                    // Crea el item y lo añade a la lista de la reposición
                    reposicion.ReposicionItems!.Add(new ReposicionItem
                    {
                        // Id del libro repuesto
                        LibroId = libro.Id,
                        // Enlaza con el objeto reposicion (EF resolverá el Id al guardar)
                        Reposicion = reposicion,
                        // Cantidad que se repone
                        CantidadReposicion = item.CantidadReposicion
                    });

                    // completamos el item con los datos reales del libro
                    // Título real del libro (el cliente no puede falsearlo)
                    item.Titulo = libro.Titulo;
                    // Autor real del libro
                    item.Autor = libro.Autor;
                    // Precio de reposición real de la BD
                    item.PrecioReposicion = libro.PrecioReposicion;
                }
            }

            // si algún libro no existe
            // Si el bucle anterior generó errores
            if (ModelState.ErrorCount > 0)
                // Devuelve 400 Bad Request con los errores
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Flujo alternativo 3: el total se calcula con los items finales del carrito
            // (así siempre coincide con los libros que quedan tras eliminar alguno)
            // Total = suma de (precio × cantidad) de cada item
            reposicion.PrecioTotal = reposicionForCreate.ReposicionItems
                .Sum(i => i.PrecioReposicion * i.CantidadReposicion);

            // datos que el usuario introduce en la reposición
            // Actualiza el nombre del usuario (el "!" indica que user no es null)
            user!.Name = reposicionForCreate.Nombre;
            // Actualiza los apellidos del usuario, guardados juntos separados por un espacio
            user.Surname = $"{reposicionForCreate.Apellido1} {reposicionForCreate.Apellido2}";
            // Actualiza el teléfono del usuario
            user.PhoneNumber = reposicionForCreate.Telefono;

            // al reponer, el stock de cada libro aumenta con la cantidad repuesta
            // Recorre de nuevo los items
            foreach (var item in reposicionForCreate.ReposicionItems)
            {
                // Obtiene el libro (First lanza excepción si no existe, pero ya se comprobó antes)
                var libro = libros.First(l => l.Id == item.LibroId);
                // Suma la cantidad repuesta al stock del libro
                libro.Stock += item.CantidadReposicion;
            }

            // Marca la reposición (y por cascada sus items y método de pago) para insertarla; aún no toca la BD
            _context.Add(reposicion);

            // Intenta guardar los cambios
            try
            {
                // guardamos en la base de datos la reposición y sus items
                // Ejecuta en una sola transacción: inserta reposición e items, y actualiza usuario y stocks
                await _context.SaveChangesAsync();
            }
            // Si ocurre cualquier error al guardar
            catch (Exception ex)
            {
                // Registra el mensaje de error en el log
                _logger.LogError(ex.Message);
                // Añade un error al ModelState (no tiene efecto real, porque no se devuelve el ModelState)
                ModelState.AddModelError("Reposicion", "Error! Ha ocurrido un error al guardar la reposición, inténtalo de nuevo más tarde");
                // Devuelve 409 Conflict con el mensaje de error
                return Conflict("Error" + ex.Message);
            }

            // devolvemos el detalle de la reposición (el recibo)
            // Construye el DTO de respuesta con el Id ya generado por la BD y los datos recibidos
            var reposicionDetail = new ReposicionDetailDTO(
                // Id generado al guardar
                reposicion.Id,
                // Fecha de la reposición
                reposicion.FechaReposicion,
                // Nombre de usuario
                reposicionForCreate.UserName,
                // Nombre
                reposicionForCreate.Nombre,
                // Primer apellido
                reposicionForCreate.Apellido1,
                // Segundo apellido
                reposicionForCreate.Apellido2,
                // Teléfono
                reposicionForCreate.Telefono,
                // Tipo de método de pago
                reposicionForCreate.MetodoPago,
                // Comentario
                reposicionForCreate.Comentario,
                // Items ya completados con los datos reales de los libros
                reposicionForCreate.ReposicionItems);

            // Devuelve 201 Created con la cabecera Location apuntando al GET de esta reposición y el recibo en el cuerpo
            return CreatedAtAction("GetDetailsForReposiciones", new { id = reposicion.Id }, reposicionDetail);
        }
    }
}