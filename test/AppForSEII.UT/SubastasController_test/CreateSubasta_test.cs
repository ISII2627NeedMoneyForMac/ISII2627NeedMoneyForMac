// Controlador que vamos a probar
using AppForSEII.API.Controllers;
// DTOs de subasta (entrada y salida del controlador)
using AppForSEII.API.DTOs.SubastaDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    // Clase de tests. Hereda de AppForSEII4SqliteUT, que crea una BD SQLite en memoria nueva para cada test
    public class CreateSubasta_test : AppForSEII4SqliteUT
    {
        // Datos fijos reutilizados en los tests (como en CreateRentals_test del ejemplo)
        private const string _userName = "peter@uclm.es";
        private const string _descripcion = "Subasta de libros de ciencia ficción";
        private const string _editorial = "Minotauro";

        private const string _libro1Titulo = "El señor de los anillos";
        private const string _libro1Autor = "Tolkien";
        private const string _libro2Titulo = "La naranja mecánica";
        private const string _libro2Autor = "Burgess";
        private const string _libro3Titulo = "Dune";
        private const string _libro3Autor = "Frank Herbert";

        // El constructor se ejecuta ANTES de cada test: deja la BD con datos de partida
        public CreateSubasta_test()
        {
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, _editorial);

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro, genero, editorial)
            // ids: El señor de los anillos = 1, La naranja mecánica = 2, Dune = 3 (sin stock)
            var libros = new List<Libro>(){
                new Libro(_libro1Titulo, _libro1Autor, new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura", genero, editorial),
                new Libro(_libro2Titulo, _libro2Autor, new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda", genero, editorial),
                // Dune tiene stock 0: sirve para probar el error "sin stock"
                new Libro(_libro3Titulo, _libro3Autor, new DateTime(1965, 8, 1), 20.0m, 12.0m, 0, "Tapa dura", genero, editorial),
            };

            // Usuario de prueba (id, nombre, apellido, email/username)
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", _userName);

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // SubastaForCreateDTO(userName, nombre, apellidos, telefono, metodoPago, descripcion, subastaItems)
        // Método auxiliar que construye un DTO de creación; descripcion, metodoPago y nombre tienen valores por defecto
        private static SubastaForCreateDTO NuevaSubasta(string usuarioUserName, List<SubastaItemDTO> items,
            string? descripcion = null, MetodoPagoSubastaTipos metodoPago = MetodoPagoSubastaTipos.GooglePay, string nombre = "Pedro")
        {
            return new SubastaForCreateDTO(usuarioUserName, nombre, "Jackson Díaz", "612345678",
                metodoPago, descripcion, items);
        }

        // Proporciona los casos de prueba (entrada + error esperado) al [Theory] de errores
        public static IEnumerable<object[]> TestCasesFor_CreateSubasta()
        {
            // SubastaItemDTO(libroId, titulo, editorial, autor, precioPuja)
            var subastaSinLibros = NuevaSubasta(_userName, new List<SubastaItemDTO>());

            var subastaLibrosRepetidos = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 10),
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 20) });

            var subastaPujaCero = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 0) });

            var subastaPujaNegativa = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, -5) });

            var subastaPujaMayorQueIntMax = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 2147483648d) });

            var subastaPujaDecimal = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 10.5) });

            var subastaUsuarioNoRegistrado = NuevaSubasta("nadie@uclm.es", new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 10) });

            // El libro con id 99 no existe en la BD
            var subastaLibroInexistente = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(99, "Libro inexistente", _editorial, "Autor desconocido", 10) });

            // Dune (id 3) tiene stock 0
            var subastaLibroSinStock = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(3, _libro3Titulo, _editorial, _libro3Autor, 10) });

            // Caso examen: Paco no puede pujar
            var subastaPujaPaco = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 10) }, nombre: "Paco");

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

            return allTests;
        }

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
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual coinciden
            Assert.Equal(errorExpected, errorActual);
            //no se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }

        // Caso feliz: un único test parametrizado con los tres métodos de pago,
        // necesario para ejecutar todas las líneas del switch CrearMetodoPago
        [Theory]
        [InlineData(MetodoPagoSubastaTipos.Visa)]
        [InlineData(MetodoPagoSubastaTipos.GooglePay)]
        [InlineData(MetodoPagoSubastaTipos.PayPal)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_Success_test(MetodoPagoSubastaTipos metodoPago)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Subasta válida con dos libros (pujas 20 y 30) y descripción
            var subastaDTO = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 20),
                new SubastaItemDTO(2, _libro2Titulo, _editorial, _libro2Autor, 30) },
                _descripcion, metodoPago);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            //comprobamos que la respuesta es 201 y obtenemos el DTO devuelto
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            // DTO esperado (la fecha la pone el controlador con DateTime.Now, por eso se copia del actual)
            var expectedSubastaDetailDTO = new SubastaDetailDTO(1, "Pedro", "Jackson Díaz", "612345678",
                metodoPago, actualSubastaDetailDTO.FechaSubasta, _descripcion,
                new List<SubastaItemDTO>()
                {
                    new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 20),
                    new SubastaItemDTO(2, _libro2Titulo, _editorial, _libro2Autor, 30),
                });

            //comprobamos que el esperado y el actual son iguales
            Assert.Equal(expectedSubastaDetailDTO, actualSubastaDetailDTO);
        }

        // Necesario para ejecutar la línea "_ => throw" del switch CrearMetodoPago
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
            var subastaDTO = NuevaSubasta(_userName, new List<SubastaItemDTO> {
                new SubastaItemDTO(1, _libro1Titulo, _editorial, _libro1Autor, 10) },
                null, (MetodoPagoSubastaTipos)99);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => controller.CreateSubasta(subastaDTO));
            //no se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }
    }
}