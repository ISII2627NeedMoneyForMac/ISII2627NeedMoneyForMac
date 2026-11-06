// Controlador a probar
using AppForSEII.API.Controllers;
// DTOs de subasta
using AppForSEII.API.DTOs.SubastaDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    // Clase de tests; hereda de la base que da una BD SQLite en memoria nueva por test (_context)
    public class GetSubastas_test : AppForSEII4SqliteUT
    {
        // Fecha fija para todas las subastas de prueba (así se puede comparar exactamente)
        private static readonly DateTime _fecha = new DateTime(2026, 11, 3, 10, 0, 0);
        // Descripción de ejemplo
        private const string _descripcion = "Subasta de libros de ciencia ficción";

        // Constructor: se ejecuta antes de cada test y prepara los datos en la BD
        public GetSubastas_test()
        {
            // Género y editorial de ejemplo
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // Dos libros (ids 1 y 2)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
            };

            // Usuario de prueba
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
            // Le asignamos teléfono
            user.PhoneNumber = "600123456";

            // subasta 1: Visa y dos libros
            var subasta1 = new Subasta
            {
                // Fecha fija
                FechaSubasta = _fecha,
                PrecioSubasta = 50,   // 20 + 30
                // Usuario que la creó
                Usuario = user,
                // Método de pago Visa (número de tarjeta y fecha de caducidad)
                MetodoPago = new Visa("4111111111111111", DateTime.Today.AddYears(2))
            };
            // Item 1: libro 1 con puja 20
            subasta1.SubastaItems.Add(new SubastaItem
            { Libro = libros[0], Subasta = subasta1, PrecioPuja = 20, Descripcion = _descripcion });
            // Item 2: libro 2 con puja 30
            subasta1.SubastaItems.Add(new SubastaItem
            { Libro = libros[1], Subasta = subasta1, PrecioPuja = 30, Descripcion = _descripcion });

            // subasta 2: GooglePay, un libro y sin descripción
            var subasta2 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 15,
                Usuario = user,
                // Método de pago GooglePay (con un email)
                MetodoPago = new GooglePay("peter@uclm.es")
            };
            // Un único item, con Descripcion null
            subasta2.SubastaItems.Add(new SubastaItem
            { Libro = libros[0], Subasta = subasta2, PrecioPuja = 15, Descripcion = null });

            // subasta 3: PayPal y un libro
            var subasta3 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 25,
                Usuario = user,
                // Método de pago PayPal (con un teléfono)
                MetodoPago = new PayPal("600123456")
            };
            // Un único item: libro 2 con puja 25
            subasta3.SubastaItems.Add(new SubastaItem
            { Libro = libros[1], Subasta = subasta3, PrecioPuja = 25, Descripcion = _descripcion });

            //subasta 4: sin usuario y sin items (cubre los ?? string.Empty y el FirstOrDefault nulo)
            var subasta4 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 0,
                // Sin usuario
                Usuario = null,
                MetodoPago = new Visa("4111111111111111", DateTime.Today.AddYears(2))
            };

            // Marcamos todo para insertar en la BD
            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(subasta1);
            _context.Add(subasta2);
            _context.Add(subasta3);
            _context.Add(subasta4);
            // Guardamos (las subastas reciben los ids 1, 2, 3 y 4 en orden)
            _context.SaveChanges();
        }

        // Test: pedir una subasta que no existe
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_NotFound_test()
        {
            // Arrange
            // Logger falso (Moq)
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            // Controlador a probar
            var controller = new SubastasController(_context, logger);

            // Act
            // Pedimos la subasta con id 0 (no existe)
            var result = await controller.GetDetailsForSubastas(0);

            //Assert
            //comprobamos que la respuesta es NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        // Test: pedir una subasta que existe (la 1) y comprobar todo su contenido
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // SubastaDetailDTO(id, nombre, apellidos, telefono, metodoPago, fechaSubasta, descripcion, items)
            // SubastaItemDTO(libroId, titulo, editorial, autor, precioPuja)
            // DTO que esperamos recibir para la subasta 1
            var expectedSubasta = new SubastaDetailDTO(1, "Peter", "Jackson", "600123456",
                MetodoPagoSubastaTipos.Visa, _fecha, _descripcion,
                new List<SubastaItemDTO>()
                {
                    new SubastaItemDTO(1, "El señor de los anillos", "Minotauro", "Tolkien", 20),
                    new SubastaItemDTO(2, "La naranja mecánica", "Minotauro", "Burgess", 30),
                });

            // Act
            // Pedimos la subasta 1
            var result = await controller.GetDetailsForSubastas(1);

            //Assert
            //comprobamos que la respuesta es OK y obtenemos la subasta
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            //no se garantiza el orden de los items al leer de la BD: los ordenamos por libro
            subastaDTOActual.SubastaItems = subastaDTOActual.SubastaItems.OrderBy(i => i.LibroId).ToList();
            //comprobamos que la esperada y la obtenida son iguales
            Assert.Equal(expectedSubasta, subastaDTOActual);
            // El precio total (20 + 30) es 50
            Assert.Equal(50m, subastaDTOActual.TotalPrice);
        }

        // Test parametrizado: una ejecución por cada pareja (id de subasta, método de pago esperado)
        [Theory]
        [InlineData(1, MetodoPagoSubastaTipos.Visa)]
        [InlineData(2, MetodoPagoSubastaTipos.GooglePay)]
        [InlineData(3, MetodoPagoSubastaTipos.PayPal)]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_MetodoPago_test(int subastaId, MetodoPagoSubastaTipos metodoPagoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForSubastas(subastaId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            // El método de pago devuelto es el esperado para esa subasta
            Assert.Equal(metodoPagoEsperado, subastaDTOActual.MetodoPago);
        }

        // Test: subasta sin descripción
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_SinDescripcion_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act
            // la subasta 2 se ha creado sin descripción (es opcional)
            var result = await controller.GetDetailsForSubastas(2);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            // La descripción es null
            Assert.Null(subastaDTOActual.Descripcion);
            // El precio total es 15
            Assert.Equal(15m, subastaDTOActual.TotalPrice);
        }

        // Test: subasta sin usuario y sin items (casos límite del controlador: ?? string.Empty y FirstOrDefault)
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_SinUsuariNiTiems_test()
        {
            //Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //Act
            // la subasta 4 se ha creado sin usuario y sin items
            var result = await controller.GetDetailsForSubastas(4);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            // Sin usuario, nombre, apellidos y teléfono deben ser cadena vacía (no null)
            Assert.Equal(string.Empty, subastaDTOActual.Nombre);
            Assert.Equal(string.Empty, subastaDTOActual.Apellidos);
            Assert.Equal(string.Empty, subastaDTOActual.Telefono);
            // Sin items, no hay descripción
            Assert.Null(subastaDTOActual.Descripcion);
            // La lista de items está vacía
            Assert.Empty(subastaDTOActual.SubastaItems);
            // El precio total es 0
            Assert.Equal(0m, subastaDTOActual.TotalPrice);
        }
    }
}