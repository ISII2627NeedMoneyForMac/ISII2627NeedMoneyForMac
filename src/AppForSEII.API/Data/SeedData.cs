namespace AppForSEII.API.Data {
    public class SeedData {
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

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) {
            //first, it checks the user does not already exist in the DB
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //administrator role
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }


            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                //A customer class has been defined because it has different attributes (purchase, rental, etc.)
                ApplicationUser user = new ApplicationUser("3", "Peter", "Jackson", "peter@uclm.es");
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");

                result.Wait();

                if (result.IsCompletedSuccessfully) {
                    //customer role
                    userManager.AddToRoleAsync(user, roles[2]).Wait();

                }
            }

        }

        public static void SeedLibros(ApplicationDbContext dbcontext) {

            //it checks or creates the Generos it needs
            var generoNovela = dbcontext.Generos.FirstOrDefault(g => g.Nombre == "Novela");
            if (generoNovela == null) {
                generoNovela = new Genero { Nombre = "Novela" };
                dbcontext.Generos.Add(generoNovela);
            }

            var generoCiencia = dbcontext.Generos.FirstOrDefault(g => g.Nombre == "Ciencia ficción");
            if (generoCiencia == null) {
                generoCiencia = new Genero { Nombre = "Ciencia ficción" };
                dbcontext.Generos.Add(generoCiencia);
            }

            //it checks or creates the Editoriales it needs
            var editorialPlaneta = dbcontext.Editoriales.FirstOrDefault(e => e.Nombre == "Planeta");
            if (editorialPlaneta == null) {
                editorialPlaneta = new Editorial { Nombre = "Planeta" };
                dbcontext.Editoriales.Add(editorialPlaneta);
            }

            //it saves Generos and Editoriales first, so they have an Id before being used by Libros
            dbcontext.SaveChanges();

            if (dbcontext.Libros.FirstOrDefault(l => l.Titulo == "Cien años de soledad") == null) {
                var libro = new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 15.95m, 12.00m, 20);
                libro.Genero = generoNovela;
                libro.Editorial = editorialPlaneta;
                dbcontext.Libros.Add(libro);
            }

            if (dbcontext.Libros.FirstOrDefault(l => l.Titulo == "Fundación") == null) {
                var libro = new Libro("Fundación", "Isaac Asimov", new DateTime(1951, 6, 1), 9.95m, 7.50m, 15);
                libro.Genero = generoCiencia;
                libro.Editorial = editorialPlaneta;
                dbcontext.Libros.Add(libro);
            }

            dbcontext.SaveChanges();
        }

    }
}