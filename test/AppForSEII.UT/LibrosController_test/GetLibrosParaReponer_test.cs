using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibrosController_test
{
    public class GetLibrosParaReponer_test : AppForSEII4SqliteUT
    {
        public GetLibrosParaReponer_test()
        {
            var generos = new List<Genero>() {
                new Genero(1, "Ciencia ficción"),
                new Genero(2, "Drama"),
                new Genero(3, "Comedia"),
                new Genero(4, "Novela")
            };

            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            var libros = new List<Libro>(){
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    { Genero = generos[0], Editorial = editorial },
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 0, "Tapa blanda")
                    { Genero = generos[0], Editorial = editorial },
                new Libro("El castillo volante", "Miyazaki", new DateTime(2007, 04, 04), 25.0m, 20.0m, 0, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },
                //este libro tiene stock > 0, por lo que no debe devolverse cuando
                //se consultan los libros para reponer
                new Libro("El hombre en el castillo", "Dick", new DateTime(2015, 01, 01), 15.0m, 10.0m, 5, "Tapa dura")
                    { Genero = generos[2], Editorial = editorial },
            };

            _context.AddRange(generos);
            _context.Add(editorial);
            _context.AddRange(libros);
            _context.SaveChanges();
        }


        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaReponer_OK()
        {
            // LibroParaReponerDTO(id, titulo, genero, fechaLanzamiento, precioReposicion, autor)
            var libroDTOs = new List<LibroParaReponerDTO>() {
                new LibroParaReponerDTO(1, "El señor de los anillos", "Ciencia ficción", new DateTime(2011, 10, 20), 10.0m, "Tolkien"),
                new LibroParaReponerDTO(2, "La naranja mecánica", "Ciencia ficción", new DateTime(1988, 02, 23), 15.0m, "Burgess"),
                new LibroParaReponerDTO(3, "El castillo volante", "Drama", new DateTime(2007, 04, 04), 20.0m, "Miyazaki"),
            };

            //sin filtros: todos los libros con stock 0 (el libro con stock > 0 no se devuelve)
            var libroDTOsTC1 = new List<LibroParaReponerDTO>() { libroDTOs[0], libroDTOs[1], libroDTOs[2] }
                //el método GetLibrosParaReponer devuelve los libros ordenados por título
                .OrderBy(l => l.Titulo).ToList();

            //filtro por autor
            var libroDTOsTC2 = new List<LibroParaReponerDTO>() { libroDTOs[1] };

            //filtro por género
            var libroDTOsTC3 = new List<LibroParaReponerDTO>() { libroDTOs[2] };

            //filtro por autor y género
            var libroDTOsTC4 = new List<LibroParaReponerDTO>() { libroDTOs[0] };

            //filtro por género con varios resultados
            var libroDTOsTC5 = new List<LibroParaReponerDTO>() { libroDTOs[0], libroDTOs[1] }
                .OrderBy(l => l.Titulo).ToList();

            //filtro por un autor cuyo libro tiene stock > 0: no se devuelve nada
            var libroDTOsTC6 = new List<LibroParaReponerDTO>();

            var allTests = new List<object?[]>
            {             //filtros a aplicar (autor, género) - libros esperados
                new object?[] { null, null, libroDTOsTC1 },
                new object?[] { "Burgess", null, libroDTOsTC2 },
                new object?[] { null, "Drama", libroDTOsTC3 },
                new object?[] { "Tolkien", "Ciencia ficción", libroDTOsTC4 },
                new object?[] { null, "Ciencia ficción", libroDTOsTC5 },
                new object?[] { "Dick", null, libroDTOsTC6 },
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaReponer_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaReponer_OK_test(string? filtroAutor, string? filtroGenero,
            IList<LibroParaReponerDTO> librosEsperados)
        {
            // Arrange
            var mock = new Mock<ILogger<LibrosController>>();
            var controller = new LibrosController(_context, mock.Object);

            // Act
            var result = await controller.GetLibrosParaReponer(filtroAutor, filtroGenero);

            //Assert
            //comprobamos que la respuesta es de tipo OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            //y obtenemos la lista de libros
            var librosActuales = Assert.IsType<List<LibroParaReponerDTO>>(okResult.Value);
            Assert.Equal(librosEsperados, librosActuales);
        }

    }
}