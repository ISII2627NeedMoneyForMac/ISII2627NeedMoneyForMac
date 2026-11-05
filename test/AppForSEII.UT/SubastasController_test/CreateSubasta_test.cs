using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.SubastaDTOs;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.UT.SubastasController_test
{
    public class CreateSubasta_test : AppForSEII4SqliteUT
    {
        private const string _descripcion = "Subasta de libros de ciencia ficción";

        public CreateSubasta_test()
        {
            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: El señor de los anillos = 1, La naranja mecánica = 2, Dune = 3 (sin stock)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 3, "Tapa blanda")
                    { Genero = genero, Editorial = editorial },
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.0m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editorial },
            };

            ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");

            _context.ApplicationUsers.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // SubastaForCreateDTO(userName, nombre, apellidos, telefono, metodoPago, descripcion, subastaItems)
        private static SubastaForCreateDTO NuevaSubasta(string usuarioUserName, List<SubastaItemDTO> items,
            string? descripcion = null, MetodoPagoSubastaTipos metodoPago = MetodoPagoSubastaTipos.GooglePay)
        {
            return new SubastaForCreateDTO(usuarioUserName, "Pedro", "Jackson Díaz", "612345678",
                metodoPago, descripcion, items);
        }

        //solo se usan LibroId y PrecioPuja: el resto de datos del libro los toma el controlador de la BD
        private static SubastaItemDTO Item(int libroId, double precioPuja)
        {
            return new SubastaItemDTO { LibroId = libroId, PrecioPuja = precioPuja };
        }

        public static IEnumerable<object[]> TestCasesFor_CreateSubasta()
        {
            var subastaSinLibros = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO>());

            var subastaLibrosRepetidos = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10), Item(1, 20) });

            var subastaPujaCero = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 0) });

            var subastaPujaNegativa = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, -5) });

            var subastaPujaMayorQueIntMax = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 2147483648d) });

            var subastaPujaDecimal = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10.5) });

            var subastaUsuarioNoRegistrado = NuevaSubasta("nadie@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10) });

            var subastaLibroInexistente = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(99, 10) });

            var subastaLibroSinStock = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(3, 10) });

            var allTests = new List<object[]>
            {             //entrada de CreateSubasta - error esperado
                new object[] { subastaSinLibros, "Error! Debes incluir al menos un libro en la subasta" },
                new object[] { subastaLibrosRepetidos, "Error! Hay libros repetidos en la subasta" },
                new object[] { subastaPujaCero, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaNegativa, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaMayorQueIntMax, "Error! El precio de la puja del libro '1' debe estar entre 1 y 2147483647" },
                new object[] { subastaPujaDecimal, "Error! El precio de la puja del libro '1' debe ser un número entero" },
                new object[] { subastaUsuarioNoRegistrado, "Error! UserName is not registered" },
                new object[] { subastaLibroInexistente, "Error! El libro con id '99' no existe" },
                new object[] { subastaLibroSinStock, "Error! El libro 'Dune' no está disponible para subasta" },
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateSubasta))]
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el mensaje de error esperado y el actual coinciden
            Assert.Equal(errorExpected, errorActual);

            //no se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_VariosErrores_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            //libros repetidos y usuario no registrado a la vez
            var subastaDTO = NuevaSubasta("nadie@uclm.es",
                new List<SubastaItemDTO> { Item(1, 10), Item(1, 20) });

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            //se devuelven todos los errores, no solo el primero
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            Assert.Contains("Error! Hay libros repetidos en la subasta", problemDetails.Errors["SubastaItems"]);
            Assert.Contains("Error! UserName is not registered", problemDetails.Errors["SubastaApplicationUser"]);
            Assert.Empty(_context.Subastas);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;

            var controller = new SubastasController(_context, logger);

            var subastaDTO = NuevaSubasta("peter@uclm.es",
                new List<SubastaItemDTO> { Item(1, 20), Item(2, 30) }, _descripcion);

            // Act
            var antes = DateTime.Now;
            var result = await controller.CreateSubasta(subastaDTO);
            var despues = DateTime.Now;

            // Assert
            //comprobamos que la respuesta es 201 y apunta al details de la subasta creada
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetDetailsForSubastas", createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            //la fecha la pone el controlador (DateTime.Now): comprobamos que es de ahora
            //y la reutilizamos en el DTO esperado
            Assert.InRange(actualSubastaDetailDTO.FechaSubasta, antes, despues);

            // SubastaDetailDTO(id, nombre, apellidos, telefono, metodoPago, fechaSubasta, descripcion, items)
            // SubastaItemDTO(libroId, titulo, editorial, autor, precioPuja)
            var expectedSubastaDetailDTO = new SubastaDetailDTO(1, "Pedro", "Jackson Díaz", "612345678",
                MetodoPagoSubastaTipos.GooglePay, actualSubastaDetailDTO.FechaSubasta, _descripcion,
                new List<SubastaItemDTO>()
                {
                    new SubastaItemDTO(1, "El señor de los anillos", "Minotauro", "Tolkien", 20),
                    new SubastaItemDTO(2, "La naranja mecánica", "Minotauro", "Burgess", 30),
                });

            //comprobamos que el esperado y el actual son iguales
            Assert.Equal(expectedSubastaDetailDTO, actualSubastaDetailDTO);
            Assert.Equal(50m, actualSubastaDetailDTO.TotalPrice);

            //además, comprobamos en la base de datos (con un contexto nuevo) que se ha guardado todo
            using var contextoNuevo = CreateContext();
            var subastaGuardada = contextoNuevo.Subastas
                .Include(s => s.SubastaItems)
                .Include(s => s.MetodoPago)
                .Include(s => s.Usuario)
                .Single();

            Assert.Equal(50d, subastaGuardada.PrecioSubasta);
            Assert.Equal(2, subastaGuardada.SubastaItems.Count);
            //no se garantiza el orden al leer de la BD: ordenamos por libro
            Assert.Equal(new[] { 20, 30 },
                subastaGuardada.SubastaItems.OrderBy(si => si.LibroId).Select(si => si.PrecioPuja).ToArray());
            Assert.All(subastaGuardada.SubastaItems, si => Assert.Equal(_descripcion, si.Descripcion));
            Assert.IsType<GooglePay>(subastaGuardada.MetodoPago);

            //los datos del formulario se guardan en el usuario
            Assert.Equal("Pedro", subastaGuardada.Usuario!.Name);
            Assert.Equal("Jackson Díaz", subastaGuardada.Usuario.Surname);
            Assert.Equal("612345678", subastaGuardada.Usuario.PhoneNumber);

            //una subasta no modifica el stock de los libros
            Assert.Equal(5, contextoNuevo.Libros.Single(l => l.Id == 1).Stock);
            Assert.Equal(3, contextoNuevo.Libros.Single(l => l.Id == 2).Stock);
        }

        [Theory]
        [InlineData(1d)]
        [InlineData(2147483647d)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_PujaLimite_Success_test(double puja)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //los valores límite permitidos: 1 e int.MaxValue
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(1, puja) });

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);
            Assert.Equal((decimal)puja, actualSubastaDetailDTO.TotalPrice);

            using var contextoNuevo = CreateContext();
            Assert.Equal(puja, contextoNuevo.Subastas.Single().PrecioSubasta);
        }

        [Theory]
        [InlineData(MetodoPagoSubastaTipos.Visa)]
        [InlineData(MetodoPagoSubastaTipos.GooglePay)]
        [InlineData(MetodoPagoSubastaTipos.PayPal)]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_MetodoPago_Success_test(MetodoPagoSubastaTipos metodoPago)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //además, la descripción es opcional, por eso va a null
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(2, 25) },
                null, metodoPago);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubastaDetailDTO = Assert.IsType<SubastaDetailDTO>(createdResult.Value);
            Assert.Equal(metodoPago, actualSubastaDetailDTO.MetodoPago);
            Assert.Null(actualSubastaDetailDTO.Descripcion);

            //el enum del DTO se convierte en la clase de método de pago correspondiente
            using var contextoNuevo = CreateContext();
            var metodoPagoGuardado = contextoNuevo.Subastas.Include(s => s.MetodoPago).Single().MetodoPago;
            switch (metodoPago)
            {
                case MetodoPagoSubastaTipos.Visa: Assert.IsType<Visa>(metodoPagoGuardado); break;
                case MetodoPagoSubastaTipos.GooglePay: Assert.IsType<GooglePay>(metodoPagoGuardado); break;
                case MetodoPagoSubastaTipos.PayPal: Assert.IsType<PayPal>(metodoPagoGuardado); break;
            }
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_MetodoPagoInvalido_Throws_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            //un valor que no existe en el enum: CrearMetodoPago lanza la excepción
            var subastaDTO = NuevaSubasta("peter@uclm.es", new List<SubastaItemDTO> { Item(1, 10) },
                null, (MetodoPagoSubastaTipos)99);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => controller.CreateSubasta(subastaDTO));

            //no se ha guardado ninguna subasta
            Assert.Empty(_context.Subastas);
        }
    }
}