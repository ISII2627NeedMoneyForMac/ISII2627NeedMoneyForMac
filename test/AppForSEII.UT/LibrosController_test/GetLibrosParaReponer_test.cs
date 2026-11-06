// Importa el controlador que vamos a probar (LibrosController)
using AppForSEII.API.Controllers;
// Importa los DTOs de libro (LibroParaReponerDTO...)
using AppForSEII.API.DTOs.LibroDTOs;

// Espacio de nombres del proyecto de tests unitarios
namespace AppForSEII.UT.LibrosController_test
{
    // Clase de tests de GetLibrosParaReponer; hereda de la clase base que prepara una BD SQLite en memoria (_context)
    public class GetLibrosParaReponer_test : AppForSEII4SqliteUT
    {
        // Constructor: se ejecuta ANTES de cada test y deja la BD con datos iniciales
        public GetLibrosParaReponer_test()
        {
            // Lista de géneros de prueba
            var generos = new List<Genero>() {
                // Género 1
                new Genero(1, "Ciencia ficción"),
                // Género 2
                new Genero(2, "Drama"),
                // Género 3
                new Genero(3, "Comedia"),
                // Género 4 (no lo usa ningún libro)
                new Genero(4, "Novela")
            };

            // Editorial de prueba (id 1) compartida por todos los libros
            var editorial = new Editorial(1, "Minotauro");

            // Libro(titulo, autor, fechaLanzamiento, precioCompra, precioReposicion, stock, tipoLibro)
            // Lista de libros que se guardarán en la BD
            var libros = new List<Libro>(){
                // Libro 1: stock 0, género Ciencia ficción
                new Libro("El señor de los anillos", "Tolkien", new DateTime(2011, 10, 20), 15.0m, 10.0m, 0, "Tapa dura")
                    // Asigna género y editorial mediante inicializador de objeto
                    { Genero = generos[0], Editorial = editorial },
                // Libro 2: stock 0, género Ciencia ficción
                new Libro("La naranja mecánica", "Burgess", new DateTime(1988, 02, 23), 20.0m, 15.0m, 0, "Tapa blanda")
                    // Asigna género y editorial
                    { Genero = generos[0], Editorial = editorial },
                // Libro 3: stock 0, género Drama
                new Libro("El castillo volante", "Miyazaki", new DateTime(2007, 04, 04), 25.0m, 20.0m, 0, "Tapa blanda")
                    // Asigna género y editorial
                    { Genero = generos[1], Editorial = editorial },
                //este libro tiene stock > 0, por lo que no debe devolverse cuando
                //se consultan los libros para reponer
                // Libro 4: stock 5, género Comedia (no debe aparecer en los resultados)
                new Libro("El hombre en el castillo", "Dick", new DateTime(2015, 01, 01), 15.0m, 10.0m, 5, "Tapa dura")
                    // Asigna género y editorial
                    { Genero = generos[2], Editorial = editorial },
            };

            // Añade todos los géneros al contexto
            _context.AddRange(generos);
            // Añade la editorial al contexto
            _context.Add(editorial);
            // Añade todos los libros al contexto
            _context.AddRange(libros);
            // Guarda todo en la base de datos de prueba
            _context.SaveChanges();
        }


        // Método que proporciona los casos de prueba (filtros + resultado esperado) al test con [MemberData]
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaReponer_OK()
        {
            // LibroParaReponerDTO(id, titulo, genero, fechaLanzamiento, precioReposicion, autor)
            // DTOs esperados, uno por cada libro con stock 0
            var libroDTOs = new List<LibroParaReponerDTO>() {
                // DTO del libro 1
                new LibroParaReponerDTO(1, "El señor de los anillos", "Ciencia ficción", new DateTime(2011, 10, 20), 10.0m, "Tolkien"),
                // DTO del libro 2
                new LibroParaReponerDTO(2, "La naranja mecánica", "Ciencia ficción", new DateTime(1988, 02, 23), 15.0m, "Burgess"),
                // DTO del libro 3
                new LibroParaReponerDTO(3, "El castillo volante", "Drama", new DateTime(2007, 04, 04), 20.0m, "Miyazaki"),
            };

            //sin filtros: todos los libros con stock 0 (el libro con stock > 0 no se devuelve)
            // Caso 1: los tres libros sin stock
            var libroDTOsTC1 = new List<LibroParaReponerDTO>() { libroDTOs[0], libroDTOs[1], libroDTOs[2] }
                //el método GetLibrosParaReponer devuelve los libros ordenados por título
                // Los ordena por título, igual que hace el método del controlador
                .OrderBy(l => l.Titulo).ToList();

            //filtro por autor
            // Caso 2: autor "Burgess" -> solo el libro 2
            var libroDTOsTC2 = new List<LibroParaReponerDTO>() { libroDTOs[1] };

            //filtro por género
            // Caso 3: género "Drama" -> solo el libro 3
            var libroDTOsTC3 = new List<LibroParaReponerDTO>() { libroDTOs[2] };

            //filtro por autor y género
            // Caso 4: autor "Tolkien" y género "Ciencia ficción" -> solo el libro 1
            var libroDTOsTC4 = new List<LibroParaReponerDTO>() { libroDTOs[0] };

            //filtro por género con varios resultados
            // Caso 5: género "Ciencia ficción" -> libros 1 y 2, ordenados por título
            var libroDTOsTC5 = new List<LibroParaReponerDTO>() { libroDTOs[0], libroDTOs[1] }
                // Los ordena por título
                .OrderBy(l => l.Titulo).ToList();

            //filtro por un autor cuyo libro tiene stock > 0: no se devuelve nada
            // Caso 6: autor "Dick" (su libro tiene stock) -> lista vacía
            var libroDTOsTC6 = new List<LibroParaReponerDTO>();

            // Lista con todos los casos: cada uno es un array [filtro autor, filtro género, lista esperada]
            var allTests = new List<object?[]>
            {             //filtros a aplicar (autor, género) - libros esperados
                // Sin filtros
                new object?[] { null, null, libroDTOsTC1 },
                // Filtro solo por autor
                new object?[] { "Burgess", null, libroDTOsTC2 },
                // Filtro solo por género
                new object?[] { null, "Drama", libroDTOsTC3 },
                // Filtro por autor y género
                new object?[] { "Tolkien", "Ciencia ficción", libroDTOsTC4 },
                // Filtro por género con varios resultados
                new object?[] { null, "Ciencia ficción", libroDTOsTC5 },
                // Filtro por autor cuyo libro tiene stock > 0
                new object?[] { "Dick", null, libroDTOsTC6 },
            };

            // Devuelve los casos para que xUnit ejecute el test una vez por cada uno
            return allTests;
        }

        // Theory: el mismo test se ejecuta varias veces con distintos datos
        [Theory]
        // Los datos de entrada vienen del método estático de arriba
        [MemberData(nameof(TestCasesFor_GetLibrosParaReponer_OK))]
        // Etiqueta: este test no usa fixture de base de datos
        [Trait("Database", "WithoutFixture")]
        // Etiqueta para filtrar tests: nivel de prueba unitaria
        [Trait("LevelTesting", "Unit Testing")]
        // Recibe los filtros (pueden ser null) y la lista de libros que se espera recibir
        public async Task GetLibrosParaReponer_OK_test(string? filtroAutor, string? filtroGenero,
            IList<LibroParaReponerDTO> librosEsperados)
        {
            // Arrange
            // Crea un logger falso (mock) para no escribir logs reales
            var mock = new Mock<ILogger<LibrosController>>();
            // Crea el controlador con la BD de prueba y el logger falso
            var controller = new LibrosController(_context, mock.Object);

            // Act
            // Llama al método que se está probando con los filtros del caso
            var result = await controller.GetLibrosParaReponer(filtroAutor, filtroGenero);

            //Assert
            //comprobamos que la respuesta es de tipo OK
            // Verifica que el resultado es un 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            //y obtenemos la lista de libros
            // Verifica que el cuerpo es una List<LibroParaReponerDTO>
            var librosActuales = Assert.IsType<List<LibroParaReponerDTO>>(okResult.Value);
            // Compara la lista esperada con la recibida (mismo orden y mismo contenido)
            Assert.Equal(librosEsperados, librosActuales);
        }

    }
}