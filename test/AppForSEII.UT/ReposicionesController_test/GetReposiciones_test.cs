// Importa el controlador que vamos a probar (ReposicionesController)
using AppForSEII.API.Controllers;
// Importa los DTOs de reposición (ReposicionDetailDTO, ReposicionItemDTO...)
using AppForSEII.API.DTOs.ReposicionDTOs;

// Espacio de nombres del proyecto de tests unitarios
namespace AppForSEII.UT.ReposicionesController_test
{
    // Clase de tests de GetDetailsForReposiciones; hereda de la clase base que prepara una BD SQLite en memoria (_context)
    public class GetReposiciones_test : AppForSEII4SqliteUT
    {
        // Constructor: se ejecuta ANTES de cada test y deja la BD con datos iniciales
        public GetReposiciones_test()
        {
            // Crea un género de prueba (id 1)
            var genero = new Genero(1, "Ciencia ficción");
            // Crea una editorial de prueba (id 1)
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // Lista de libros que se guardarán en la BD
            var libros = new List<Libro>(){
                // Libro 1: stock 0, precio de reposición 10
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    // Asigna género y editorial mediante inicializador de objeto
                    { Genero = genero, Editorial = editorial },
                // Libro 2: stock 0, precio de reposición 15
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 0, "Tapa blanda")
                    // Asigna género y editorial
                    { Genero = genero, Editorial = editorial },
            };

            // El controller separa Surname en Apellido1 y Apellido2 por el primer espacio
            // Crea el usuario de prueba (id "1", nombre, apellidos juntos y nombre de usuario)
            ApplicationUser user = new ApplicationUser("1", "Daniel", "García López", "daniel@uclm.es");
            // Asigna el teléfono del usuario
            user.PhoneNumber = "600123456";

            // Crea una reposición ya existente en la BD (tendrá Id 1)
            var reposicion = new Reposicion
            {
                // Fecha actual
                FechaReposicion = DateTime.Now,
                // Total de la reposición
                PrecioTotal = 50.0m, // 5 unidades x 10.0
                // Comentario de la reposición
                Comentario = "Reposición de libros agotados",
                // Usuario que hizo la reposición
                Usuario = user,
                // Método de pago Visa (número de tarjeta y fecha de caducidad a 2 años)
                MetodoPago = new Visa("4111111111111111", DateTime.Today.AddYears(2)),
                // Lista vacía de items que se rellena justo debajo
                ReposicionItems = new List<ReposicionItem>()
            };
            // Añade un item a la reposición
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                // Libro repuesto: el libro 1
                Libro = libros[0],
                // Enlaza el item con su reposición
                Reposicion = reposicion,
                // Se reponen 5 unidades
                CantidadReposicion = 5
            });

            // Añade el usuario al contexto
            _context.ApplicationUsers.Add(user);
            // Añade el género al contexto
            _context.Add(genero);
            // Añade la editorial al contexto
            _context.Add(editorial);
            // Añade los libros al contexto
            _context.AddRange(libros);
            // Añade la reposición (con sus items y método de pago) al contexto
            _context.Add(reposicion);
            // Guarda todo en la base de datos de prueba
            _context.SaveChanges();
        }

        // Fact: test con un único caso (sin parámetros)
        [Fact]
        // Etiqueta: este test no usa fixture de base de datos
        [Trait("Database", "WithoutFixture")]
        // Etiqueta para filtrar tests: nivel de prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Test del caso en que la reposición no existe
        public async Task GetReposicion_NotFound_test()
        {
            // Arrange
            // Crea un logger falso (mock) para no escribir logs reales
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Obtiene el objeto logger a partir del mock
            ILogger<ReposicionesController> logger = mock.Object;

            // Crea el controlador con la BD de prueba y el logger falso
            var controller = new ReposicionesController(_context, logger);

            // Act
            // Pide una reposición con id 0, que no existe
            var result = await controller.GetDetailsForReposiciones(0);

            // Assert
            // comprobamos que la respuesta es NotFound
            // Verifica que el resultado es un 404 NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        // Test de un caso (sin parámetros)
        [Fact]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Test del caso en que la reposición existe
        public async Task GetReposicion_Found_test()
        {
            // Arrange
            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Obtiene el objeto logger
            ILogger<ReposicionesController> logger = mock.Object;
            // Crea el controlador
            var controller = new ReposicionesController(_context, logger);

            // Detalle que se espera recibir: apellidos ya separados en "García" y "López"
            var expectedReposicion = new ReposicionDetailDTO(1, DateTime.Now, "daniel@uclm.es", "Daniel",
                                "García", "López", "600123456", MetodoPagoTipos.Visa,
                                "Reposición de libros agotados",
                                new List<ReposicionItemDTO>());
            // ReposicionItemDTO(libroId, titulo, autor, precioReposicion, cantidadReposicion)
            // Añade al detalle esperado el item del libro 1 con cantidad 5
            expectedReposicion.ReposicionItems.Add(
                new ReposicionItemDTO(1, "El señor de los anillos", "Tolkien", 10.0m, 5));

            // Act
            // Pide la reposición con id 1
            var result = await controller.GetDetailsForReposiciones(1);

            // Assert
            // comprobamos que la respuesta es OK y obtenemos la reposición
            // Verifica que el resultado es un 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el cuerpo es un ReposicionDetailDTO
            var reposicionDTOActual = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            // comprobamos que la esperada y la obtenida son iguales
            // Compara el detalle esperado con el recibido
            Assert.Equal(expectedReposicion, reposicionDTOActual);
        }

        // Theory: el mismo test se ejecuta varias veces con distintos datos
        [Theory]
        // Caso 1: método de pago GooglePay
        [InlineData(MetodoPagoTipos.GooglePay)]
        // Caso 2: método de pago PayPal
        [InlineData(MetodoPagoTipos.PayPal)]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Recibe el tipo de pago con el que se creará la segunda reposición
        public async Task GetReposicion_MetodoPago_test(MetodoPagoTipos tipo)
        {
            // Arrange: segunda reposición (Id = 2) con otro método de pago
            // Obtiene el primer usuario de la BD
            var user = _context.ApplicationUsers.First();
            // Obtiene el primer libro de la BD
            var libro = _context.Libros.First();

            // Crea la clase concreta de método de pago según el tipo recibido
            MetodoPago metodoPago = tipo == MetodoPagoTipos.GooglePay
                // Si es GooglePay, crea un GooglePay
                ? new GooglePay()
                // Si no, crea un PayPal
                : new PayPal();

            // Crea la segunda reposición
            var reposicion = new Reposicion
            {
                // Fecha actual
                FechaReposicion = DateTime.Now,
                // Total de la reposición
                PrecioTotal = 10.0m,
                // Comentario de la reposición
                Comentario = "Reposición con otro método de pago",
                // Usuario existente
                Usuario = user,
                // Método de pago elegido en el test
                MetodoPago = metodoPago,
                // Lista vacía de items
                ReposicionItems = new List<ReposicionItem>()
            };
            // Añade un item a la reposición
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                // Libro existente
                Libro = libro,
                // Enlaza el item con su reposición
                Reposicion = reposicion,
                // Se repone 1 unidad
                CantidadReposicion = 1
            });
            // Añade la reposición al contexto
            _context.Add(reposicion);
            // Guarda en la BD
            _context.SaveChanges();

            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Crea el controlador
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            // Pide la reposición recién creada (usa el Id generado por la BD)
            var result = await controller.GetDetailsForReposiciones(reposicion.Id);

            // Assert
            // Verifica que devuelve 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el cuerpo es un ReposicionDetailDTO
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            // Verifica que el tipo de pago devuelto coincide con el guardado
            Assert.Equal(tipo, dto.MetodoPago);
        }

        // NUEVO: cubre el caso de un único apellido (apellido2 = "")
        // Test de un caso (sin parámetros)
        [Fact]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Test del caso en que el usuario solo tiene un apellido
        public async Task GetReposicion_UnSoloApellido_test()
        {
            // Arrange: usuario con un único apellido
            // Crea un usuario cuyo Surname no tiene espacios
            var user = new ApplicationUser("3", "Pedro", "Martín", "pedro@uclm.es");
            // Asigna el teléfono
            user.PhoneNumber = "600999888";
            // Obtiene el primer libro de la BD
            var libro = _context.Libros.First();

            // Crea una reposición para ese usuario
            var reposicion = new Reposicion
            {
                // Fecha actual
                FechaReposicion = DateTime.Now,
                // Total de la reposición
                PrecioTotal = 10.0m,
                // Comentario de la reposición
                Comentario = "Reposición con un solo apellido",
                // Usuario con un solo apellido
                Usuario = user,
                // Método de pago PayPal
                MetodoPago = new PayPal(),
                // Lista vacía de items
                ReposicionItems = new List<ReposicionItem>()
            };
            // Añade un item a la reposición
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                // Libro existente
                Libro = libro,
                // Enlaza el item con su reposición
                Reposicion = reposicion,
                // Se repone 1 unidad
                CantidadReposicion = 1
            });
            // Añade el usuario nuevo al contexto
            _context.ApplicationUsers.Add(user);
            // Añade la reposición al contexto
            _context.Add(reposicion);
            // Guarda en la BD
            _context.SaveChanges();

            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Crea el controlador
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            // Pide la reposición recién creada
            var result = await controller.GetDetailsForReposiciones(reposicion.Id);

            // Assert
            // Verifica que devuelve 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el cuerpo es un ReposicionDetailDTO
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            // Verifica que el primer apellido es "Martín"
            Assert.Equal("Martín", dto.Apellido1);
            // Verifica que el segundo apellido queda vacío
            Assert.Equal("", dto.Apellido2);
        }

        // Test de un caso (sin parámetros)
        [Fact]
        // Etiqueta: prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta: sin fixture de BD
        [Trait("Database", "WithoutFixture")]
        // Test del caso en que el usuario no tiene teléfono (null)
        public async Task GetReposicion_TelefonoNull_test()
        {
            // Arrange
            // Obtiene el primer usuario de la BD
            var user = _context.ApplicationUsers.First();
            // Le quita el teléfono (null)
            user.PhoneNumber = null;
            // Guarda el cambio en la BD
            _context.SaveChanges();

            // Crea el logger falso
            var mock = new Mock<ILogger<ReposicionesController>>();
            // Crea el controlador
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            // Pide la reposición con id 1
            var result = await controller.GetDetailsForReposiciones(1);

            // Assert
            // Verifica que devuelve 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el cuerpo es un ReposicionDetailDTO
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);

            // Verifica que el teléfono null se convirtió en cadena vacía (por el "?? """ del controlador)
            Assert.Equal("", dto.Telefono);
        }
    }
}