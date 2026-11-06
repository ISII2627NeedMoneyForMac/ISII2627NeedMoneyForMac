// Controlador que vamos a probar
using AppForSEII.API.Controllers;
// DTOs de subasta (entrada y salida del controlador)
using AppForSEII.API.DTOs.SubastaDTOs;
// Para usar Include() al consultar la BD en los tests
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.UT.SubastasController_test
{
    // Clase de tests. Hereda de AppForSEII4SqliteUT, que crea una BD SQLite en memoria nueva para cada test
    // y expone _context (contexto de BD) y CreateContext() (para abrir un contexto nuevo)
    public class CreateSubasta_test : AppForSEII4SqliteUT
    {
        // Descripción de ejemplo reutilizada en varios tests (const: valor fijo)
        private const string _descripcion = "Subasta de libros de ciencia ficción";

        // El constructor se ejecuta ANTES de cada test: deja la BD con datos de partida
        public CreateSubasta_test()
        {
            // Creamos un género de ejemplo (id 1)
            var genero = new Genero(1, "Ciencia ficción");
            // Creamos una editorial de ejemplo (id 1)
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: El señor de los anillos = 1, La naranja mecánica = 2, Dune = 3 (sin stock)
            // Lista de 3 libros; todos con el mismo género y editorial
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
                // Dune tiene stock 0 (el 0 de antes de "Tapa dura"): sirve para probar el error "sin stock"
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.0m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
            };

            // Usuario de prueba (id "3", nombre, apellido, email/username)
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");

            // Marcamos para insertar el usuario
            _context.ApplicationUsers.Add(user);
            // Marcamos para insertar el género
            _context.Add(genero);
            // Marcamos para insertar la editorial
            _context.Add(editorial);
            // Marcamos para insertar todos los libros
            _context.AddRange(libros);
            // Guardamos todo en la BD de pruebas
            _context.SaveChanges();
        }

        // SubastaForCreateDTO(userName, nombre, apellidos, telefono, metodoPago, descripcion, subastaItems)
        // Método auxiliar que construye un DTO de creación; descripcion y metodoPago tienen valores por defecto
        private static SubastaForCreateDTO NuevaSubasta(string usuarioUserName, List<SubastaItemDTO> items,
            string? descripcion = null, MetodoPagoSubastaTipos metodoPago = MetodoPagoSubastaTipos.GooglePay)
        {
            // Nombre, apellidos y teléfono son fijos en todos los tests
            return new SubastaForCreateDTO(usuarioUserName, "Pedro", "Jackson Díaz", "612345678",
                metodoPago, descripcion, items);
        }

        //solo se usan LibroId y PrecioPuja: el resto de datos del libro los toma el controlador de la BD
        // Método auxiliar que crea un item de puja con solo id de libro y precio
        private static SubastaItemDTO Item(int libroId, double precioPuja)
        {
            return new SubastaItemDTO { LibroId = libroId, PrecioPuja = precioPuja };
        }

        // Proporciona los casos de prueba (entrada + error esperado) al [Theory] de errores
        public static IEnumerable<object[]> TestCasesFor_CreateSubasta()
        {
            // Caso: lista de items vacía
            var subastaSinLibros = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO>());

            // Caso: el libro 1 aparece dos veces
            var subastaLibrosRepetidos = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10), Item(1, 20) });

            // Caso: puja de 0 (menor que 1)
            var subastaPujaCero = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 0) });

            // Caso: puja negativa
            var subastaPujaNegativa = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, -5) });

            // Caso: puja mayor que int.MaxValue (2147483647 + 1)
            var subastaPujaMayorQueIntMax = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 2147483648d) });

            // Caso: puja con decimales
            var subastaPujaDecimal = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10.5) });

            // Caso: usuario que no existe en la BD
            var subastaUsuarioNoRegistrado = NuevaSubasta("nadie@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10) });

            // Caso: libro con id 99, que no existe
            var subastaLibroInexistente = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(99, 10) });

            // Caso: libro 3 (Dune), que tiene stock 0
            var subastaLibroSinStock = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(3, 10) });

            // Lista de casos: cada fila es { DTO de entrada, mensaje de error esperado }
            var allTests = new List<object[]>
            {             //entrada de CreateSubasta - error esperado
                new object[] { subastaSinLibros, "Error! Debes incluir al menos un libro en la subasta" },
                new object[] { subastaLibrosRepetidos, "Error! Hay libros repetidos en la subasta" },
                new object[] { subastaPujaCero, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaNegativa, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaMayorQueIntMax, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaDecimal, "Error! El precio de la puja del libro '1' debe ser un número entero" },
                new object[] { subastaUsuarioNoRegistrado, "Error! UserName is not registered" },
                new object[] { subastaLibroInexistente, "Error! El libro con id '99' no existe" },
                new object[] { subastaLibroSinStock, "Error! El libro 'Dune' no está disponible para subasta" },
            };

            // Devolvemos los casos a xUnit
            return allTests;
        }

        // [Theory]: test parametrizado, se ejecuta una vez por cada caso
        [Theory]
        // Etiquetas (traits) para clasificar/filtrar tests
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        // Los parámetros salen del método TestCasesFor_CreateSubasta
        [MemberData(nameof(TestCasesFor_CreateSubasta))]
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaDTO, string errorExpected)
        {
            // Arrange
            // Creamos un logger falso (Moq) para no depender de uno real
            var mock = new Mock<ILogger<SubastasController>>();
            // Obtenemos el objeto logger del mock
            ILogger<SubastasController> logger = mock.Object;

            // Creamos el controlador con la BD de pruebas y el logger falso
            var controller = new SubastasController(_context, logger);

            // Act
            // Llamamos al método a probar
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            // Assert.IsType falla si no es de ese tipo; si lo es, nos devuelve el objeto ya convertido
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // El cuerpo del BadRequest debe ser un ValidationProblemDetails
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Cogemos el primer mensaje del primer error del diccionario de errores
            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual coinciden
            Assert.Equal(errorExpected, errorActual);

            //no se ha guardado ninguna subasta
            // La tabla de subastas debe seguir vacía
            Assert.Empty(_context.Subastas);
        }

        // [Fact]: test simple, sin parámetros
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_VariosErrores_test()
        {
            // Arrange
            // Logger falso
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            // Controlador a probar
            var controller = new SubastasController(_context, logger);

            //libros repetidos y usuario no registrado a la vez
            var subastaDTO = NuevaSubasta("nadie@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10), Item(1, 20) });

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            //se devuelven todos los errores, no solo el primero
            // Comprobamos que es BadRequest y extraemos sus detalles
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // En la clave "SubastaItems" debe estar el error de libros repetidos
            Assert.Contains("Error! Hay libros repetidos en la subasta", problemDetails.Errors["SubastaItems"]);
            // En la clave "SubastaApplicationUser" debe estar el error de usuario no registrado
            Assert.Contains("Error! UserName is not registered", problemDetails.Errors["SubastaApplicationUser"]);
            // No se guardó nada
            Assert.Empty(_context.Subastas);
        }

        // Test del caso feliz: todo correcto
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            // Subasta válida con dos libros (pujas 20 y 30) y descripción
            var subastaDTO = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 20), Item(2, 30) }, _descripcion);

            // Act
            // Guardamos la hora antes de llamar...
            var antes = DateTime.Now;
            // ...llamamos al método...
            var result = await controller.CreateSubasta(subastaDTO);
            // ...y guardamos la hora después: así tenemos un rango en el que debe estar la fecha de la subasta
            var despues = DateTime.Now;

            // Assert
            //comprobamos que la respuesta es 201 y apunta al details de la subasta creada
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            // El nombre de la acción a la que apunta debe ser GetDetailsForSubastas
            Assert.Equal("GetDetailsForSubastas", createdResult.ActionName);
            // El id de la ruta debe ser 1 (primera subasta de la BD)
            Assert.Equal(1, createdResult.RouteValues!["id"]);
            // El cuerpo debe ser un SubastaDetailDTO
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
            //y la reutilizamos en el DTO esperado
            Assert.InRange(actualSubastaDetailDTO.FechaSubasta, antes, despues);

            // SubastaDetailDTO(id, nombre, apellidos, telefono, metodoPago, fechaSubasta, descripcion, items)
            // SubastaItemDTO(libroId, titulo, editorial, autor, precioPuja)
            // Construimos el DTO que esperamos recibir (la fecha se copia del actual porque no se puede predecir exacta)
            var expectedSubastaDetailDTO = new SubastaDetailDTO(1, "Pedro", "Jackson Díaz", "612345678",
                MetodoPagoSubastaTipos.GooglePay, actualSubastaDetailDTO.FechaSubasta, _descripcion,
                new List<SubastaItemDTO>()
                {
                    new SubastaItemDTO(1, "El señor de los anillos", "Minotauro", "Tolkien", 20),
                    new SubastaItemDTO(2, "La naranja mecánica", "Minotauro", "Burgess", 30),
                });

            //comprobamos que el esperado y el actual son iguales
            Assert.Equal(expectedSubastaDetailDTO, actualSubastaDetailDTO);
            // El precio total (20 + 30) debe ser 50
            Assert.Equal(50m, actualSubastaDetailDTO.TotalPrice);

            //además, comprobamos en la base de datos (con un contexto nuevo) que se ha guardado todo
            // Contexto nuevo: así leemos de verdad de la BD y no de la caché del contexto anterior
            using var contextoNuevo = CreateContext();
            // Leemos la única subasta guardada, con sus items, método de pago y usuario
            var subastaGuardada = contextoNuevo.Subastas
                .Include(s => s.SubastaItems)
                .Include(s => s.MetodoPago)
                .Include(s => s.Usuario)
                .Single();

            // El precio guardado de la subasta es 50
            Assert.Equal(50d, subastaGuardada.PrecioSubasta);
            // Se guardaron 2 items
            Assert.Equal(2, subastaGuardada.SubastaItems.Count);
            //no se garantiza el orden al leer de la BD: ordenamos por libro
            // Las pujas, ordenadas por id de libro, son 20 y 30
            Assert.Equal(new[] { 20, 30 },
                subastaGuardada.SubastaItems.OrderBy(si => si.LibroId).Select(si => si.PrecioPuja).ToArray());
            // Todos los items tienen la descripción de la subasta
            Assert.All(subastaGuardada.SubastaItems, si => Assert.Equal(_descripcion, si.Descripcion));
            // El método de pago guardado es de tipo GooglePay
            Assert.IsType<GooglePay>(subastaGuardada.MetodoPago);

            //los datos del formulario se guardan en el usuario
            Assert.Equal("Pedro", subastaGuardada.Usuario!.Name);
            Assert.Equal("Jackson Díaz", subastaGuardada.Usuario.Surname);
            Assert.Equal("612345678", subastaGuardada.Usuario.PhoneNumber);

            //una subasta no modifica el stock de los libros
            Assert.Equal(5, contextoNuevo.Libros.Single(l => l.Id == 1).Stock);
            Assert.Equal(3, contextoNuevo.Libros.Single(l => l.Id == 2).Stock);
        }

        // Test parametrizado con valores límite (se ejecuta dos veces: puja 1 y puja int.MaxValue)
        [Theory]
        [InlineData(1d)]
        [InlineData(2147483647d)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_PujaLimite_Success_test(double puja)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //los valores límite permitidos: 1 e int.MaxValue
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(1, puja) });

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            // Debe devolver 201 Created
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);
            // El precio total debe ser igual a la puja (convertida a decimal)
            Assert.Equal((decimal)puja, actualSubastaDetailDTO.TotalPrice);

            // Comprobamos en la BD (contexto nuevo) que se guardó el mismo precio
            using var contextoNuevo = CreateContext();
            Assert.Equal(puja, contextoNuevo.Subastas.Single().PrecioSubasta);
        }

        // Test parametrizado: se ejecuta una vez por cada método de pago
        [Theory]
        [InlineData(MetodoPagoSubastaTipos.Visa)]
        [InlineData(MetodoPagoSubastaTipos.GooglePay)]
        [InlineData(MetodoPagoSubastaTipos.PayPal)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_MetodoPago_Success_test(MetodoPagoSubastaTipos metodoPago)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //además, la descripción es opcional, por eso va a null
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(2, 25) },
                null, metodoPago);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);
            // El método de pago devuelto coincide con el enviado
            Assert.Equal(metodoPago, actualSubastaDetailDTO.MetodoPago);
            // La descripción devuelta es null
            Assert.Null(actualSubastaDetailDTO.Descripcion);

            //el enum del DTO se convierte en la clase de método de pago correspondiente
            using var contextoNuevo = CreateContext();
            // Leemos de la BD el método de pago guardado
            var metodoPagoGuardado = contextoNuevo.Subastas.Include(s => s.MetodoPago).Single().MetodoPago;
            // Según el enum, comprobamos que la clase guardada es la correcta
            switch (metodoPago)
            {
                case MetodoPagoSubastaTipos.Visa: Assert.IsType<Visa>(metodoPagoGuardado); break;
                case MetodoPagoSubastaTipos.GooglePay: Assert.IsType<GooglePay>(metodoPagoGuardado); break;
                case MetodoPagoSubastaTipos.PayPal: Assert.IsType<PayPal>(metodoPagoGuardado); break;
            }
        }

        // Test de una excepción: método de pago con un valor no válido
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_MetodoPagoInvalido_Throws_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //un valor que no existe en el enum: CrearMetodoPago lanza la excepción
            // (MetodoPagoSubastaTipos)99 fuerza un valor inexistente del enum
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(1, 10) },
                null, (MetodoPagoSubastaTipos)99);

            // Act & Assert
            // Comprobamos que la llamada lanza ArgumentOutOfRangeException
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => controller.CreateSubasta(subastaDTO));

            //no se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }
    }
}