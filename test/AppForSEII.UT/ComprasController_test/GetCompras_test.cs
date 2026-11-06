// Importamos el espacio de nombres donde se encuentra ComprasController,
// que contiene el método GetDetailsForCompras() que vamos a probar.
using AppForSEII.API.Controllers;

// Importamos los DTO relacionados con las compras.
// Se utilizan para construir y comprobar los datos devueltos.
using AppForSEII.API.DTOs.CompraDTOs;

namespace AppForSEII.UT.ComprasController_test
{
    // Clase que contiene las pruebas unitarias del método
    // GetDetailsForCompras() del controlador ComprasController.
    //
    // Hereda de AppForSEII4SqliteUT, que proporciona el contexto SQLite
    // y las herramientas necesarias para ejecutar los tests.
    public class GetCompras_test : AppForSEII4SqliteUT
    {
        // Constructor que prepara los datos iniciales de la base de datos.
        // Se ejecuta antes de realizar las pruebas de esta clase.
        public GetCompras_test()
        {
            // Creamos una lista con dos géneros literarios.
            var generos = new List<Genero>() {
                // Género con Id 1: Novela.
                new Genero(1, "Novela"),

                // Género con Id 2: Ciencia ficción.
                new Genero(2, "Ciencia ficción"),
            };

            // Creamos una editorial con Id 1 y nombre Planeta.
            var editorial = new Editorial(1, "Planeta");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra,
            // precioReposicion, stock, tipoLibro)
            //
            // Creamos dos libros que utilizaremos en las compras.
            var libros = new List<Libro>(){
                // Libro Fundación:
                // Precio de compra: 9.95.
                // Stock: 15.
                // Género: Ciencia ficción.
                // Editorial: Planeta.
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },

                // Libro Cien años de soledad:
                // Precio de compra: 15.95.
                // Stock: 20.
                // Género: Novela.
                // Editorial: Planeta.
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura"),
            };

            // Creamos el usuario Elena Navarro Martínez.
            // Id: 1.
            // Nombre de usuario: elena@uclm.es.
            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");

            // Asignamos la dirección del usuario.
            user.Direccion = "Avda. España s/n, Albacete 02071";

            // Asignamos el teléfono del usuario.
            user.PhoneNumber = "600123456";

            //métodos de pago: GooglePay -> compra 1, Visa -> compra 2, PayPal -> compra 3

            // Creamos los tres métodos de pago que se utilizarán
            // en las tres compras de prueba.

            // Primer método de pago: GooglePay.
            // Asociado al correo electrónico de Elena.
            var metodoPago = new GooglePay("elena@uclm.es");

            // Segundo método de pago: Visa.
            // Número de tarjeta: 4111111111111111.
            // Fecha de caducidad: enero de 2030.
            var metodoVisa = new Visa("4111111111111111", new DateTime(2030, 1, 1));

            // Tercer método de pago: PayPal.
            // Asociado al teléfono indicado.
            var metodoPayPal = new PayPal("600123456");

            // PRIMERA COMPRA: GooglePay.
            // Creamos una compra utilizando un inicializador de objetos.
            var compra = new Compra {
                // Fecha de realización de la compra.
                FechaCompra = new DateTime(2026, 10, 15, 12, 0, 0),

                // Precio total: 19.90 €.
                PrecioTotal = 19.90m,

                // Usuario que realiza la compra.
                Usuario = user,

                // Método de pago utilizado: GooglePay.
                MetodoPago = metodoPago,

                // Código de descuento utilizado.
                CodigoDescuento = "DESC10",

                // Inicializamos la lista de elementos de la compra.
                CompraItems = new List<CompraItem>()
            };

            // Añadimos a la primera compra dos unidades del libro Fundación.
            // Cada unidad cuesta 9.95 €, por lo que el total es 19.90 €.
            compra.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compra, Cantidad = 2 });

            // SEGUNDA COMPRA: Visa.
            // Creamos una segunda compra asociada al mismo usuario.
            var compraVisa = new Compra {
                // Fecha de realización: 16 de octubre de 2026.
                FechaCompra = new DateTime(2026, 10, 16, 12, 0, 0),

                // Precio total: 9.95 €.
                PrecioTotal = 9.95m,

                // Mismo usuario: Elena.
                Usuario = user,

                // Método de pago utilizado: Visa.
                MetodoPago = metodoVisa,

                // No se utiliza ningún código de descuento.
                CodigoDescuento = null,

                // Inicializamos la lista de elementos de la compra.
                CompraItems = new List<CompraItem>()
            };

            // Añadimos una unidad del libro Fundación a la segunda compra.
            compraVisa.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compraVisa, Cantidad = 1 });

            // TERCERA COMPRA: PayPal.
            // Creamos una tercera compra asociada al mismo usuario.
            var compraPayPal = new Compra {
                // Fecha de realización: 17 de octubre de 2026.
                FechaCompra = new DateTime(2026, 10, 17, 12, 0, 0),

                // Precio total: 15.95 €.
                PrecioTotal = 15.95m,

                // Mismo usuario: Elena.
                Usuario = user,

                // Método de pago utilizado: PayPal.
                MetodoPago = metodoPayPal,

                // No se utiliza ningún código de descuento.
                CodigoDescuento = null,

                // Inicializamos la lista de elementos de la compra.
                CompraItems = new List<CompraItem>()
            };

            // Añadimos una unidad de Cien años de soledad a la tercera compra.
            compraPayPal.CompraItems.Add(new CompraItem { Libro = libros[1], Compra = compraPayPal, Cantidad = 1 });

            // Añadimos el usuario a la tabla ApplicationUsers.
            _context.ApplicationUsers.Add(user);

            // Añadimos los géneros a la base de datos.
            _context.AddRange(generos);

            // Añadimos la editorial.
            _context.Add(editorial);

            // Añadimos los dos libros.
            _context.AddRange(libros);

            // Añadimos el método de pago GooglePay.
            _context.Add(metodoPago);

            // Añadimos el método de pago Visa.
            _context.Add(metodoVisa);

            // Añadimos el método de pago PayPal.
            _context.Add(metodoPayPal);

            //el orden de inserción determina los ids: 1, 2 y 3

            // Añadimos las tres compras en orden.
            // Primera compra: GooglePay, Id esperado 1.
            _context.Add(compra);

            // Segunda compra: Visa, Id esperado 2.
            _context.Add(compraVisa);

            // Tercera compra: PayPal, Id esperado 3.
            _context.Add(compraPayPal);

            // Guardamos todos los datos preparados en la base de datos.
            _context.SaveChanges();
        }

        // [Fact] indica que es una prueba individual.
        // No recibe parámetros externos.
        [Fact]

        // Indica que el test se ejecuta sin un fixture compartido.
        [Trait("Database", "WithoutFixture")]

        // Clasifica el test como prueba unitaria.
        [Trait("LevelTesting", "Unit Testing")]

        // Comprueba qué sucede cuando intentamos consultar
        // una compra que no existe en la base de datos.
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // ARRANGE: preparamos los objetos necesarios.

            // Creamos un Mock del logger del controlador.
            // Sustituye al logger real porque no necesitamos
            // comprobar su funcionamiento.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el objeto simulado del logger.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador que queremos probar.
            // Le pasamos el contexto SQLite y el logger simulado.
            var controller = new ComprasController(_context, logger);

            // ACT: ejecutamos el método que queremos probar.

            // Buscamos la compra con Id 0.
            // No existe ninguna compra con ese identificador.
            // await espera a que termine la operación asíncrona.
            var result = await controller.GetDetailsForCompras(0);

            // ASSERT: comprobamos el resultado.

            // Comprobamos que el controlador devuelve NotFoundResult.
            // Esto representa una respuesta HTTP 404 Not Found.
            Assert.IsType<NotFoundResult>(result);
        }

        // [Fact] indica que es una prueba individual.
        [Fact]

        // Clasifica el test como prueba unitaria.
        [Trait("LevelTesting", "Unit Testing")]

        // Indica que se ejecuta sin fixture compartido.
        [Trait("Database", "WithoutFixture")]

        // Comprueba que el controlador recupera correctamente
        // todos los datos de una compra existente.
        public async Task GetDetailsForCompras_Found_test()
        {
            // ARRANGE: preparamos los objetos necesarios.

            // Creamos un Mock del logger.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el logger simulado.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador que vamos a probar.
            var controller = new ComprasController(_context, logger);

            // ComprasDetailDTO(
            // id, nombre, apellidos, direccion, telefono,
            // fechaCompra, precioTotal, metodoPago, codigoDescuento, items)
            //
            // Construimos el DTO que esperamos recibir al consultar
            // la primera compra, cuyo Id es 1.
            var expectedCompra = new ComprasDetailDTO(1, "Elena", "Navarro Martínez",
                "Avda. España s/n, Albacete 02071", "600123456",

                // Fecha de la primera compra.
                new DateTime(2026, 10, 15, 12, 0, 0),

                // Precio total.
                19.90m,

                // Método de pago.
                "GooglePay",

                // Código de descuento.
                "DESC10",

                // Inicializamos la lista de elementos de compra.
                new List<CompraItemDTO>());

            // CompraItemDTO(
            // libroId, titulo, genero, editorial, precioCompra, cantidad)
            //
            // Añadimos el libro Fundación al DTO esperado.
            // Id: 1.
            // Género: Ciencia ficción.
            // Editorial: Planeta.
            // Precio unitario: 9.95 €.
            // Cantidad: 2.
            expectedCompra.Items.Add(new CompraItemDTO(1, "Fundación", "Ciencia ficción", "Planeta", 9.95m, 2));

            // ACT: ejecutamos el método que queremos probar.

            // Solicitamos los detalles de la compra con Id 1.
            var result = await controller.GetDetailsForCompras(1);

            // ASSERT: comprobamos que la respuesta es correcta.

            // Comprobamos que el resultado sea OkObjectResult.
            // Representa una respuesta HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Comprobamos que el contenido de la respuesta sea
            // un ComprasDetailDTO y lo guardamos en compraDTOActual.
            var compraDTOActual = Assert.IsType<ComprasDetailDTO>(okResult.Value);

            //comprobamos que la esperada y la actual son iguales

            // Comparamos el DTO esperado con el DTO devuelto realmente.
            // Se comprueban todos los datos de la compra y sus elementos.
            Assert.Equal(expectedCompra, compraDTOActual);
        }

        // Cubre las tres ramas del ternario del método de pago al consultar el detalle

        // [Theory] indica que es un test parametrizado.
        // Se ejecutará tres veces utilizando los datos de InlineData.
        [Theory]

        // Primer conjunto de datos:
        // Compra con Id 1, cuyo método de pago esperado es GooglePay.
        [InlineData(1, "GooglePay")]

        // Segundo conjunto de datos:
        // Compra con Id 2, cuyo método de pago esperado es Visa.
        [InlineData(2, "Visa")]

        // Tercer conjunto de datos:
        // Compra con Id 3, cuyo método de pago esperado es PayPal.
        [InlineData(3, "PayPal")]

        // Clasifica la prueba como prueba unitaria.
        [Trait("LevelTesting", "Unit Testing")]

        // Indica que se ejecuta sin fixture compartido.
        [Trait("Database", "WithoutFixture")]

        // Test que comprueba que GetDetailsForCompras devuelve
        // correctamente el nombre del método de pago utilizado.
        //
        // Parámetros:
        // compraId: identificador de la compra que queremos consultar.
        // metodoPagoEsperado: nombre del método de pago que esperamos recibir.
        public async Task GetDetailsForCompras_MetodoPago_test(int compraId, string metodoPagoEsperado)
        {
            // ARRANGE: preparamos los objetos necesarios.

            // Creamos un Mock del logger del controlador.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el objeto simulado del logger.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador que vamos a probar.
            var controller = new ComprasController(_context, logger);

            // ACT: ejecutamos el método que queremos probar.

            // Consultamos la compra cuyo identificador recibimos
            // como parámetro desde InlineData.
            //
            // La primera ejecución consultará la compra 1.
            // La segunda ejecución consultará la compra 2.
            // La tercera ejecución consultará la compra 3.
            var result = await controller.GetDetailsForCompras(compraId);

            // ASSERT: comprobamos el resultado.

            // Comprobamos que la respuesta sea OkObjectResult (HTTP 200).
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Obtenemos el DTO que contiene los detalles de la compra.
            var dto = Assert.IsType<ComprasDetailDTO>(okResult.Value);

            // Comprobamos que el método de pago devuelto por el controlador
            // coincida con el esperado para cada compra.
            //
            // Ejecución 1: GooglePay == GooglePay.
            // Ejecución 2: Visa == Visa.
            // Ejecución 3: PayPal == PayPal.
            //
            // Así comprobamos las tres posibilidades del operador ternario
            // utilizado en GetDetailsForCompras().
            Assert.Equal(metodoPagoEsperado, dto.MetodoPago);
        }
    }
}