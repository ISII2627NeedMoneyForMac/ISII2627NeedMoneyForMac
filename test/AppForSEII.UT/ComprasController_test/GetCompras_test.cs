// Importa el controlador que vamos a probar (ComprasController)
using AppForSEII.API.Controllers;
// Importa los DTOs de compras (ComprasDetailDTO, CompraItemDTO)
using AppForSEII.API.DTOs.CompraDTOs;

// Espacio de nombres de los tests del controlador de compras
namespace AppForSEII.UT.ComprasController_test
{
    // Clase de tests; hereda de la clase base que prepara una base de datos SQLite en memoria (_context)
    public class GetCompras_test : AppForSEII4SqliteUT
    {
        // Constructor: xUnit crea una instancia nueva de la clase por cada test, así que este seed se ejecuta antes de cada uno
        public GetCompras_test()
        {
            // Lista de géneros de prueba
            var generos = new List<Genero>() {
                // Género con id 1
                new Genero(1, "Novela"),
                // Género con id 2
                new Genero(2, "Ciencia ficción"),
            };

            // Editorial de prueba con id 1
            var editorial = new Editorial(1, "Planeta");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // Lista de libros de prueba
            var libros = new List<Libro>(){
                // Primer libro: Fundación (se inserta primero, así que tendrá Id 1)
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    // Asigna sus navegaciones: género de ciencia ficción y la editorial
                    { Genero = generos[1], Editorial = editorial },
                // Segundo libro: Cien años de soledad (Id 2)
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    // Asigna sus navegaciones: género novela y la misma editorial
                    { Genero = generos[0], Editorial = editorial },
            };

            // Usuario de prueba: los argumentos son (id, nombre, apellidos, email)
            ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
            // Dirección del usuario
            user.Direccion = "Avda. España s/n, Albacete 02071";
            // Teléfono del usuario
            user.PhoneNumber = "600123456";

            //métodos de pago: GooglePay -> compra 1, Visa -> compra 2, PayPal -> compra 3
            // Método de pago GooglePay, identificado por el email
            var metodoPago = new GooglePay("elena@uclm.es");
            // Método de pago Visa, con número de tarjeta y fecha de caducidad
            var metodoVisa = new Visa("4111111111111111", new DateTime(2030, 1, 1));
            // Método de pago PayPal, identificado por el teléfono
            var metodoPayPal = new PayPal("600123456");

            // Primera compra (pagada con GooglePay)
            var compra = new Compra {
                // Fecha de la compra
                FechaCompra = new DateTime(2026, 10, 15, 12, 0, 0),
                // Precio total guardado (2 × 9,95 = 19,90)
                PrecioTotal = 19.90m,
                // Usuario que compra (navegación)
                Usuario = user,
                // Método de pago usado (navegación)
                MetodoPago = metodoPago,
                // Código de descuento aplicado
                CodigoDescuento = "DESC10",
                // Lista de líneas vacía, que se rellena justo debajo
                CompraItems = new List<CompraItem>()
            };
            // Añade una línea: 2 unidades de Fundación, enlazada a esta compra
            compra.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compra, Cantidad = 2 });

            // Segunda compra (pagada con Visa)
            var compraVisa = new Compra {
                // Fecha de la compra (un día después)
                FechaCompra = new DateTime(2026, 10, 16, 12, 0, 0),
                // Precio total (1 × 9,95)
                PrecioTotal = 9.95m,
                // Mismo usuario
                Usuario = user,
                // Método de pago Visa
                MetodoPago = metodoVisa,
                // Sin código de descuento
                CodigoDescuento = null,
                // Lista de líneas vacía
                CompraItems = new List<CompraItem>()
            };
            // Añade una línea: 1 unidad de Fundación
            compraVisa.CompraItems.Add(new CompraItem { Libro = libros[0], Compra = compraVisa, Cantidad = 1 });

            // Tercera compra (pagada con PayPal)
            var compraPayPal = new Compra {
                // Fecha de la compra (otro día después)
                FechaCompra = new DateTime(2026, 10, 17, 12, 0, 0),
                // Precio total (1 × 15,95)
                PrecioTotal = 15.95m,
                // Mismo usuario
                Usuario = user,
                // Método de pago PayPal
                MetodoPago = metodoPayPal,
                // Sin código de descuento
                CodigoDescuento = null,
                // Lista de líneas vacía
                CompraItems = new List<CompraItem>()
            };
            // Añade una línea: 1 unidad de Cien años de soledad
            compraPayPal.CompraItems.Add(new CompraItem { Libro = libros[1], Compra = compraPayPal, Cantidad = 1 });

            // Marca el usuario para insertarlo en la BD
            _context.ApplicationUsers.Add(user);
            // Marca los géneros para insertarlos
            _context.AddRange(generos);
            // Marca la editorial para insertarla
            _context.Add(editorial);
            // Marca los libros para insertarlos
            _context.AddRange(libros);
            // Marca el método de pago GooglePay
            _context.Add(metodoPago);
            // Marca el método de pago Visa
            _context.Add(metodoVisa);
            // Marca el método de pago PayPal
            _context.Add(metodoPayPal);
            //el orden de inserción determina los ids: 1, 2 y 3
            // Marca la compra 1 (sus líneas se insertan por cascada)
            _context.Add(compra);
            // Marca la compra 2
            _context.Add(compraVisa);
            // Marca la compra 3
            _context.Add(compraPayPal);
            // Ejecuta todos los INSERT en la BD; aquí es donde salta el FOREIGN KEY constraint failed (línea 194)
            _context.SaveChanges();
        }

        // Test unitario simple (sin parámetros)
        [Fact]
        // Etiqueta para filtrar tests: no usa fixture de base de datos
        [Trait("Database", "WithoutFixture")]
        // Etiqueta para filtrar tests: nivel unitario
        [Trait("LevelTesting", "Unit Testing")]
        // Comprueba que pedir una compra que no existe devuelve 404
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // Arrange
            // Crea un mock (simulacro) del logger, porque el test no necesita logs reales
            var mock = new Mock<ILogger<ComprasController>>();
            // Obtiene el objeto logger falso a partir del mock
            ILogger<ComprasController> logger = mock.Object;
            // Crea el controlador con el contexto de SQLite y el logger falso
            var controller = new ComprasController(_context, logger);

            // Act
            // Llama al método con el id 0, que no existe en la BD
            var result = await controller.GetDetailsForCompras(0);

            // Assert
            //comprobamos que la respuesta es de tipo NotFound
            // Falla si el resultado no es exactamente un NotFoundResult
            Assert.IsType<NotFoundResult>(result);
        }

        // Test unitario simple
        [Fact]
        // Etiqueta de nivel de testing
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta de base de datos
        [Trait("Database", "WithoutFixture")]
        // Comprueba que una compra existente devuelve 200 con el detalle correcto
        public async Task GetDetailsForCompras_Found_test()
        {
            // Arrange
            // Mock del logger
            var mock = new Mock<ILogger<ComprasController>>();
            // Objeto logger falso
            ILogger<ComprasController> logger = mock.Object;
            // Controlador bajo prueba
            var controller = new ComprasController(_context, logger);

            // ComprasDetailDTO(id, nombre, apellidos, direccion, telefono, fechaCompra, precioTotal, metodoPago, codigoDescuento, items)
            // Construye el DTO que esperamos recibir para la compra 1, con la lista de items vacía por ahora
            var expectedCompra = new ComprasDetailDTO(1, "Elena", "Navarro Martínez",
                "Avda. España s/n, Albacete 02071", "600123456",
                new DateTime(2026, 10, 15, 12, 0, 0), 19.90m, "GooglePay", "DESC10",
                new List<CompraItemDTO>());
            // CompraItemDTO(libroId, titulo, genero, editorial, precioCompra, cantidad)
            // Añade la línea esperada: 2 unidades de Fundación (libro 1)
            expectedCompra.Items.Add(new CompraItemDTO(1, "Fundación", "Ciencia ficción", "Planeta", 9.95m, 2));

            // Act
            // Pide al controlador el detalle de la compra con id 1
            var result = await controller.GetDetailsForCompras(1);

            // Assert
            //comprobamos que la respuesta es de tipo OK y obtenemos la compra
            // Verifica que es un 200 OK y lo guarda para inspeccionarlo
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el contenido es un ComprasDetailDTO y lo guarda
            var compraDTOActual = Assert.IsType<ComprasDetailDTO>(okResult.Value);
            //comprobamos que la esperada y la actual son iguales
            // Compara el DTO esperado con el real (usa la igualdad del tipo DTO)
            Assert.Equal(expectedCompra, compraDTOActual);
        }

        // Cubre las tres ramas del ternario del método de pago al consultar el detalle
        // Test con parámetros: se ejecuta una vez por cada InlineData
        [Theory]
        // Compra 1 debe devolver "GooglePay"
        [InlineData(1, "GooglePay")]
        // Compra 2 debe devolver "Visa"
        [InlineData(2, "Visa")]
        // Compra 3 debe devolver "PayPal"
        [InlineData(3, "PayPal")]
        // Etiqueta de nivel de testing
        [Trait("LevelTesting", "Unit Testing")]
        // Etiqueta de base de datos
        [Trait("Database", "WithoutFixture")]
        // Recibe el id de la compra y el texto del método de pago que se espera
        public async Task GetDetailsForCompras_MetodoPago_test(int compraId, string metodoPagoEsperado)
        {
            // Arrange
            // Mock del logger
            var mock = new Mock<ILogger<ComprasController>>();
            // Objeto logger falso
            ILogger<ComprasController> logger = mock.Object;
            // Controlador bajo prueba
            var controller = new ComprasController(_context, logger);

            // Act
            // Pide el detalle de la compra indicada por el parámetro
            var result = await controller.GetDetailsForCompras(compraId);

            // Assert
            // Verifica que la respuesta es 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verifica que el cuerpo es un ComprasDetailDTO
            var dto = Assert.IsType<ComprasDetailDTO>(okResult.Value);
            // Comprueba que el método de pago del DTO coincide con el esperado para ese id
            Assert.Equal(metodoPagoEsperado, dto.MetodoPago);
        }
    }
}