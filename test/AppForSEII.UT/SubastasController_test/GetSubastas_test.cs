using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.SubastaDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    public class GetSubastas_test : AppForSEII4SqliteUT
    {
        private const string Descripcion = "Subasta de libros de ciencia ficción y novela";

        public GetSubastas_test()
        {
            var generos = new List<Genero>() {
                new Genero(1, "Novela"),
                new Genero(2, "Ciencia ficción"),
            };

            var editorial = new Editorial(1, "Planeta");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            var libros = new List<Libro>(){
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = generos[0], Editorial = editorial },
            };

            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
            user.PhoneNumber = "600123456";

            // Subasta 1: GooglePay con dos libros
            var subastaGooglePay = new Subasta {
                FechaSubasta = new DateTime(2026, 10, 15, 12, 0, 0),
                PrecioSubasta = 30,
                Usuario = user,
                MetodoPago = new GooglePay("elena@uclm.es")
            };
            subastaGooglePay.SubastaItems.Add(new SubastaItem { Libro = libros[0], Subasta = subastaGooglePay, PrecioPuja = 10, Descripcion = Descripcion });
            subastaGooglePay.SubastaItems.Add(new SubastaItem { Libro = libros[1], Subasta = subastaGooglePay, PrecioPuja = 20, Descripcion = Descripcion });

            // Subasta 2: Visa
            var subastaVisa = new Subasta {
                FechaSubasta = new DateTime(2026, 10, 16, 12, 0, 0),
                PrecioSubasta = 10,
                Usuario = user,
                MetodoPago = new Visa("4000000000000002", new DateTime(2029, 1, 28))
            };
            subastaVisa.SubastaItems.Add(new SubastaItem { Libro = libros[0], Subasta = subastaVisa, PrecioPuja = 10, Descripcion = Descripcion });

            // Subasta 3: PayPal
            var subastaPayPal = new Subasta {
                FechaSubasta = new DateTime(2026, 10, 17, 12, 0, 0),
                PrecioSubasta = 20,
                Usuario = user,
                MetodoPago = new PayPal("600800001")
            };
            subastaPayPal.SubastaItems.Add(new SubastaItem { Libro = libros[1], Subasta = subastaPayPal, PrecioPuja = 20, Descripcion = Descripcion });

            // Subasta 4: sin usuario, sin método de pago y sin items
            var subastaVacia = new Subasta {
                FechaSubasta = new DateTime(2026, 10, 18, 12, 0, 0),
                PrecioSubasta = 0
            };

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.AddRange(subastaGooglePay, subastaVisa, subastaPayPal, subastaVacia);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForSubastas_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForSubastas(0);

            // Assert
            //comprobamos que la respuesta es de tipo NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // SubastaItemDTO(libroId, titulo, editorial, autor, precioPuja)
            // SubastaDetailDTO(id, nombre, apellidos, telefono, metodoPago, fechaSubasta, descripcion, items)
            var expectedSubasta = new SubastaDetailDTO(1, "Elena", "Navarro Martínez", "600123456",
                MetodoPagoSubastaTipos.GooglePay, new DateTime(2026, 10, 15, 12, 0, 0), Descripcion,
                new List<SubastaItemDTO>() {
                    new SubastaItemDTO(1, "Fundación", "Planeta", "Isaac Asimov", 10),
                    new SubastaItemDTO(2, "Cien años de soledad", "Planeta", "Gabriel García Márquez", 20),
                });

            // Act
            var result = await controller.GetDetailsForSubastas(1);

            // Assert
            //comprobamos que la respuesta es de tipo OK y obtenemos la subasta
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            //comprobamos que la esperada y la actual son iguales
            Assert.Equal(expectedSubasta, subastaDTOActual);
            //el precio total es la suma de las pujas
            Assert.Equal(30m, subastaDTOActual.TotalPrice);
        }

        [Theory]
        [InlineData(1, MetodoPagoSubastaTipos.GooglePay)]
        [InlineData(2, MetodoPagoSubastaTipos.Visa)]
        [InlineData(3, MetodoPagoSubastaTipos.PayPal)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_MetodoPago_test(int subastaId, MetodoPagoSubastaTipos metodoPagoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            var controller = new SubastasController(_context, mock.Object);

            // Act
            var result = await controller.GetDetailsForSubastas(subastaId);

            // Assert
            //el tipo de método de pago de la entidad se traduce al enum del DTO
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            Assert.Equal(metodoPagoEsperado, subastaDTOActual.MetodoPago);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_SinUsuarioNiItems_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            var controller = new SubastasController(_context, mock.Object);

            //sin usuario los datos personales son cadenas vacías, sin método de pago se devuelve Visa
            //y sin items la descripción es null
            var expectedSubasta = new SubastaDetailDTO(4, "", "", "", MetodoPagoSubastaTipos.Visa,
                new DateTime(2026, 10, 18, 12, 0, 0), null, new List<SubastaItemDTO>());

            // Act
            var result = await controller.GetDetailsForSubastas(4);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var subastaDTOActual = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            Assert.Equal(expectedSubasta, subastaDTOActual);
        }
    }
}