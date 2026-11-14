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
        private const string _userName = "laura@uclm.es";
        private const string _nombre = "Laura";
        private const string _apellido1 = "Sánchez";
        private const string _apellido2 = "Ruiz";
        private const string _telefono = "+600123456";
        private const string _comentario = "Reposición de libros agotados";

        private const string _libro1Titulo = "El señor de los anillos";
        private const string _libro1Autor = "Tolkien";
        private const string _libro2Titulo = "La naranja mecánica";
        private const string _libro2Autor = "Burgess";

        // Constructor: se ejecuta ANTES de cada test y deja la BD con datos iniciales
        public CreateReposiciones_test()
        {
            // Crea un género de prueba (id 1)
            var genero = new Genero(1, "Ciencia ficción");
            // Crea una editorial de prueba (id 1)
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

            // Crea el usuario de prueba (id "2", nombre, apellidos juntos y nombre de usuario)
            ApplicationUser user = new ApplicationUser("2", "Laura", "Sánchez Ruiz", _userName);

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
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

        // Proporciona los casos de prueba (datos de entrada + error esperado) al test con [MemberData]
        public static IEnumerable<object[]> TestCasesFor_CreateReposicion()
        {
            // Función que devuelve una lista NUEVA de items cada vez que se llama
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

            // DTO válido con un item del libro 1 (el error vendrá solo de que no hay libros con stock 0)
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

            // Reposición válida: 5 unidades del libro 1 (que tiene stock 0)
            var reposicionDTO = NuevaReposicion(_userName,
                new List<ReposicionItemDTO> { Item(1, 5) });

            // Act
            // Rango de tiempo en el que debe estar la fecha de la reposición
            var antes = DateTime.Now;
            var result = await controller.CreateReposicion(reposicionDTO);
            var despues = DateTime.Now;

            // Assert
            //comprobamos que la respuesta es 201 y obtenemos el DTO devuelto
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualReposicionDetailDTO = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
            Assert.InRange(actualReposicionDetailDTO.FechaReposicion, antes, despues);

            // Construimos el DTO esperado (la fecha se copia del actual porque no se puede predecir exacta)
            var expectedReposicionDetailDTO = new ReposicionDetailDTO(1, actualReposicionDetailDTO.FechaReposicion,
                _userName, _nombre, _apellido1, _apellido2, _telefono, MetodoPagoTipos.Visa, _comentario,
                new List<ReposicionItemDTO>()
                { new ReposicionItemDTO(1, _libro1Titulo, _libro1Autor, 10.0m, 5) });

            //comprobamos que el esperado y el actual son iguales
            Assert.Equal(expectedReposicionDetailDTO, actualReposicionDetailDTO);
        }

    }
}