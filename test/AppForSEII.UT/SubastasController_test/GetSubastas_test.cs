using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.SubastaDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    public class GetSubastas_test : AppForSEII4SqliteUT
    {
        private static readonly DateTime _fecha = new DateTime(2026, 11, 3, 10, 0, 0);
        private const string _descripcion = "Subasta de libros de ciencia ficción";

        public GetSubastas_test()
        {
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
            };

            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
            user.PhoneNumber = "600123456";

            // subasta 1: Visa y dos libros
            var subasta1 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 50,   // 20 + 30
                Usuario = user,
                MetodoPago = new Visa("4111111111111111", DateTime.Today.AddYears(2))
            };
            subasta1.SubastaItems.Add(new SubastaItem
            { Libro = libros[0], Subasta = subasta1, PrecioPuja = 20, Descripcion = _descripcion });
            subasta1.SubastaItems.Add(new SubastaItem
            { Libro = libros[1], Subasta = subasta1, PrecioPuja = 30, Descripcion = _descripcion });

            // subasta 2: GooglePay, un libro y sin descripción
            var subasta2 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 15,
                Usuario = user,
                MetodoPago = new GooglePay("peter@uclm.es")
            };
            subasta2.SubastaItems.Add(new SubastaItem
            { Libro = libros[0], Subasta = subasta2, PrecioPuja = 15, Descripcion = null });

            // subasta 3: PayPal y un libro
            var subasta3 = new Subasta
            {
                FechaSubasta = _fecha,
                PrecioSubasta = 25,
                Usuario = user,
                MetodoPago = new PayPal("600123456")
            };
            subasta3.SubastaItems.Add(new SubastaItem
            { Libro = libros[1], Subasta = subasta3, PrecioPuja = 25, Descripcion = _descripcion });

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(subasta1);
            _context.Add(subasta2);
            _context.Add(subasta3);
            _context.SaveChanges();
        }

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
            var result = await controller.GetDetailsForSubastas(0);

            //Assert
            //comprobamos que la respuesta es NotFound
            Assert.IsType<NotFoundResult>(result);
        }

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
            //comprobamos que la esperada y la obtenida son iguales
            Assert.Equal(expectedSubasta, subastaDTOActual);
            Assert.Equal(50m, subastaDTOActual.TotalPrice);
        }

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
            Assert.Equal(metodoPagoEsperado, subastaDTOActual.MetodoPago);
        }

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
            Assert.Null(subastaDTOActual.Descripcion);
            Assert.Equal(15m, subastaDTOActual.TotalPrice);
        }
    }
}