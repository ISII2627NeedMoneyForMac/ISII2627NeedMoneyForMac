// Importa el controlador que vamos a probar (ReposicionesController)
using AppForSEII.API.Controllers;
// Importa los DTOs de reposición (ReposicionForCreateDTO, ReposicionItemDTO, ReposicionDetailDTO...)
using AppForSEII.API.DTOs.ReposicionDTOs;

// Espacio de nombres del proyecto de tests unitarios
namespace AppForSEII.UT.ReposicionesController_test
{
    // Clase de tests de CreateReposicion; hereda de la clase base que prepara una BD SQLite en memoria (_context)
    public class CreateReposiciones_test : AppForSEII4SqliteUT
    {
        // Datos de prueba reutilizables (constantes para no repetir textos en cada test)
        // Nombre de usuario (email) del usuario de prueba
        private const string _userName = "laura@uclm.es";
        // Nombre del usuario
        private const string _nombre = "Laura";
        // Primer apellido
        private const string _apellido1 = "Sánchez";
        // Segundo apellido
        private const string _apellido2 = "Ruiz";
        // Teléfono
        private const string _telefono = "+600123456";
        // Comentario de la reposición
        private const string _comentario = "Reposición de libros agotados";

        // Título del libro 1
        private const string _libro1Titulo = "El señor de los anillos";
        // Autor del libro 1
        private const string _libro1Autor = "Tolkien";
        // Título del libro 2
        private const string _libro2Titulo = "La naranja mecánica";
        // Autor del libro 2
        private const string _libro2Autor = "Burgess";

        // Constructor: se ejecuta ANTES de cada test y deja la BD con datos iniciales
        public CreateReposiciones_test()
        {
            // Crea un género de prueba (id 1)
            var genero = new Genero(1, "Ciencia ficción");
            // Crea una editorial de prueba (id 1)
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // Lista de libros que se guardarán en la BD
            var libros = new List<Libro>(){
                // libro 1: sin stock, se puede reponer
                new Libro(_libro1Titulo, _libro1Autor, new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    // Asigna género y editorial mediante inicializador de objeto
                    { Genero = genero, Editorial = editorial },
                // libro 2: con stock
                new Libro(_libro2Titulo, _libro2Autor, new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    // Asigna género y editorial
                    { Genero = genero, Editorial = editorial },
            };

            // Crea el usuario de prueba (id "2", nombre, apellidos juntos y nombre de usuario)
            ApplicationUser user = new ApplicationUser("2", "Laura", "Sánchez Ruiz", _userName);

            // Añade el usuario al contexto
            _context.ApplicationUsers.Add(user);
            // Añade el género al contexto
            _context.Add(genero);
            // Añade la editorial al contexto
            _context.Add(editorial);
            // Añade todos los libros al contexto
            _context.AddRange(libros);
            // Guarda todo en la base de datos de prueba
            _context.SaveChanges();
        }
        // ReposicionForCreateDTO(userName, nombre, apellido1, apellido2, telefono, metodoPago, comentario, items)
// Crea un DTO de creación válido; solo hay que indicar lo que cambia en cada test
private static ReposicionForCreateDTO NuevaReposicion(string usuarioUserName, List<ReposicionItemDTO> items,
    string nombre = _nombre, string telefono = _telefono,
    string? comentario = _comentario, MetodoPagoTipos metodoPago = MetodoPagoTipos.Visa)
{
    // Los apellidos son fijos en todos los tests
    return new ReposicionForCreateDTO(usuarioUserName, nombre, _apellido1, _apellido2,
        telefono, metodoPago, comentario, items);
}

// Solo se usan LibroId y CantidadReposicion: título, autor y precio los toma el controlador de la BD
private static ReposicionItemDTO Item(int libroId, int cantidad)
{
    return new ReposicionItemDTO(libroId, "", "", 0m, cantidad);
}

        // Método que proporciona los casos de prueba (datos de entrada + error esperado) al test con [MemberData]
        public static IEnumerable<object[]> TestCasesFor_CreateReposicion()
        {
            // Función (lambda) que devuelve una lista NUEVA de items cada vez que se llama (evita compartir la misma instancia entre casos)
            var items = () => new List<ReposicionItemDTO>()
                // Un item: libro 1, con su título, autor, precio 10 y cantidad 5
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) };

            // no se ha incluido ningún libro
            // Reposición con lista de items vacía
            var reposicionSinItems = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario, new List<ReposicionItemDTO>());

            // falta un dato obligatorio (nombre vacío)
            // Reposición con el nombre vacío ("")
            var reposicionSinNombre = new ReposicionForCreateDTO(_userName, "", _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario, items());

            // el usuario no está registrado
            // Reposición con un nombre de usuario que no existe en la BD
            var reposicionUsuarioNoRegistrado = new ReposicionForCreateDTO("victor.lopez@uclm.es", _nombre,
                _apellido1, _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario, items());

            // el libro no existe
            // Reposición con un item cuyo LibroId (99) no existe en la BD
            var reposicionLibroInexistente = new ReposicionForCreateDTO(_userName, _nombre, _apellido1,
                _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>() { new ReposicionItemDTO(99, "Libro inventado", "Nadie", 10.0m, 5) });

            // Lista con todos los casos: cada uno es un array [DTO de entrada, mensaje de error esperado]
            var allTests = new List<object[]>
            {             //input for createreposicion - Error expected
                // Caso 1: sin items -> error de "al menos un libro"
                new object[] { reposicionSinItems, "Error! Debes incluir al menos un libro para reponer", },
                // Caso 2: sin nombre -> error de datos obligatorios
                new object[] { reposicionSinNombre, "Error! Faltan datos obligatorios por rellenar", },
                // Caso 3: usuario no registrado -> error de usuario
                new object[] { reposicionUsuarioNoRegistrado, "Error! El nombre de usuario no está registrado", },
                // Caso 4: libro inexistente -> error indicando el id
                new object[] { reposicionLibroInexistente, "Error! El libro con id 99 no existe", },
                // Caso 5: teléfono incorrecto -> error de formato
            };

            // Devuelve los casos para que xUnit ejecute el test una vez por cada uno
            return allTests;
        }

        // Theory: el mismo test se ejecuta varias veces con distintos datos
        [Theory]
        // Etiqueta para filtrar tests: nivel de prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: este test no usa fixture de base de datos
        [Trait("Database", "WithoutFixture")]
        // Los datos de entrada vienen del método estático de arriba
        [MemberData(nameof(TestCasesFor_CreateReposicion))]
        // Recibe el DTO de entrada y el mensaje de error que se espera
        public async Task CreateReposicion_Error_test(ReposicionForCreateDTO reposicionDTO, string errorExpected)
        {
            // Arrange
            // Crea un logger falso (mock) para no escribir logs reales
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Obtiene el objeto logger a partir del mock
            ILogger<ReposicionesController> logger = mock.Object;

            // Crea el controlador con la BD de prueba y el logger falso
            var controller = new ReposicionesController(_context, logger);

            // Act
            // Llama al método que se está probando
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            // Verifica que el resultado es un 400 BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Verifica que el cuerpo de la respuesta es un ValidationProblemDetails
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Obtiene el primer mensaje de error del primer campo con errores
            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual son iguales
            // Verifica que el mensaje real empieza por el esperado
            Assert.StartsWith(errorExpected, errorActual);
        }

        // Fact: test con un único caso (sin parámetros)
        [Fact]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Test del caso en que no hay ningún libro sin stock
        public async Task CreateReposicion_NoLibrosParaReponer_test()
        {
            // Arrange
            // todos los libros tienen stock > 0, así que no hay nada que reponer
            // Recorre todos los libros de la BD
            foreach (var libro in _context.Libros.ToList())
                // Pone el stock de cada uno a 5
                libro.Stock = 5;
            // Guarda el cambio de stock en la BD
            _context.SaveChanges();

            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Obtiene el objeto logger
            ILogger<ReposicionesController> logger = mock.Object;
            // Crea el controlador
            var controller = new ReposicionesController(_context, logger);

            // DTO válido con un item del libro 1 (el error vendrá solo de que no hay libros con stock 0)
            var reposicionDTO = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>() { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            // Act
            // Ejecuta el método a probar
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            // Verifica que devuelve 400 BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Verifica que el cuerpo es un ValidationProblemDetails
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Obtiene el primer mensaje de error
            var errorActual = problemDetails.Errors.First().Value[0];
            // Verifica que el mensaje es el de "no hay libros para reponer"
            Assert.StartsWith("No hay libros disponibles para reponer", errorActual);
        }

        // Test de un caso (sin parámetros)
        [Fact]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Test del caso correcto: la reposición se crea bien
public async Task CreateReposicion_Success_test()
{
    // Arrange
    var mock = new Mock<ILogger<ReposicionesController>>();
    ILogger<ReposicionesController> logger = mock.Object;
    var controller = new ReposicionesController(_context, logger);

    // Nombre distinto al del usuario guardado ("Laura"): así comprobamos que el controlador lo actualiza
    const string nombreNuevo = "Pedro";

    // Reposición válida: 5 unidades del libro 1 (que tiene stock 0)
    var reposicionDTO = NuevaReposicion(_userName,
        new List<ReposicionItemDTO> { Item(1, 5) }, nombre: nombreNuevo);

    // Act
    // Rango de tiempo en el que debe estar la fecha de la reposición
    var antes = DateTime.Now;
    var result = await controller.CreateReposicion(reposicionDTO);
    var despues = DateTime.Now;

    // Assert
    //comprobamos que la respuesta es 201 y apunta al details de la reposición creada
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    Assert.Equal("GetDetailsForReposiciones", createdResult.ActionName);
    Assert.Equal(1, createdResult.RouteValues!["id"]);
    var actualReposicionDetailDTO = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

    //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
    Assert.InRange(actualReposicionDetailDTO.FechaReposicion, antes, despues);

    // Construimos el DTO esperado (la fecha se copia del actual porque no se puede predecir exacta)
    var expectedReposicionDetailDTO = new ReposicionDetailDTO(1, actualReposicionDetailDTO.FechaReposicion,
        _userName, nombreNuevo, _apellido1, _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario,
        new List<ReposicionItemDTO>()
        { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

    Assert.Equal(expectedReposicionDetailDTO, actualReposicionDetailDTO);
    // El precio total (10 x 5) debe ser 50
    Assert.Equal(50m, actualReposicionDetailDTO.TotalPrice);

    //además, comprobamos en la base de datos (con un contexto nuevo) que se ha guardado todo
    using var contextoNuevo = CreateContext();
    var reposicionGuardada = contextoNuevo.Reposiciones
        .Include(r => r.ReposicionItems)
        .Include(r => r.MetodoPago)
        .Include(r => r.Usuario)
        .Single();

    Assert.Equal(50m, reposicionGuardada.PrecioTotal);
    Assert.Equal(_comentario, reposicionGuardada.Comentario);
    Assert.Single(reposicionGuardada.ReposicionItems!);
    Assert.Equal(1, reposicionGuardada.ReposicionItems!.Single().LibroId);
    Assert.Equal(5, reposicionGuardada.ReposicionItems!.Single().CantidadReposicion);
    Assert.IsType<Visa>(reposicionGuardada.MetodoPago);

    //los datos del formulario se guardan en el usuario
    Assert.Equal(nombreNuevo, reposicionGuardada.Usuario!.Name);
    Assert.Equal($"{_apellido1} {_apellido2}", reposicionGuardada.Usuario.Surname);
    Assert.Equal(_telefono, reposicionGuardada.Usuario.PhoneNumber);

    //reponer sube el stock del libro 1 (de 0 a 5) y no toca el del libro 2
    Assert.Equal(5, contextoNuevo.Libros.Single(l => l.Id == 1).Stock);
    Assert.Equal(3, contextoNuevo.Libros.Single(l => l.Id == 2).Stock);
}

        // NUEVO: cubre las ramas GooglePay y PayPal del switch de metodoPago
        // Theory con InlineData: se ejecuta una vez por cada línea de datos
        [Theory]
        // Caso 1: tipo GooglePay, se espera la clase GooglePay
        [InlineData(MetodoPagoTipos.GooglePay, typeof(GooglePay))]
        // Caso 2: tipo PayPal, se espera la clase PayPal
        [InlineData(MetodoPagoTipos.PayPal, typeof(PayPal))]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Recibe el tipo de pago a probar y la clase concreta que debería guardarse
        public async Task CreateReposicion_MetodoPago_test(MetodoPagoTipos tipo, Type tipoEsperado)
        {
            // Arrange
            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Obtiene el objeto logger
            ILogger<ReposicionesController> logger = mock.Object;

            // Crea el controlador
            var controller = new ReposicionesController(_context, logger);

            // DTO válido, pero con el método de pago que llega por parámetro
            var reposicionDTO = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, tipo, _comentario,
                new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            // Act
            // Ejecuta el método a probar
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            // Verifica que devuelve 201 Created
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            // Verifica que el cuerpo es un ReposicionDetailDTO
            var detail = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            // el DTO devuelto lleva el tipo de pago elegido
            // Verifica que el tipo de pago del recibo coincide con el enviado
            Assert.Equal(tipo, detail.MetodoPago);

            // y en la base de datos se ha guardado la clase concreta correspondiente
            // Obtiene la única reposición guardada (Single falla si hay 0 o más de 1)
            var guardada = _context.Reposiciones.Single();
            // Verifica que el método de pago guardado es exactamente de la clase esperada
            Assert.IsType(tipoEsperado, guardada.MetodoPago);
        }

    }
}