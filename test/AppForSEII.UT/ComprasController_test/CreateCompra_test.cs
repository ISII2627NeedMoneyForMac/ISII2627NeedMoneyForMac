using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.CompraDTOs;

namespace AppForSEII.UT.ComprasController_test
{
    // Clase de pruebas para el método CreateCompra del ComprasController.
    // Hereda de AppForSEII4SqliteUT, que proporciona una base de datos SQLite
    // preparada para realizar pruebas.
    public class CreateCompra_test : AppForSEII4SqliteUT
    {
        // Constructor: prepara los datos iniciales que utilizarán todos los tests.
        public CreateCompra_test()
        {
            // Creamos dos géneros literarios.
            var generos = new List<Genero>() {
                new Genero(1, "Novela"),
                new Genero(2, "Ciencia ficción"),
            };

            // Creamos una editorial que compartirán los tres libros.
            var editorial = new Editorial(1, "Planeta");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: Fundación = 1, Cien años de soledad = 2, Dune = 3 (sin stock)
            // Creamos tres libros con sus respectivos datos.
            var libros = new List<Libro>(){
                // Libro 1: Fundación, de Isaac Asimov.
                // Tiene 15 unidades disponibles y pertenece a Ciencia ficción.
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },

                // Libro 2: Cien años de soledad, de Gabriel García Márquez.
                // Tiene 20 unidades disponibles y pertenece a Novela.
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = generos[0], Editorial = editorial },

                // Libro 3: Dune, de Frank Herbert.
                // Su stock es 0, por lo que no se puede comprar.
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = generos[1], Editorial = editorial },
            };

            // Creamos el usuario que realizará las compras.
            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");

            //métodos de pago: GooglePay tendrá id = 1, Visa id = 2 y PayPal id = 3 (el orden de inserción importa)
            // Creamos tres métodos de pago diferentes para poder probarlos.
            var metodoPago = new GooglePay("peter@uclm.es");
            var metodoVisa = new Visa("4111111111111111", new DateTime(2030, 1, 1));
            var metodoPayPal = new PayPal("612345678");

            // Añadimos el usuario a la base de datos.
            _context.ApplicationUsers.Add(user);

            // Añadimos los géneros.
            _context.AddRange(generos);

            // Añadimos la editorial.
            _context.Add(editorial);

            // Añadimos los libros.
            _context.AddRange(libros);

            // Añadimos los tres métodos de pago.
            _context.Add(metodoPago);
            _context.Add(metodoVisa);
            _context.Add(metodoPayPal);

            // Guardamos todos los datos iniciales en la base de datos.
            _context.SaveChanges();
        }

        // Método auxiliar para crear una compra de prueba.
        // Recibe el nombre de usuario, el identificador del método de pago,
        // los libros que se quieren comprar y, opcionalmente, un código de descuento.
        private static CompraForCreateDTO NuevaCompra(string usuarioUserName, int metodoPagoId,
            List<CompraItemDTO> items, string? codigoDescuento = null)
        {
            // CompraForCreateDTO(usuarioUserName, nombre, apellidos, direccion, telefono, metodoPagoId, codigoDescuento, items)
            // Construimos y devolvemos el DTO con los datos de la compra.
            // Se utilizan los datos de Peter y los parámetros recibidos.
            return new CompraForCreateDTO(usuarioUserName, "Peter", "Jackson", "Calle Mayor 1, Albacete",
                "612345678", metodoPagoId, codigoDescuento, items);
        }

        // Método auxiliar para crear un elemento de compra.
        // Recibe el ID del libro y la cantidad que se desea comprar.
        private static CompraItemDTO Item(int libroId, int cantidad)
        {
            // Devolvemos un DTO que representa un libro dentro de la compra.
            return new CompraItemDTO { LibroId = libroId, Cantidad = cantidad };
        }

        // Proporciona los distintos casos de prueba para comprobar errores en CreateCompra.
        // Cada caso contiene:
        // 1. Un DTO de compra con unos datos determinados.
        // 2. El mensaje de error que esperamos recibir.
        public static IEnumerable<object[]> TestCasesFor_CreateCompra()
        {
            // Caso 1: se intenta realizar una compra sin seleccionar ningún libro.
            var compraSinLibros = NuevaCompra("peter@uclm.es", 1, new List<CompraItemDTO>());

            // Caso 2: se intenta añadir dos veces el mismo libro a una compra.
            var compraLibrosRepetidos = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 1), Item(1, 2) });

            // Caso 3: se intenta comprar utilizando un usuario que no está registrado.
            var compraUsuarioNoRegistrado = NuevaCompra("nadie@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 1) });

            // Caso 4: se intenta comprar utilizando un método de pago que no existe.
            // El ID 99 no corresponde a ninguno de los métodos de pago creados.
            var compraMetodoPagoInexistente = NuevaCompra("peter@uclm.es", 99,
                new List<CompraItemDTO> { Item(1, 1) });

            // Caso 5: se intenta comprar un libro que no existe.
            // El libro con ID 99 no está en la base de datos.
            var compraLibroInexistente = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(99, 1) });

            // Caso 6: se intenta comprar 500 unidades de Fundación,
            // pero solamente hay 15 disponibles.
            var compraStockInsuficiente = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 500) });

            // Caso 7: se intenta comprar Dune, que tiene stock igual a 0.
            var compraLibroSinStock = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(3, 1) });

            // Lista que almacena todos los casos de prueba y sus errores esperados.
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

            // Devolvemos los casos para que xUnit pueda ejecutar el test con cada uno.
            return allTests;
        }


        // Theory permite ejecutar el mismo test varias veces con datos diferentes.
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        // MemberData indica que los datos de entrada se obtienen del método TestCasesFor_CreateCompra.
        [MemberData(nameof(TestCasesFor_CreateCompra))]
        // Comprueba que CreateCompra rechaza correctamente las compras incorrectas.
        // Recibe el DTO de compra y el mensaje de error que esperamos.
        public async Task CreateCompra_Error_test(CompraForCreateDTO compraDTO, string errorExpected)
        {
            // Arrange: preparamos los objetos necesarios para ejecutar el test.

            // Creamos un mock del logger para no necesitar un sistema de registro real.
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador que vamos a probar, utilizando el contexto de pruebas.
            var controller = new ComprasController(_context, logger);

            // Act: ejecutamos el método que queremos probar.
            // Le pasamos el DTO que contiene los datos incorrectos.
            var result = await controller.CreateCompra(compraDTO);

            // Assert: comprobamos que el resultado es el esperado.
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            // BadRequestObjectResult representa una respuesta HTTP 400.
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Comprobamos que el contenido de la respuesta es un ValidationProblemDetails.
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Obtenemos el primer mensaje de error devuelto por el controlador.
            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual coinciden
            // Verificamos que el mensaje real comienza por el mensaje que esperábamos.
            Assert.StartsWith(errorExpected, errorActual);

            //no se ha guardado ninguna compra
            // Comprobamos que la compra incorrecta no se ha almacenado en la base de datos.
            Assert.Empty(_context.Compras);
        }


        // Fact ejecuta este test una única vez, ya que no necesita distintos datos de entrada.
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        // Comprueba que una compra con datos válidos se crea correctamente.
        public async Task CreateCompra_Success_test()
        {
            // Arrange: preparamos los datos para realizar una compra válida.

            // Creamos el mock del logger.
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador.
            var controller = new ComprasController(_context, logger);

            // Creamos una compra para Peter:
            // - 2 unidades de Fundación (ID 1).
            // - 1 unidad de Cien años de soledad (ID 2).
            // - Método de pago con ID 1 (GooglePay).
            // - Código de descuento DESC10.
            var compraDTO = NuevaCompra("peter@uclm.es", 1,
                new List<CompraItemDTO> { Item(1, 2), Item(2, 1) }, "DESC10");

            // Act: ejecutamos el método CreateCompra.
            // Guardamos el momento anterior y posterior para comprobar la fecha de compra.
            var antes = DateTime.Now;
            var result = await controller.CreateCompra(compraDTO);
            var despues = DateTime.Now;

            // Assert: comprobamos que la compra se ha creado correctamente.

            //comprobamos que la respuesta es 201 y apunta al details de la compra creada
            // CreatedAtActionResult representa una respuesta HTTP 201 Created.
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // Comprobamos que la acción a la que apunta la respuesta es GetDetailsForCompras.
            Assert.Equal("GetDetailsForCompras", createdResult.ActionName);

            // Comprobamos que el objeto devuelto es un DTO con los detalles de la compra.
            var actualCompraDetailDTO = Assert.IsType<ComprasDetailDTO>(createdResult.Value);

            //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
            //y la reutilizamos en el DTO esperado
            // La fecha debe encontrarse entre el instante anterior y el posterior a la llamada.
            Assert.InRange(actualCompraDetailDTO.FechaCompra, antes, despues);

            // ComprasDetailDTO(id, nombre, apellidos, direccion, telefono, fechaCompra, precioTotal, metodoPago, codigoDescuento, items)
            // CompraItemDTO(libroId, titulo, genero, editorial, precioCompra, cantidad)
            // Construimos el DTO que esperamos recibir.
            // Usamos la fecha real devuelta por el controlador para que la comparación sea exacta.
            var expectedCompraDetailDTO = new ComprasDetailDTO(1, "Peter", "Jackson",
                "Calle Mayor 1, Albacete", "612345678",
                actualCompraDetailDTO.FechaCompra, 35.85m, "GooglePay", "DESC10",
                new List<CompraItemDTO>()
                {
                    // Datos esperados del primer libro: 2 unidades de Fundación.
                    new CompraItemDTO(1, "Fundación", "Ciencia ficción", "Planeta", 9.95m, 2),

                    // Datos esperados del segundo libro: 1 unidad de Cien años de soledad.
                    new CompraItemDTO(2, "Cien años de soledad", "Novela", "Planeta", 15.95m, 1),
                });

            //comprobamos que el esperado y el actual son iguales
            // Comparamos el DTO esperado con el que realmente devuelve el controlador.
            Assert.Equal(expectedCompraDetailDTO, actualCompraDetailDTO);

            //además, comprobamos en la base de datos (con un contexto nuevo) que se ha guardado todo
            // Creamos un contexto nuevo para comprobar que los cambios se han guardado realmente.
            using var contextoNuevo = CreateContext();

            // Comprobamos que se ha guardado exactamente una compra.
            Assert.Equal(1, contextoNuevo.Compras.Count());

            // Comprobamos que se han guardado dos elementos dentro de la compra.
            Assert.Equal(2, contextoNuevo.CompraItems.Count());

            // Fundación tenía 15 unidades y se han comprado 2: quedan 13.
            Assert.Equal(13, contextoNuevo.Libros.Single(l => l.Id == 1).Stock);

            // Cien años de soledad tenía 20 unidades y se ha comprado 1: quedan 19.
            Assert.Equal(19, contextoNuevo.Libros.Single(l => l.Id == 2).Stock);

            // Comprobamos que también se ha actualizado la dirección del usuario.
            Assert.Equal("Calle Mayor 1, Albacete",
                contextoNuevo.ApplicationUsers.Single(u => u.UserName == "peter@uclm.es").Direccion);
        }

        // Cubre las tres ramas del ternario del método de pago al crear la compra
        // Theory permite repetir la prueba con los tres métodos de pago.
        [Theory]
        [InlineData("GooglePay")]
        [InlineData("Visa")]
        [InlineData("PayPal")]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        // Comprueba que el controlador identifica correctamente el método de pago utilizado.
        public async Task CreateCompra_MetodoPago_test(string metodoPagoEsperado)
        {
            // Arrange: preparamos la compra y seleccionamos el método de pago correspondiente.

            //obtenemos el id real del método de pago según su tipo (no dependemos del orden de inserción)
            // Buscamos el método de pago por su tipo concreto y obtenemos su ID.
            // Así evitamos depender de que GooglePay, Visa y PayPal tengan IDs determinados.
            int metodoPagoId = metodoPagoEsperado switch
            {
                // Si el método esperado es Visa, buscamos el objeto de tipo Visa.
                "Visa" => _context.MetodoPagos.OfType<Visa>().Single().Id,

                // Si es PayPal, buscamos el objeto de tipo PayPal.
                "PayPal" => _context.MetodoPagos.OfType<PayPal>().Single().Id,

                // Para cualquier otro valor, buscamos GooglePay.
                _ => _context.MetodoPagos.OfType<GooglePay>().Single().Id
            };

            // Creamos el mock del logger.
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador.
            var controller = new ComprasController(_context, logger);

            // Creamos una compra con una unidad de Fundación y el método de pago seleccionado.
            var compraDTO = NuevaCompra("peter@uclm.es", metodoPagoId,
                new List<CompraItemDTO> { Item(1, 1) });

            // Act: ejecutamos la creación de la compra.
            var result = await controller.CreateCompra(compraDTO);

            // Assert: comprobamos que la compra se ha creado correctamente.
            // Verificamos que el resultado es HTTP 201 Created.
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // Obtenemos el DTO devuelto por el controlador.
            var actual = Assert.IsType<ComprasDetailDTO>(createdResult.Value);

            // Comprobamos que el método de pago devuelto coincide con el que hemos seleccionado.
            Assert.Equal(metodoPagoEsperado, actual.MetodoPago);
        }
    }
}