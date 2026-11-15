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
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
                // Dune tiene stock 0: sirve para probar el error "sin stock"
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.0m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
            };

            // Usuario de prueba (id "3", nombre, apellido, email/username)
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            // Guardamos todo en la BD de pruebas
            _context.SaveChanges();
        }

        // SubastaForCreateDTO(userName, nombre, apellidos, telefono, metodoPago, descripcion, subastaItems)
        // Método auxiliar que construye un DTO de creación; descripcion, metodoPago y nombre tienen valores por defecto
        private static SubastaForCreateDTO NuevaSubasta(string usuarioUserName, List<SubastaItemDTO> items,
            string? descripcion = null, MetodoPagoSubastaTipos metodoPago = MetodoPagoSubastaTipos.GooglePay, string nombre = "Pedro")
        {
            // Apellidos y teléfono son fijos en todos los tests
            return new SubastaForCreateDTO(usuarioUserName, nombre, "Jackson Díaz", "612345678",
                metodoPago, descripcion, items);
        }

        // solo se usan LibroId y PrecioPuja: el resto de datos del libro los toma el controlador de la BD
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

            // Caso examen: Paco no puede pujar
            var subastaPujaPaco = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10) }, nombre: "Paco");

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
                new object[] { subastaPujaPaco, "¡Error!, los Pacos y los Pepes no pueden pujar" }
            };

            // Devolvemos los casos a xUnit
            return allTests;
        }

        // [Theory]: test parametrizado, se ejecuta una vez por cada caso
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateSubasta))]
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            // Comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Cogemos el primer mensaje del primer error del diccionario de errores
            var errorActual = problemDetails.Errors.First().Value[0];

            // Comprobamos que el mensaje de error esperado y el actual coinciden
            Assert.Equal(errorExpected, errorActual);

            // No se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }

        // [Fact]: test simple, sin parámetros
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_VariosErrores_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            // Libros repetidos y usuario no registrado a la vez
            var subastaDTO = NuevaSubasta("nadie@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10), Item(1, 20) });

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            // Se devuelven todos los errores, no solo el primero
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            Assert.Contains("Error! Hay libros repetidos en la subasta", problemDetails.Errors["SubastaItems"]);
            Assert.Contains("Error! UserName is not registered", problemDetails.Errors["SubastaApplicationUser"]);
            // No se guardó nada
            Assert.Empty(_context.Subastas);
        }

        // Único test del caso feliz: todo correcto
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
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            // Comprobamos que la respuesta es 201 y obtenemos el DTO devuelto
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            // DTO esperado completo (la fecha la pone el controlador con DateTime.Now,
            // por eso se copia del actual, ya que no se puede predecir exacta)
            var expectedSubastaDetailDTO = new SubastaDetailDTO(1, "Pedro", "Jackson Díaz", "612345678",
                MetodoPagoSubastaTipos.GooglePay, actualSubastaDetailDTO.FechaSubasta, _descripcion,
                new List<SubastaItemDTO>()
                {
                    new SubastaItemDTO(1, "El señor de los anillos", "Minotauro", "Tolkien", 20),
                    new SubastaItemDTO(2, "La naranja mecánica", "Minotauro", "Burgess", 30),
                });

            // Un único Equal que compara todo el objeto
            Assert.Equal(expectedSubastaDetailDTO, actualSubastaDetailDTO);
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

            // Un valor que no existe en el enum: CrearMetodoPago lanza la excepción
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(1, 10) },
                null, (MetodoPagoSubastaTipos)99);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => controller.CreateSubasta(subastaDTO));

            // No se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }
    }
}