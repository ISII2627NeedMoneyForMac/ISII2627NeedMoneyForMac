// Importamos el espacio de nombres donde se encuentra LibrosController,
// que contiene el método GetLibrosParaComprar() que vamos a probar.
using AppForSEII.API.Controllers;

// Importamos los DTO relacionados con los libros.
// Utilizaremos LibroParaComprarDTO para construir los resultados esperados.
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibrosController_test
{
    // Clase que contiene las pruebas unitarias del método
    // GetLibrosParaComprar() de LibrosController.
    //
    // Hereda de AppForSEII4SqliteUT, que proporciona el contexto
    // SQLite y la infraestructura necesaria para realizar los tests.
    public class GetLibrosParaComprar_test : AppForSEII4SqliteUT
    {
        // Constructor que prepara los datos iniciales de la base de datos.
        // Se ejecuta antes de realizar las pruebas de esta clase.
        public GetLibrosParaComprar_test()
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
            // Creamos tres libros que utilizaremos para comprobar
            // el funcionamiento de los filtros.
            var libros = new List<Libro>(){

                // Libro Fundación:
                // Precio: 9.95.
                // Stock: 15.
                // Género: Ciencia ficción.
                // Editorial: Planeta.
                new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15, "Tapa blanda")
                    { Genero = generos[1], Editorial = editorial },

                // Libro Cien años de soledad:
                // Precio: 15.95.
                // Stock: 20.
                // Género: Novela.
                // Editorial: Planeta.
                new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20, "Tapa dura")
                    { Genero = generos[0], Editorial = editorial },

                //este libro tiene stock = 0, por lo que no debe devolverse nunca
                //
                // Libro Dune:
                // Precio: 20.00.
                // Stock: 0.
                // Género: Ciencia ficción.
                // Editorial: Planeta.
                //
                // Este libro sirve para comprobar que los libros sin stock
                // no aparecen entre los libros disponibles para comprar.
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20.0m, 12.00m, 0, "Tapa dura")
                    { Genero = generos[1], Editorial = editorial },
            };

            // Añadimos los dos géneros a la base de datos.
            _context.AddRange(generos);

            // Añadimos la editorial.
            _context.Add(editorial);

            // Añadimos los tres libros.
            _context.AddRange(libros);

            // Guardamos los datos iniciales en la base de datos.
            // A partir de aquí se podrán realizar consultas sobre ellos.
            _context.SaveChanges();
        }

        // Método que proporciona los diferentes conjuntos de datos
        // que utilizará el test parametrizado GetLibrosParaComprar_OK_test.
        //
        // Devuelve IEnumerable<object?[]> porque cada conjunto de datos
        // es un array de objetos y algunos parámetros pueden ser null.
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            // LibroParaComprarDTO(
            // id, titulo, precioCompra, fechaLanzamiento, genero, editorial)
            //
            // ids:
            // Fundación = 1.
            // Cien años de soledad = 2.
            // Dune = 3 (sin stock).
            //
            // Creamos los DTO que esperamos recibir cuando consultemos
            // los dos libros que tienen stock disponible.
            var fundacion = new LibroParaComprarDTO(1, "Fundación", 9.95m, new DateTime(1951, 6, 1), "Ciencia ficción", "Planeta");

            var cienAnios = new LibroParaComprarDTO(2, "Cien años de soledad", 15.95m, new DateTime(1967, 5, 30), "Novela", "Planeta");

            //el método GetLibrosParaComprar devuelve los libros ordenados por título
            //
            // Creamos diferentes listas que representan los resultados
            // esperados según los filtros que se apliquen.

            // Lista con los dos libros disponibles.
            // El orden es alfabético por título:
            // Cien años de soledad aparece antes que Fundación.
            var todos = new List<LibroParaComprarDTO>() { cienAnios, fundacion };

            // Lista que contiene únicamente Fundación.
            var soloFundacion = new List<LibroParaComprarDTO>() { fundacion };

            // Lista que contiene únicamente Cien años de soledad.
            var soloCienAnios = new List<LibroParaComprarDTO>() { cienAnios };

            // Lista vacía: representa que ningún libro cumple los filtros.
            var ninguno = new List<LibroParaComprarDTO>();

            // Creamos todos los casos de prueba.
            //
            // Cada object?[] contiene cuatro elementos:
            // 1. Género que queremos filtrar.
            // 2. Precio mínimo.
            // 3. Precio máximo.
            // 4. Lista de libros que esperamos obtener.
            //
            // Los valores null indican que no queremos aplicar ese filtro.
            var allTests = new List<object?[]>
            {             //filtros a aplicar (género, precioMin, precioMax) - libros esperados

                // CASO 1:
                // Sin filtro de género.
                // Sin precio mínimo.
                // Sin precio máximo.
                // Esperamos los dos libros disponibles.
                new object?[] { null, null, null, todos },

                // CASO 2:
                // Filtramos por el género Ciencia ficción.
                // No establecemos límites de precio.
                // Esperamos únicamente Fundación.
                // Dune no se devuelve porque tiene stock 0.
                new object?[] { "Ciencia ficción", null, null, soloFundacion },

                // CASO 3:
                // Filtramos por el género Novela.
                // Esperamos únicamente Cien años de soledad.
                new object?[] { "Novela", null, null, soloCienAnios },

                // CASO 4:
                // Establecemos un precio máximo de 10 €.
                // No establecemos género ni precio mínimo.
                // Fundación cuesta 9.95 €, por lo que es el único resultado.
                new object?[] { null, null, 10m, soloFundacion },

                // CASO 5:
                // Establecemos un precio mínimo de 10 €.
                // No establecemos género ni precio máximo.
                // Cien años de soledad cuesta 15.95 €.
                // Fundación cuesta 9.95 €, por lo que queda excluido.
                new object?[] { null, 10m, null, soloCienAnios },

                // CASO 6:
                // Establecemos un intervalo de precios entre 9.95 € y 15.95 €.
                // Ambos libros disponibles cumplen el intervalo.
                new object?[] { null, 9.95m, 15.95m, todos },

                // CASO 7:
                // Establecemos un precio mínimo de 11 €.
                // Establecemos un precio máximo de 15 €.
                // Fundación cuesta menos de 11 €.
                // Cien años de soledad cuesta más de 15 €.
                // Ninguno cumple el intervalo.
                new object?[] { null, 11m, 15m, ninguno },

                // CASO 8:
                // Filtramos por el género Novela.
                // Establecemos un precio máximo de 10 €.
                // Cien años de soledad pertenece a Novela,
                // pero cuesta 15.95 €, superando el máximo.
                // Fundación cuesta menos de 10 €, pero no pertenece a Novela.
                // Por tanto, no hay resultados.
                new object?[] { "Novela", null, 10m, ninguno },
            };

            // Devolvemos todos los casos de prueba.
            // xUnit utilizará estos datos para ejecutar el test
            // una vez por cada combinación de filtros.
            return allTests;
        }

        // [Theory] indica que es un test parametrizado.
        // Se ejecutará varias veces utilizando los datos proporcionados
        // por el método TestCasesFor_GetLibrosParaComprar_OK.
        [Theory]

        // Indica de dónde se obtienen los datos de entrada del test.
        // nameof referencia el método que devuelve los distintos casos.
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]

        // Clasifica el test como una prueba sin fixture compartido.
        [Trait("Database", "WithoutFixture")]

        // Clasifica el test como prueba unitaria.
        [Trait("LevelTesting", "Unit Testing")]

        // Test que comprueba que GetLibrosParaComprar devuelve
        // correctamente los libros aplicando los filtros recibidos.
        //
        // Parámetros:
        // generoLibro: género por el que queremos filtrar.
        // precioMin: precio mínimo permitido.
        // precioMax: precio máximo permitido.
        // librosEsperados: lista de libros que esperamos obtener.
        //
        // Los tres primeros parámetros pueden ser null,
        // indicando que no queremos aplicar ese filtro.
        public async Task GetLibrosParaComprar_OK_test(string? generoLibro, decimal? precioMin, decimal? precioMax,
            List<LibroParaComprarDTO> librosEsperados)
        {
            // Arrange: preparamos los objetos necesarios para ejecutar el test.

            // Creamos un Mock del logger de LibrosController.
            // No necesitamos probar el funcionamiento del logger.
            var mock = new Mock<ILogger<LibrosController>>();

            // Creamos el controlador que vamos a probar.
            // Le pasamos el contexto SQLite y el logger simulado.
            var controller = new LibrosController(_context, mock.Object);

            // Act: ejecutamos el método que queremos probar.

            // Llamamos a GetLibrosParaComprar pasando los tres filtros.
            // Sus valores proceden del conjunto de datos proporcionado
            // por MemberData.
            var result = await controller.GetLibrosParaComprar(generoLibro, precioMin, precioMax);

            // Assert: comprobamos que el resultado sea correcto.

            // Comprobamos que el controlador devuelva un OkObjectResult.
            // Esto representa una respuesta HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            //y obtenemos la lista de libros
            //
            // Comprobamos que el contenido de la respuesta sea
            // una lista de objetos LibroParaComprarDTO.
            //
            // Guardamos esa lista para poder compararla posteriormente.
            var librosActuales = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);

            // Comparamos la lista esperada con la lista realmente devuelta.
            // Comprueba que los libros, sus datos y su orden coinciden.
            Assert.Equal(librosEsperados, librosActuales);
        }

        // [Fact] indica que es una prueba individual.
        // No necesita MemberData porque solamente comprueba un escenario.
        [Fact]

        // Clasifica la prueba como una prueba sin fixture compartido.
        [Trait("Database", "WithoutFixture")]

        // Clasifica el test como prueba unitaria.
        [Trait("LevelTesting", "Unit Testing")]

        // Test que comprueba que el controlador devuelve BadRequest
        // cuando el precio mínimo es superior al precio máximo.
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            // Arrange: preparamos los objetos necesarios.

            // Creamos un Mock del logger de LibrosController.
            var mock = new Mock<ILogger<LibrosController>>();

            // Creamos el controlador que vamos a probar.
            // Le pasamos el contexto SQLite y el logger simulado.
            var controller = new LibrosController(_context, mock.Object);

            // Act: ejecutamos el método que queremos probar.

            //precioMin mayor que precioMax
            //
            // Llamamos a GetLibrosParaComprar con:
            // Género: null (sin filtro).
            // Precio mínimo: 20 €.
            // Precio máximo: 10 €.
            //
            // Esta combinación es incorrecta porque el precio mínimo
            // no puede ser superior al precio máximo.
            var result = await controller.GetLibrosParaComprar(null, 20m, 10m);

            // Assert: comprobamos que el controlador detecte el error.

            // Comprobamos que la respuesta sea BadRequestObjectResult.
            // Esto representa una respuesta HTTP 400 Bad Request.
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            // Comprobamos que el contenido de la respuesta sea
            // un objeto ValidationProblemDetails.
            // Este objeto almacena los errores de validación.
            var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);

            // Comprobamos que entre las claves de los errores
            // se encuentre "precioMin&precioMax".
            //
            // Assert.Contains comprueba que un elemento está presente
            // en una colección.
            //
            // Esto verifica que el error corresponde a la validación
            // conjunta de los parámetros precioMin y precioMax.
            Assert.Contains("precioMin&precioMax", problem.Errors.Keys);
        }
    }
}