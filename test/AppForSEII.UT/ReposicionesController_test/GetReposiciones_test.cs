using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ReposicionDTOs;

namespace AppForSEII.UT.ReposicionesController_test
{
    public class GetReposiciones_test : AppForSEII4SqliteUT
    {
        public GetReposiciones_test()
        {
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 0, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
            };

            // El controller separa Surname en Apellido1 y Apellido2 por el primer espacio
            ApplicationUser user = new ApplicationUser("1", "Daniel", "García López", "daniel@uclm.es");
            user.PhoneNumber = "600123456";

            var reposicion = new Reposicion
            {
                FechaReposicion = DateTime.Now,
                PrecioTotal = 50.0m, // 5 unidades x 10.0
                Comentario = "Reposición de libros agotados",
                Usuario = user,
                MetodoPago = new Visa("4111111111111111", DateTime.Today.AddYears(2)),
                ReposicionItems = new List<ReposicionItem>()
            };
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                Libro = libros[0],
                Reposicion = reposicion,
                CantidadReposicion = 5
            });

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(reposicion);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetReposicion_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;

            var controller = new ReposicionesController(_context, logger);

            // Act
            var result = await controller.GetDetailsForReposiciones(0);

            // Assert
            // comprobamos que la respuesta es NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetReposicion_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            var expectedReposicion = new ReposicionDetailDTO(1, DateTime.Now, "daniel@uclm.es", "Daniel",
                                "García", "López", "600123456", MetodoPagoTipos.Visa,
                                "Reposición de libros agotados",
                                new List<ReposicionItemDTO>());
            // ReposicionItemDTO(libroId, titulo, autor, precioReposicion, cantidadReposicion)
            expectedReposicion.ReposicionItems.Add(
                new ReposicionItemDTO(1, "El señor de los anillos", "Tolkien", 10.0m, 5));

            // Act
            var result = await controller.GetDetailsForReposiciones(1);

            // Assert
            // comprobamos que la respuesta es OK y obtenemos la reposición
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reposicionDTOActual = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            // comprobamos que la esperada y la obtenida son iguales
            Assert.Equal(expectedReposicion, reposicionDTOActual);
        }

        [Theory]
        [InlineData(MetodoPagoTipos.GooglePay)]
        [InlineData(MetodoPagoTipos.PayPal)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetReposicion_MetodoPago_test(MetodoPagoTipos tipo)
        {
            // Arrange: segunda reposición (Id = 2) con otro método de pago
            var user = _context.ApplicationUsers.First();
            var libro = _context.Libros.First();

            MetodoPago metodoPago = tipo == MetodoPagoTipos.GooglePay
                ? new GooglePay()
                : new PayPal();

            var reposicion = new Reposicion
            {
                FechaReposicion = DateTime.Now,
                PrecioTotal = 10.0m,
                Comentario = "Reposición con otro método de pago",
                Usuario = user,
                MetodoPago = metodoPago,
                ReposicionItems = new List<ReposicionItem>()
            };
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                Libro = libro,
                Reposicion = reposicion,
                CantidadReposicion = 1
            });
            _context.Add(reposicion);
            _context.SaveChanges();

            var mock = new Mock<ILogger<ReposicionesController>>();
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            var result = await controller.GetDetailsForReposiciones(reposicion.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            Assert.Equal(tipo, dto.MetodoPago);
        }

        // NUEVO: cubre el caso de un único apellido (apellido2 = "")
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetReposicion_UnSoloApellido_test()
        {
            // Arrange: usuario con un único apellido
            var user = new ApplicationUser("3", "Pedro", "Martín", "pedro@uclm.es");
            user.PhoneNumber = "600999888";
            var libro = _context.Libros.First();

            var reposicion = new Reposicion
            {
                FechaReposicion = DateTime.Now,
                PrecioTotal = 10.0m,
                Comentario = "Reposición con un solo apellido",
                Usuario = user,
                MetodoPago = new PayPal(),
                ReposicionItems = new List<ReposicionItem>()
            };
            reposicion.ReposicionItems.Add(new ReposicionItem
            {
                Libro = libro,
                Reposicion = reposicion,
                CantidadReposicion = 1
            });
            _context.ApplicationUsers.Add(user);
            _context.Add(reposicion);
            _context.SaveChanges();

            var mock = new Mock<ILogger<ReposicionesController>>();
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            var result = await controller.GetDetailsForReposiciones(reposicion.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            Assert.Equal("Martín", dto.Apellido1);
            Assert.Equal("", dto.Apellido2);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetReposicion_TelefonoNull_test()
        {
            // Arrange
            var user = _context.ApplicationUsers.First();
            user.PhoneNumber = null;
            _context.SaveChanges();

            var mock = new Mock<ILogger<ReposicionesController>>();
            var controller = new ReposicionesController(_context, mock.Object);

            // Act
            var result = await controller.GetDetailsForReposiciones(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ReposicionDetailDTO>(okResult.Value);

            Assert.Equal("", dto.Telefono);
        }
    }
}