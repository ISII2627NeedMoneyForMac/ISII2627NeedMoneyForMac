using Microsoft.AspNetCore.Identity;
using AppForSEII.API.Models;

namespace AppForSEII.API.Data {
    public class SeedData {

        // ====== CONFIGURACIÓN ======
        // Libros sintéticos adicionales (para pruebas de carga/paginación). Pon 0 para desactivarlos.
        private const int LibrosExtra = 300;
        private const int NumEmpleados = 10;
        private const int NumClientes = 50;
        private const string PasswordMasiva = "Password1234%";

        public static void Initialize(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) {
            List<string> rolesNames = new List<string> { "Administrator", "Employee", "Customer" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Users in the Database.");
            }

            try {
                //it initializes the database with generos, editoriales and libros
                SeedLibros(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred seeding the Libros in the Database.");
            }

        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) {

            foreach (string roleName in roles) {
                //it checks such role does not exist in the database 
                if (!roleManager.RoleExistsAsync(roleName).Result) {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName;
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }

        }

        // ====================== USUARIOS ======================

        private static readonly string[] Nombres = {
            "Lucía", "Mateo", "Sofía", "Hugo", "Martina", "Daniel", "Paula", "Álvaro", "Carla", "Pablo",
            "Julia", "Adrián", "Valeria", "Sergio", "Irene", "Javier", "Noa", "Diego", "Alba", "Rubén",
            "Marta", "Iván", "Claudia", "Raúl", "Nuria", "Óscar", "Elena", "Carlos", "Laura", "Andrés"
        };

        private static readonly string[] Apellidos = {
            "García", "Rodríguez", "González", "Fernández", "López", "Martínez", "Sánchez", "Pérez",
            "Gómez", "Martín", "Jiménez", "Ruiz", "Hernández", "Díaz", "Moreno", "Muñoz", "Álvarez",
            "Romero", "Alonso", "Gutiérrez", "Navarro", "Torres", "Domínguez", "Vázquez", "Ramos",
            "Gil", "Serrano", "Blanco", "Molina", "Morales", "Ortega", "Delgado", "Castro", "Ortiz"
        };

        private static string SinTildes(string s) {
            var normalized = s.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                              != System.Globalization.UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
        }

        private static void CrearUsuario(UserManager<ApplicationUser> userManager, string id, string nombre,
                                         string apellidos, string email, string password, string rol) {
            if (userManager.FindByNameAsync(email).Result != null) return;

            ApplicationUser user = new ApplicationUser(id, nombre, apellidos, email);
            user.EmailConfirmed = true;

            var result = userManager.CreateAsync(user, password).Result;
            if (result.Succeeded) {
                userManager.AddToRoleAsync(user, rol).Wait();
            }
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) {
            // --- Usuarios originales / fijos ---
            CrearUsuario(userManager, "1", "Elena", "Navarro Martínez", "elena@uclm.es", "Password1234%", roles[0]);
            CrearUsuario(userManager, "2", "Laura", "Sánchez Ruiz", "laura@uclm.es", "Password1234%", roles[1]);
            CrearUsuario(userManager, "3", "Peter", "Jackson", "peter@uclm.es", "OtherPass12$", roles[2]);

            // --- Usuarios masivos (determinista: misma semilla = mismos usuarios) ---
            var rnd = new Random(1234);

            for (int i = 1; i <= NumEmpleados; i++) {
                string nombre = Nombres[rnd.Next(Nombres.Length)];
                string ap1 = Apellidos[rnd.Next(Apellidos.Length)];
                string ap2 = Apellidos[rnd.Next(Apellidos.Length)];
                string email = $"{SinTildes(nombre)}.{SinTildes(ap1)}.emp{i}@uclm.es";
                CrearUsuario(userManager, $"E{i:000}", nombre, $"{ap1} {ap2}", email, PasswordMasiva, roles[1]);
            }

            for (int i = 1; i <= NumClientes; i++) {
                string nombre = Nombres[rnd.Next(Nombres.Length)];
                string ap1 = Apellidos[rnd.Next(Apellidos.Length)];
                string ap2 = Apellidos[rnd.Next(Apellidos.Length)];
                string email = $"{SinTildes(nombre)}.{SinTildes(ap1)}.cli{i}@uclm.es";
                CrearUsuario(userManager, $"C{i:000}", nombre, $"{ap1} {ap2}", email, PasswordMasiva, roles[2]);
            }
        }

        // ====================== LIBROS ======================

        // (Título, Autor, Año, Género, Editorial)
        private static readonly (string Titulo, string Autor, int Anio, string Genero, string Editorial)[] CatalogoLibros = {
            // --- Novela ---
            ("Cien años de soledad", "Gabriel García Márquez", 1967, "Novela", "Planeta"),
            ("La sombra del viento", "Carlos Ruiz Zafón", 2001, "Novela", "Planeta"),
            ("Don Quijote de la Mancha", "Miguel de Cervantes", 1605, "Novela", "Alfaguara"),
            ("La ciudad y los perros", "Mario Vargas Llosa", 1963, "Novela", "Alfaguara"),
            ("Pedro Páramo", "Juan Rulfo", 1955, "Novela", "Alfaguara"),
            ("La casa de los espíritus", "Isabel Allende", 1982, "Novela", "Plaza & Janés"),
            ("El amor en los tiempos del cólera", "Gabriel García Márquez", 1985, "Novela", "Penguin Random House"),
            ("Rayuela", "Julio Cortázar", 1963, "Novela", "Alfaguara"),
            ("Crónica de una muerte anunciada", "Gabriel García Márquez", 1981, "Novela", "Penguin Random House"),
            ("Los renglones torcidos de Dios", "Torcuato Luca de Tena", 1979, "Novela", "Planeta"),
            ("Nada", "Carmen Laforet", 1944, "Novela", "Destino"),
            ("La familia de Pascual Duarte", "Camilo José Cela", 1942, "Novela", "Destino"),
            ("Los santos inocentes", "Miguel Delibes", 1981, "Novela", "Destino"),
            ("Patria", "Fernando Aramburu", 2016, "Novela", "Tusquets"),
            ("El nombre de la rosa", "Umberto Eco", 1980, "Novela", "Lumen"),

            // --- Ciencia ficción ---
            ("Fundación", "Isaac Asimov", 1951, "Ciencia ficción", "Planeta"),
            ("Dune", "Frank Herbert", 1965, "Ciencia ficción", "Minotauro"),
            ("1984", "George Orwell", 1949, "Ciencia ficción", "Debolsillo"),
            ("Un mundo feliz", "Aldous Huxley", 1932, "Ciencia ficción", "Debolsillo"),
            ("Fahrenheit 451", "Ray Bradbury", 1953, "Ciencia ficción", "Minotauro"),
            ("Neuromante", "William Gibson", 1984, "Ciencia ficción", "Minotauro"),
            ("Yo, robot", "Isaac Asimov", 1950, "Ciencia ficción", "Debolsillo"),
            ("El problema de los tres cuerpos", "Liu Cixin", 2008, "Ciencia ficción", "Penguin Random House"),
            ("La mano izquierda de la oscuridad", "Ursula K. Le Guin", 1969, "Ciencia ficción", "Minotauro"),
            ("Ubik", "Philip K. Dick", 1969, "Ciencia ficción", "Minotauro"),
            ("Solaris", "Stanisław Lem", 1961, "Ciencia ficción", "Minotauro"),
            ("Hyperion", "Dan Simmons", 1989, "Ciencia ficción", "Minotauro"),

            // --- Fantasía ---
            ("El hobbit", "J. R. R. Tolkien", 1937, "Fantasía", "Minotauro"),
            ("El señor de los anillos", "J. R. R. Tolkien", 1954, "Fantasía", "Minotauro"),
            ("Harry Potter y la piedra filosofal", "J. K. Rowling", 1997, "Fantasía", "Salamandra"),
            ("El nombre del viento", "Patrick Rothfuss", 2007, "Fantasía", "Plaza & Janés"),
            ("Juego de tronos", "George R. R. Martin", 1996, "Fantasía", "Debolsillo"),
            ("El imperio final", "Brandon Sanderson", 2006, "Fantasía", "Ediciones B"),
            ("El león, la bruja y el armario", "C. S. Lewis", 1950, "Fantasía", "Destino"),
            ("Eragon", "Christopher Paolini", 2002, "Fantasía", "Roca"),

            // --- Misterio ---
            ("Asesinato en el Orient Express", "Agatha Christie", 1934, "Misterio", "Molino"),
            ("Los hombres que no amaban a las mujeres", "Stieg Larsson", 2005, "Misterio", "Destino"),
            ("El código Da Vinci", "Dan Brown", 2003, "Misterio", "Planeta"),
            ("El silencio de los corderos", "Thomas Harris", 1988, "Misterio", "Planeta"),
            ("Todo esto te daré", "Dolores Redondo", 2016, "Misterio", "Planeta"),
            ("Los crímenes de Oxford", "Guillermo Martínez", 2003, "Misterio", "Destino"),
            ("Estudio en escarlata", "Arthur Conan Doyle", 1887, "Misterio", "Alianza"),
            ("Perdida", "Gillian Flynn", 2012, "Misterio", "Penguin Random House"),

            // --- Terror ---
            ("Drácula", "Bram Stoker", 1897, "Terror", "Alianza"),
            ("Frankenstein", "Mary Shelley", 1818, "Terror", "Alianza"),
            ("It", "Stephen King", 1986, "Terror", "Debolsillo"),
            ("El resplandor", "Stephen King", 1977, "Terror", "Debolsillo"),
            ("Cementerio de animales", "Stephen King", 1983, "Terror", "Debolsillo"),
            ("El exorcista", "William Peter Blatty", 1971, "Terror", "Planeta"),
            ("Carrie", "Stephen King", 1974, "Terror", "Debolsillo"),

            // --- Historia ---
            ("Sapiens", "Yuval Noah Harari", 2011, "Historia", "Debate"),
            ("Homo Deus", "Yuval Noah Harari", 2015, "Historia", "Debate"),
            ("Armas, gérmenes y acero", "Jared Diamond", 1997, "Historia", "Debate"),
            ("SPQR", "Mary Beard", 2015, "Historia", "Crítica"),
            ("La Segunda Guerra Mundial", "Antony Beevor", 2012, "Historia", "Crítica"),
            ("Imperiofobia y leyenda negra", "Elvira Roca Barea", 2016, "Historia", "Siruela"),
            ("Breve historia de España", "Fernando García de Cortázar", 1994, "Historia", "Alianza"),

            // --- Ensayo ---
            ("Meditaciones", "Marco Aurelio", 180, "Ensayo", "Alianza"),
            ("Así habló Zaratustra", "Friedrich Nietzsche", 1883, "Ensayo", "Alianza"),
            ("El contrato social", "Jean-Jacques Rousseau", 1762, "Ensayo", "Alianza"),
            ("Crítica de la razón pura", "Immanuel Kant", 1781, "Ensayo", "Alianza"),
            ("El mito de Sísifo", "Albert Camus", 1942, "Ensayo", "Alianza"),
            ("La sociedad del cansancio", "Byung-Chul Han", 2010, "Ensayo", "Herder"),
            ("El mundo como voluntad y representación", "Arthur Schopenhauer", 1818, "Ensayo", "Alianza"),

            // --- Divulgación científica ---
            ("Breve historia del tiempo", "Stephen Hawking", 1988, "Divulgación científica", "Crítica"),
            ("Cosmos", "Carl Sagan", 1980, "Divulgación científica", "Planeta"),
            ("El gen egoísta", "Richard Dawkins", 1976, "Divulgación científica", "Salvat"),
            ("El origen de las especies", "Charles Darwin", 1859, "Divulgación científica", "Alianza"),
            ("Astrofísica para gente con prisa", "Neil deGrasse Tyson", 2017, "Divulgación científica", "Paidós"),
            ("El cerebro de Broca", "Carl Sagan", 1979, "Divulgación científica", "Crítica"),

            // --- Informática ---
            ("Clean Code", "Robert C. Martin", 2008, "Informática", "Pearson"),
            ("Clean Architecture", "Robert C. Martin", 2017, "Informática", "Pearson"),
            ("Design Patterns", "Erich Gamma et al.", 1994, "Informática", "Pearson"),
            ("Refactoring", "Martin Fowler", 1999, "Informática", "Pearson"),
            ("The Pragmatic Programmer", "Andrew Hunt y David Thomas", 1999, "Informática", "Pearson"),
            ("Introduction to Algorithms", "Thomas H. Cormen et al.", 1990, "Informática", "MIT Press"),

            // --- Poesía ---
            ("Veinte poemas de amor y una canción desesperada", "Pablo Neruda", 1924, "Poesía", "Seix Barral"),
            ("Romancero gitano", "Federico García Lorca", 1928, "Poesía", "Cátedra"),
            ("Rimas y leyendas", "Gustavo Adolfo Bécquer", 1871, "Poesía", "Cátedra"),
            ("Campos de Castilla", "Antonio Machado", 1912, "Poesía", "Cátedra"),
            ("Poeta en Nueva York", "Federico García Lorca", 1940, "Poesía", "Cátedra"),

            // --- Infantil y juvenil ---
            ("El Principito", "Antoine de Saint-Exupéry", 1943, "Infantil y juvenil", "Salamandra"),
            ("Matilda", "Roald Dahl", 1988, "Infantil y juvenil", "Alfaguara"),
            ("Momo", "Michael Ende", 1973, "Infantil y juvenil", "Alfaguara"),
            ("La historia interminable", "Michael Ende", 1979, "Infantil y juvenil", "Alfaguara"),
            ("Charlie y la fábrica de chocolate", "Roald Dahl", 1964, "Infantil y juvenil", "Alfaguara"),
            ("Los juegos del hambre", "Suzanne Collins", 2008, "Infantil y juvenil", "Molino"),
            ("Platero y yo", "Juan Ramón Jiménez", 1914, "Infantil y juvenil", "Cátedra"),
        };

        // Pools para los libros sintéticos
        private static readonly string[] AutoresFicticios = {
            "Ana Beltrán", "Luis Ferrer", "Marina Soler", "Tomás Quiroga", "Beatriz Lagos", "Ignacio Peña",
            "Rosa Valdés", "Fermín Cano", "Alicia Montoya", "Ramón Ibáñez", "Gloria Esteban", "Héctor Calvo"
        };

        private static readonly string[] PalabrasTitulo = {
            "El secreto de", "La última noche en", "Crónicas de", "Memorias de", "El viaje a", "Sombras sobre",
            "El enigma de", "Cartas desde", "La herencia de", "El regreso a", "Ecos de", "La caída de"
        };

        private static readonly string[] Lugares = {
            "Toledo", "Marte", "la Mancha", "Atlántida", "Venecia", "Kioto", "el Cairo", "Praga",
            "Lisboa", "Sevilla", "Estambul", "Alejandría", "Valparaíso", "Reikiavik", "Bagdad", "Granada"
        };

        private static readonly string[] Formatos = { "Físico", "Digital" };

        public static void SeedLibros(ApplicationDbContext dbcontext) {

            // ---------- Géneros ----------
            var nombresGeneros = CatalogoLibros.Select(l => l.Genero).Distinct().ToList();
            var generos = dbcontext.Generos.ToDictionary(g => g.Nombre, g => g);
            foreach (var nombre in nombresGeneros) {
                if (!generos.ContainsKey(nombre)) {
                    var g = new Genero { Nombre = nombre };
                    dbcontext.Generos.Add(g);
                    generos[nombre] = g;
                }
            }

            // ---------- Editoriales ----------
            var nombresEditoriales = CatalogoLibros.Select(l => l.Editorial).Distinct().ToList();
            var editoriales = dbcontext.Editoriales.ToDictionary(e => e.Nombre, e => e);
            foreach (var nombre in nombresEditoriales) {
                if (!editoriales.ContainsKey(nombre)) {
                    var e = new Editorial { Nombre = nombre };
                    dbcontext.Editoriales.Add(e);
                    editoriales[nombre] = e;
                }
            }

            //it saves Generos and Editoriales first, so they have an Id before being used by Libros
            dbcontext.SaveChanges();

            // ---------- Libros reales ----------
            var titulosExistentes = new HashSet<string>(dbcontext.Libros.Select(l => l.Titulo));
            var rnd = new Random(42); // semilla fija => datos reproducibles
            var nuevos = new List<Libro>();

            foreach (var item in CatalogoLibros) {
                if (!titulosExistentes.Add(item.Titulo)) continue;

                nuevos.Add(CrearLibro(rnd, item.Titulo, item.Autor, item.Anio,
                                      generos[item.Genero], editoriales[item.Editorial]));
            }

            // ---------- Libros sintéticos ----------
            if (LibrosExtra > 0) {
                var listaGeneros = generos.Values.ToList();
                var listaEditoriales = editoriales.Values.ToList();

                for (int i = 1; i <= LibrosExtra; i++) {
                    string titulo = $"{PalabrasTitulo[rnd.Next(PalabrasTitulo.Length)]} {Lugares[rnd.Next(Lugares.Length)]} (Vol. {i:000})";
                    if (!titulosExistentes.Add(titulo)) continue;

                    string autor = AutoresFicticios[rnd.Next(AutoresFicticios.Length)];
                    int anio = rnd.Next(1950, 2025);

                    nuevos.Add(CrearLibro(rnd, titulo, autor, anio,
                                          listaGeneros[rnd.Next(listaGeneros.Count)],
                                          listaEditoriales[rnd.Next(listaEditoriales.Count)]));
                }
            }

            dbcontext.Libros.AddRange(nuevos);
            dbcontext.SaveChanges();
        }

        private static Libro CrearLibro(Random rnd, string titulo, string autor, int anio, Genero genero, Editorial editorial) {
            // Fecha de publicación: el año indicado con mes/día aleatorio
            var fecha = new DateTime(anio, rnd.Next(1, 13), rnd.Next(1, 29));

            // Precio de venta entre 6,95 y 29,95; alquiler entre el 55 % y el 80 % de la venta
            decimal precioVenta = Math.Round((decimal)(6.95 + rnd.NextDouble() * 23.0), 2);
            decimal precioAlquiler = Math.Round(precioVenta * (decimal)(0.55 + rnd.NextDouble() * 0.25), 2);

            int stock = rnd.Next(5, 51);
            string formato = Formatos[rnd.Next(Formatos.Length)];

            var libro = new Libro(titulo, autor, fecha, precioVenta, precioAlquiler, stock, formato);
            libro.Genero = genero;
            libro.Editorial = editorial;
            return libro;
        }

    }
}