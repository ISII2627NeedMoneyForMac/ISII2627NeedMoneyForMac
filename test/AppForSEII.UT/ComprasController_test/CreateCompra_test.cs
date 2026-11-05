using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.CompraDTOs;

namespace AppForSEII.UT.ComprasController_test
{
    public class CreateCompra_test : AppForSEII4SqliteUT
    {
        public CreateCompra_test()
        {
            var generos = new List<Genero>() {
                new Genero(1, "Novela"),
                new Genero(2, "Ciencia ficción"),
            };

            var editorial = new Editorial(1, "Planeta");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: Fundación = 1, Cien años de soledad = 2, Dune = 3 (sin stock)
            var libros = new List<Libro>(){
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = generos[0], Editorial = editorial },
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = generos[1], Editorial = editorial },
            };

            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");

            //métodos de pago: GooglePay tendrá id = 1, Visa id = 2 y PayPal id = 3 (el orden de inserción importa)
            var metodoPago = new GooglePay("peter@uclm.es");
            var metodoVisa = new Visa("4111111111111111", new DateTime(2030, 1, 1));
            var metodoPayPal = new PayPal("612345678");

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.Add(metodoPago);
            _context.Add(metodoVisa);
            _context.Add(metodoPayPal);
            _context.SaveChanges();
        }

        private static CompraForCreateDTO NuevaCompra(string usuarioUserName, int metodoPagoId,
            List<CompraItemDTO> items, string? codigoDescuento = null)
        {
            // CompraForCreateDTO(usuarioUserName, nombre, apellidos, direccion, telefono, metodoPagoId, codigoDescuento, items)
            return new CompraForCreateDTO(usuarioUserName, "Peter", "Jackson", "Calle Mayor 1, Albacete",
                "612345678", metodoPagoId, codigoDescuento, items);
        }

        private static CompraItemDTO Item(int libroId, int cantidad)
        {
            return new CompraItemDTO { LibroId = libroId, Cantidad = cantidad };
        }

        public static IEnumerable<object[]> TestCasesFor_CreateCompra()
        {
            var compraSinLibros = NuevaCompra("peter@uclm.es", 1, new List<CompraItemDTO>());

            var compraLibrosRepetidos = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 1), Item(1, 2) });

            var compraUsuarioNoRegistrado = NuevaCompra("nadie@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 1) });

            var compraMetodoPagoInexistente = NuevaCompra("peter@uclm.es", 99,
                new List<CompraItemDTO> { Item(1, 1) });

            var compraLibroInexistente = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(99, 1) });

            var compraStockInsuficiente = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 500) });

            var compraLibroSinStock = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(3, 1) });

            var allTests = new List<object[]>
            {             //entrada de CreateCompra - error esperado
                new object[] { compraSinLibros, "Error! Debe seleccionar al menos un libro para comprar" },
                new object[] { compraLibrosRepetidos, "Error! Hay libros repetidos en la compra" },
                new object[] { compraUsuarioNoRegistrado, "Error! El usuario no está registrado" },
                new object[] { compraMetodoPagoInexistente, "Error! El método de pago no existe" },
                new object[] { compraLibroInexistente, "Error! El libro con id 99 no existe" },
                new object[] { compraStockInsuficiente, "Error! No hay stock suficiente de 'Fundación'" },
                new object[] { compraLibroSinStock, "Error! No hay stock suficiente de 'Dune'" },
            };

            return allTests;
        }


        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateCompra))]
        public async Task CreateCompra_Error_test(CompraForCreateDTO compraDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.CreateCompra(compraDTO);

            // Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual coinciden
            Assert.StartsWith(errorExpected, errorActual);

            //no se ha guardado ninguna compra
            Assert.Empty(_context.Compras);
        }


        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateCompra_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            var compraDTO = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 2), Item(2, 1) }, "DESC10");

            // Act
            var antes = DateTime.Now;
            var result = await controller.CreateCompra(compraDTO);
            var despues = DateTime.Now;

            // Assert
            //comprobamos que la respuesta es 201 y apunta al details de la compra creada
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetDetailsForCompras", createdResult.ActionName);
            var actualCompraDetailDTO = Assert.IsType<ComprasDetailDTO>(createdResult.Value);

            //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
            //y la reutilizamos en el DTO esperado
            Assert.InRange(actualCompraDetailDTO.FechaCompra, antes, despues);

            // ComprasDetailDTO(id, nombre, apellidos, direccion, telefono, fechaCompra, precioTotal, metodoPago, codigoDescuento, items)
            // CompraItemDTO(libroId, titulo, genero, editorial, precioCompra, cantidad)
            var expectedCompraDetailDTO = new ComprasDetailDTO(1, "Peter", "Jackson",
                "Calle Mayor 1, Albacete", "612345678",
                actualCompraDetailDTO.FechaCompra, 35.85m, "GooglePay", "DESC10",
                new List<CompraItemDTO>()
                {
                    new CompraItemDTO(1, "Fundación", "Ciencia ficción", "Planeta", 9.95m, 2),
                    new CompraItemDTO(2, "Cien años de soledad", "Novela", "Planeta", 15.95m, 1),
                });

            //comprobamos que el esperado y el actual son iguales
            Assert.Equal(expectedCompraDetailDTO, actualCompraDetailDTO);

            //además, comprobamos en la base de datos (con un contexto nuevo) que se ha guardado todo
            using var contextoNuevo = CreateContext();
            Assert.Equal(1, contextoNuevo.Compras.Count());
            Assert.Equal(2, contextoNuevo.CompraItems.Count());
            Assert.Equal(13, contextoNuevo.Libros.Single(l => l.Id == 1).Stock);
            Assert.Equal(19, contextoNuevo.Libros.Single(l => l.Id == 2).Stock);
            Assert.Equal("Calle Mayor 1, Albacete",
                contextoNuevo.ApplicationUsers.Single(u => u.UserName == "peter@uclm.es").Direccion);
        }

        // Cubre las tres ramas del ternario del método de pago al crear la compra
        [Theory]
        [InlineData("GooglePay")]
        [InlineData("Visa")]
        [InlineData("PayPal")]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateCompra_MetodoPago_test(string metodoPagoEsperado)
        {
            // Arrange
            //obtenemos el id real del método de pago según su tipo (no dependemos del orden de inserción)
            int metodoPagoId = metodoPagoEsperado switch
            {
                "Visa" => _context.MetodoPagos.OfType<Visa>().Single().Id,
                "PayPal" => _context.MetodoPagos.OfType<PayPal>().Single().Id,
                _ => _context.MetodoPagos.OfType<GooglePay>().Single().Id
            };

            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            var compraDTO = NuevaCompra("peter@uclm.es", metodoPagoId,
                new List<CompraItemDTO> { Item(1, 1) });

            // Act
            var result = await controller.CreateCompra(compraDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actual = Assert.IsType<ComprasDetailDTO>(createdResult.Value);
            Assert.Equal(metodoPagoEsperado, actual.MetodoPago);
        }
    }
}