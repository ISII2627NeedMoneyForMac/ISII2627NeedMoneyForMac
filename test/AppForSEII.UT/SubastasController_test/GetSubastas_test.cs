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

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro, genero, editorial)
            // Dos libros (ids 1 y 2)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura", genero, editorial),
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda", genero, editorial),
            };

            // Usuario de prueba
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
            // Le asignamos teléfono
            user.PhoneNumber = "600123456";

            // Subasta(fechaSubasta, usuario, metodoPago, subastaItems)
            // SubastaItem(libro, subasta, precioPuja, descripcion)

            // subasta 1: Visa y dos libros
            var subasta1 = new Subasta(_fecha, user,
                new Visa("4111111111111111", DateTime.Today.AddYears(2)), new List<SubastaItem>());
            subasta1.SubastaItems.Add(new SubastaItem(libros[0], subasta1, 20, _descripcion));
            subasta1.SubastaItems.Add(new SubastaItem(libros[1], subasta1, 30, _descripcion));

            // subasta 2: GooglePay y un libro
            var subasta2 = new Subasta(_fecha, user,
                new GooglePay("peter@uclm.es"), new List<SubastaItem>());
            subasta2.SubastaItems.Add(new SubastaItem(libros[0], subasta2, 15, _descripcion));

            // subasta 3: PayPal y un libro
            var subasta3 = new Subasta(_fecha, user,
                new PayPal("600123456"), new List<SubastaItem>());
            subasta3.SubastaItems.Add(new SubastaItem(libros[1], subasta3, 25, _descripcion));

            // Marcamos todo para insertar en la BD
            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(subasta1);
            _context.Add(subasta2);
            _context.Add(subasta3);
            // Guardamos (las subastas reciben los ids 1, 2 y 3 en orden)
            _context.SaveChanges();
        }

        // Test: pedir una subasta que no existe
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetSubasta_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
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
            var result = await controller.GetDetailsForSubastas(1);

            //Assert
            //comprobamos que la respuesta es OK y obtenemos la subasta
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            //no se garantiza el orden de los items al leer de la BD: los ordenamos por libro
            subastaDTOActual.SubastaItems = subastaDTOActual.SubastaItems.OrderBy(i => i.LibroId).ToList();
            //comprobamos que la esperada y la obtenida son iguales (Equals también compara el precio total)
            Assert.Equal(expectedSubasta, subastaDTOActual);
        }

        // Test parametrizado: necesario para ejecutar todas las líneas del switch ObtenerTipoMetodoPago
        // (una por cada método de pago)
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
    }
}