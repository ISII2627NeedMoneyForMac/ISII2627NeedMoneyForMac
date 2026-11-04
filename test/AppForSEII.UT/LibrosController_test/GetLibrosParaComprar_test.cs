using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibrosController_test
{
    public class GetLibrosParaComprar_test : AppForSEII4SqliteUT
    {
        public GetLibrosParaComprar_test()
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

                //este libro tiene stock = 0, por lo que no debe devolverse nunca
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = generos[1], Editorial = editorial },
            };

            _context.AddRange(generos);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            // LibroParaComprarDTO(id, titulo, precioCompra, fechaLanzamiento, genero, editorial)
            // ids: Fundación = 1, Cien años de soledad = 2, Dune = 3 (sin stock)
            var fundacion = new LibroParaComprarDTO(1, "Fundación", 9.95m, new DateTime(1951, 6, 1), "Ciencia ficción", "Planeta");
            var cienAnios = new LibroParaComprarDTO(2, "Cien años de soledad", 15.95m, new DateTime(1967, 5, 30), "Novela", "Planeta");

            //el método GetLibrosParaComprar devuelve los libros ordenados por título
            var todos = new List<LibroParaComprarDTO>() { cienAnios, fundacion };
            var soloFundacion = new List<LibroParaComprarDTO>() { fundacion };
            var soloCienAnios = new List<LibroParaComprarDTO>() { cienAnios };
            var ninguno = new List<LibroParaComprarDTO>();

            var allTests = new List<object?[]>
            {             //filtros a aplicar (género, precioMin, precioMax) - libros esperados
                new object?[] { null, null, null, todos },
                new object?[] { "Ciencia ficción", null, null, soloFundacion },
                new object?[] { "Novela", null, null, soloCienAnios },
                new object?[] { null, null, 10m, soloFundacion },
                new object?[] { null, 10m, null, soloCienAnios },
                new object?[] { null, 9.95m, 15.95m, todos },
                new object?[] { null, 11m, 15m, ninguno },
                new object?[] { "Novela", null, 10m, ninguno },
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_OK_test(string? generoLibro, decimal? precioMin, decimal? precioMax,
            List<LibroParaComprarDTO> librosEsperados)
        {
            // Arrange
            var mock = new Mock<ILogger<LibrosController>>();
            var controller = new LibrosController(_context, mock.Object);

            // Act
            var result = await controller.GetLibrosParaComprar(generoLibro, precioMin, precioMax);

            // Assert
            //comprobamos que la respuesta es de tipo OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            //y obtenemos la lista de libros
            var librosActuales = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);
            Assert.Equal(librosEsperados, librosActuales);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            // Arrange
            var mock = new Mock<ILogger<LibrosController>>();
            var controller = new LibrosController(_context, mock.Object);

            // Act: precioMin mayor que precioMax
            var result = await controller.GetLibrosParaComprar(null, 20m, 10m);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
            Assert.Contains("precioMin&precioMax", problem.Errors.Keys);
        }
    }
}