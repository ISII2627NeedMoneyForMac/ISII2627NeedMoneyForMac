// Controlador de libros (el que contiene GetLibrosParaSubastar)
using AppForSEII.API.Controllers;
// DTOs de libros, entre ellos LibroParaSubastarDTO
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibrosController_test
{
    // Clase de tests con BD SQLite en memoria nueva para cada test (_context)
    public class GetLibrosParaSubastar_test : AppForSEII4SqliteUT
    {
        // Constructor: prepara los datos antes de cada test
        public GetLibrosParaSubastar_test()
        {
            // Un género
            var genero = new Genero(1, "Novela");

            // Dos editoriales
            var editoriales = new List<Editorial>() {
                new Editorial(1, "Planeta"),
                new Editorial(2, "Alfaguara"),
            };

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // ids: Fundación = 1, Cien años de soledad = 2, Dune = 3 (sin stock)
            var libros = new List<Libro>(){
                // Libro 1: stock 15, editorial Planeta
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = genero, Editorial = editoriales[0] },
                // Libro 2: stock 20, editorial Alfaguara
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = genero, Editorial = editoriales[1] },

                //este libro tiene stock = 0, por lo que no debe devolverse nunca
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = genero, Editorial = editoriales[0] },
            };

            // Marcamos todo para insertar y lo guardamos en la BD
            _context.Add(genero);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // Casos de prueba: (filtro título, filtro editorial, lista de libros esperada)
        // object?[] permite valores null en los parámetros
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaSubastar_OK()
        {
            // LibroParaSubastarDTO(id, titulo, autor, editorial, tipoLibro)
            // DTO esperado del libro Fundación
            var fundacion = new LibroParaSubastarDTO(1, "Fundación", "Isaac Asimov", "Planeta", "Tapa blanda");
            // DTO esperado del libro Cien años de soledad
            var cienAnios = new LibroParaSubastarDTO(2, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", "Tapa dura");

            //el método GetLibrosParaSubastar devuelve los libros con stock ordenados por título
            // Listas esperadas: "Cien..." va antes que "Fund..." por orden alfabético
            var todos = new List<LibroParaSubastarDTO>() { cienAnios, fundacion };
            var soloFundacion = new List<LibroParaSubastarDTO>() { fundacion };
            var soloCienAnios = new List<LibroParaSubastarDTO>() { cienAnios };
            // Lista vacía: ningún libro cumple el filtro
            var ninguno = new List<LibroParaSubastarDTO>();

            // Cada fila es un caso: { título, editorial, resultado esperado }
            var allTests = new List<object?[]>
            {             //filtros a aplicar (titulo, editorial) - libros esperados
                // Sin filtros: todos los libros con stock (Dune queda fuera por stock 0)
                new object?[] { null, null, todos },
                // Cadenas vacías: se tratan como "sin filtro"
                new object?[] { "", "", todos },
                // Solo espacios: también se tratan como "sin filtro"
                new object?[] { "   ", "   ", todos },
                // Título que contiene "Fund": solo Fundación
                new object?[] { "Fund", null, soloFundacion },
                // Título que contiene "años": solo Cien años de soledad
                new object?[] { "años", null, soloCienAnios },
                // Editorial "Planeta": Fundación (Dune también es de Planeta pero no tiene stock)
                new object?[] { null, "Planeta", soloFundacion },
                // Editorial que contiene "Alfa": Cien años de soledad
                new object?[] { null, "Alfa", soloCienAnios },
                // Ambos filtros que coinciden en el mismo libro
                new object?[] { "Fund", "Planeta", soloFundacion },
                // Ambos filtros pero que no coinciden en ningún libro: lista vacía
                new object?[] { "Fund", "Alfaguara", ninguno },
                // Dune existe pero no tiene stock: no se devuelve
                new object?[] { "Dune", null, ninguno },
                // Título que no existe: lista vacía
                new object?[] { "Inexistente", null, ninguno },
            };

            // Devolvemos los casos a xUnit
            return allTests;
        }

        // Test parametrizado: una ejecución por cada caso de arriba
        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaSubastar_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaSubastar_OK_test(string? titulo, string? editorial,
            List<LibroParaSubastarDTO> librosEsperados)
        {
            // Arrange
            // Logger falso para LibrosController
            var mock = new Mock<ILogger<LibrosController>>();
            // Creamos el controlador con la BD de pruebas
            var controller = new LibrosController(_context, mock.Object);

            // Act
            // Llamamos al método con los filtros del caso
            var result = await controller.GetLibrosParaSubastar(titulo, editorial);

            // Assert
            //este método devuelve ActionResult<IList<...>>, por eso el OK está en result.Result
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            //y obtenemos la lista de libros
            var librosActuales = Assert.IsType<List<LibroParaSubastarDTO>>(okResult.Value);
            // La lista obtenida debe ser igual (mismo contenido y mismo orden) a la esperada
            Assert.Equal(librosEsperados, librosActuales);
        }
    }
}