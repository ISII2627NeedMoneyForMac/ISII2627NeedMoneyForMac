using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ReposicionDTOs;

namespace AppForSEII.UT.ReposicionesController_test
{
    public class CreateReposiciones_test : AppForSEII4SqliteUT
    {
        private const string _userName = "laura@uclm.es";
        private const string _nombre = "Laura";
        private const string _apellido1 = "Sánchez";
        private const string _apellido2 = "Ruiz";
        private const string _telefono = "600123456";
        private const string _comentario = "Reposición de libros agotados";

        private const string _libro1Titulo = "El señor de los anillos";
        private const string _libro1Autor = "Tolkien";
        private const string _libro2Titulo = "La naranja mecánica";
        private const string _libro2Autor = "Burgess";

        public CreateReposiciones_test()
        {
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            var libros = new List<Libro>(){
                // libro 1: sin stock, se puede reponer
                new Libro(_libro1Titulo, _libro1Autor, new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                // libro 2: con stock
                new Libro(_libro2Titulo, _libro2Autor, new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
            };

            ApplicationUser user = new ApplicationUser("2", "Laura", "Sánchez Ruiz", _userName);

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesFor_CreateReposicion()
        {
            var items = () => new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) };

            // no se ha incluido ningún libro
            var reposicionSinItems = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario, new List<ReposicionItemDTO>());

            // falta un dato obligatorio (nombre vacío)
            var reposicionSinNombre = new ReposicionForCreateDTO(_userName, "", _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario, items());

            // el usuario no está registrado
            var reposicionUsuarioNoRegistrado = new ReposicionForCreateDTO("victor.lopez@uclm.es", _nombre,
                _apellido1, _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario, items());

            // el libro no existe
            var reposicionLibroInexistente = new ReposicionForCreateDTO(_userName, _nombre, _apellido1,
                _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>() { new ReposicionItemDTO(99, "Libro inventado", "Nadie", 10.0m, 5) });

            var allTests = new List<object[]>
            {             //input for createreposicion - Error expected
                new object[] { reposicionSinItems, "Error! Debes incluir al menos un libro para reponer", },
                new object[] { reposicionSinNombre, "Error! Faltan datos obligatorios por rellenar", },
                new object[] { reposicionUsuarioNoRegistrado, "Error! El nombre de usuario no está registrado", },
                new object[] { reposicionLibroInexistente, "Error! El libro con id 99 no existe", },
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateReposicion))]
        public async Task CreateReposicion_Error_test(ReposicionForCreateDTO reposicionDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;

            var controller = new ReposicionesController(_context, logger);

            // Act
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual son iguales
            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateReposicion_NoLibrosParaReponer_test()
        {
            // Arrange
            // todos los libros tienen stock > 0, así que no hay nada que reponer
            foreach (var libro in _context.Libros.ToList())
                libro.Stock = 5;
            _context.SaveChanges();

            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            var reposicionDTO = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>() { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            // Act
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.StartsWith("No hay libros disponibles para reponer", errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateReposicion_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;

            var controller = new ReposicionesController(_context, logger);

            var reposicionDTO = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            var expectedReposicionDetailDTO = new ReposicionDetailDTO(1, DateTime.Now,
                _userName, _nombre, _apellido1, _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            // Act
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            //comprobamos que la respuesta es Created y obtenemos el detalle devuelto
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualReposicionDetailDTO = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            Assert.Equal(expectedReposicionDetailDTO, actualReposicionDetailDTO);

            // la reposición se ha guardado y el stock del libro ha aumentado con la cantidad repuesta
            Assert.Equal(1, _context.Reposiciones.Count());
            Assert.Equal(5, _context.Libros.First(l => l.Id == 1).Stock);
        }

        // NUEVO: cubre las ramas GooglePay y PayPal del switch de metodoPago
        [Theory]
        [InlineData(MetodoPagoTipos.GooglePay, typeof(GooglePay))]
        [InlineData(MetodoPagoTipos.PayPal, typeof(PayPal))]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateReposicion_MetodoPago_test(MetodoPagoTipos tipo, Type tipoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;

            var controller = new ReposicionesController(_context, logger);

            var reposicionDTO = new ReposicionForCreateDTO(_userName, _nombre, _apellido1, _apellido2,
                _telefono, tipo, _comentario,
                new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            // Act
            var result = await controller.CreateReposicion(reposicionDTO);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var detail = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            // el DTO devuelto lleva el tipo de pago elegido
            Assert.Equal(tipo, detail.MetodoPago);

            // y en la base de datos se ha guardado la clase concreta correspondiente
            var guardada = _context.Reposiciones.Single();
            Assert.IsType(tipoEsperado, guardada.MetodoPago);
        }

    }
}