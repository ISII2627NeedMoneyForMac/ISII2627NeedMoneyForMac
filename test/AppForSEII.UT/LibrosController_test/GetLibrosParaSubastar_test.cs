using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibrosController_test
{
    public class GetLibrosParaSubastar_test : AppForSEII4SqliteUT
    {
        public GetLibrosParaSubastar_test()
        {
            var genero = new Genero(1, "Novela");

            var editoriales = new List<Editorial>() {
                new Editorial(1, "Planeta"),
                new Editorial(2, "Alfaguara"),
            };

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: Fundación = 1, Cien años de soledad = 2, Dune = 3 (sin stock)
            var libros = new List<Libro>(){
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = genero, Editorial = editoriales[0] },
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = genero, Editorial = editoriales[1] },

                //este libro tiene stock = 0, por lo que no debe devolverse nunca
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editoriales[0] },
            };

            _context.Add(genero);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaSubastar_OK()
        {
            // LibroParaSubastarDTO(id, titulo, autor, editorial, tipoLibro)
            var fundacion = new LibroParaSubastarDTO(1, "Fundación", "Isaac Asimov", "Planeta", "Tapa blanda");
            var cienAnios = new LibroParaSubastarDTO(2, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", "Tapa dura");

            //el método GetLibrosParaSubastar devuelve los libros con stock ordenados por título
            var todos = new List<LibroParaSubastarDTO>() { cienAnios, fundacion };
            var soloFundacion = new List<LibroParaSubastarDTO>() { fundacion };
            var soloCienAnios = new List<LibroParaSubastarDTO>() { cienAnios };
            var ninguno = new List<LibroParaSubastarDTO>();

            var allTests = new List<object?[]>
            {             //filtros a aplicar (titulo, editorial) - libros esperados
                new object?[] { null, null, todos },
                new object?[] { "", "", todos },
                new object?[] { "   ", "   ", todos },
                new object?[] { "Fund", null, soloFundacion },
                new object?[] { "años", null, soloCienAnios },
                new object?[] { null, "Planeta", soloFundacion },
                new object?[] { null, "Alfa", soloCienAnios },
                new object?[] { "Fund", "Planeta", soloFundacion },
                new object?[] { "Fund", "Alfaguara", ninguno },
                new object?[] { "Dune", null, ninguno },
                new object?[] { "Inexistente", null, ninguno },
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaSubastar_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaSubastar_OK_test(string? titulo, string? editorial,
            List<LibroParaSubastarDTO> librosEsperados)
        {
            // Arrange
            var mock = new Mock<ILogger<LibrosController>>();
            var controller = new LibrosController(_context, mock.Object);

            // Act
            var result = await controller.GetLibrosParaSubastar(titulo, editorial);

            // Assert
            //este método devuelve ActionResult<IList<...>>, por eso el OK está en result.Result
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            //y obtenemos la lista de libros
            var librosActuales = Assert.IsType<List<LibroParaSubastarDTO>>(okResult.Value);
            Assert.Equal(librosEsperados, librosActuales);
        }
    }
}