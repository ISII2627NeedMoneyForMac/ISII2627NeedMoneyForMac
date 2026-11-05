using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.CompraDTOs;

namespace AppForSEII.UT.ComprasController_test
{
    public class GetCompras_test : AppForSEII4SqliteUT
    {
        public GetCompras_test()
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
            user.Direccion = "Avda. España s/n, Albacete 02071";
            user.PhoneNumber = "600123456";

            //métodos de pago: GooglePay -> compra 1, Visa -> compra 2, PayPal -> compra 3
            var metodoPago = new GooglePay("elena@uclm.es");
            var metodoVisa = new Visa("4111111111111111", new DateTime(2030, 1, 1));
            var metodoPayPal = new PayPal("600123456");

            var compra = new Compra {
                FechaCompra = new DateTime(2026, 10, 15, 12, 0, 0),
                PrecioTotal = 19.90m,
                Usuario = user,
                MetodoPago = metodoPago,
                CodigoDescuento = "DESC10",
                CompraItems = new List<CompraItem>()
            };
            compra.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compra, Cantidad = 2 });

            var compraVisa = new Compra {
                FechaCompra = new DateTime(2026, 10, 16, 12, 0, 0),
                PrecioTotal = 9.95m,
                Usuario = user,
                MetodoPago = metodoVisa,
                CodigoDescuento = null,
                CompraItems = new List<CompraItem>()
            };
            compraVisa.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compraVisa, Cantidad = 1 });

            var compraPayPal = new Compra {
                FechaCompra = new DateTime(2026, 10, 17, 12, 0, 0),
                PrecioTotal = 15.95m,
                Usuario = user,
                MetodoPago = metodoPayPal,
                CodigoDescuento = null,
                CompraItems = new List<CompraItem>()
            };
            compraPayPal.CompraItems.Add(new CompraItem { Libro = libros[1], Compra = compraPayPal, Cantidad = 1 });

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(metodoPago);
            _context.Add(metodoVisa);
            _context.Add(metodoPayPal);
            //el orden de inserción determina los ids: 1, 2 y 3
            _context.Add(compra);
            _context.Add(compraVisa);
            _context.Add(compraPayPal);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForCompras(0);

            // Assert
            //comprobamos que la respuesta es de tipo NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForCompras_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // ComprasDetailDTO(id, nombre, apellidos, direccion, telefono, fechaCompra, precioTotal, metodoPago, codigoDescuento, items)
            var expectedCompra = new ComprasDetailDTO(1, "Elena", "Navarro Martínez",
                "Avda. España s/n, Albacete 02071", "600123456",
                new DateTime(2026, 10, 15, 12, 0, 0), 19.90m, "GooglePay", "DESC10",
                new List<CompraItemDTO>());
            // CompraItemDTO(libroId, titulo, genero, editorial, precioCompra, cantidad)
            expectedCompra.Items.Add(new CompraItemDTO(1, "Fundación", "Ciencia ficción", "Planeta", 9.95m, 2));

            // Act
            var result = await controller.GetDetailsForCompras(1);

            // Assert
            //comprobamos que la respuesta es de tipo OK y obtenemos la compra
            var okResult = Assert.IsType<OkObjectResult>(result);
            var compraDTOActual = Assert.IsType<ComprasDetailDTO>(okResult.Value);
            //comprobamos que la esperada y la actual son iguales
            Assert.Equal(expectedCompra, compraDTOActual);
        }

        // Cubre las tres ramas del ternario del método de pago al consultar el detalle
        [Theory]
        [InlineData(1, "GooglePay")]
        [InlineData(2, "Visa")]
        [InlineData(3, "PayPal")]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForCompras_MetodoPago_test(int compraId, string metodoPagoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForCompras(compraId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ComprasDetailDTO>(okResult.Value);
            Assert.Equal(metodoPagoEsperado, dto.MetodoPago);
        }
    }
}