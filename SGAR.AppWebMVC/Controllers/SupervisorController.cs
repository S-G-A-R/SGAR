using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using System.Text;
using System.Security.Cryptography;

namespace SGAR.AppWebMVC.Controllers
{
    // 1. [Authorize(Roles = "Alcaldia, Supervisor")]
    //    Este atributo de autorización a nivel de clase indica que para acceder a cualquier acción (método público)
    //    dentro de este SupervisorController, el usuario debe estar autenticado y pertenecer a uno de los roles
    //    especificados: "Alcaldia" o "Supervisor".
    [Authorize(Roles = "Alcaldia, Supervisor")]
    public class SupervisorController : Controller
    {
        // 2. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public SupervisorController(SgarDbContext context)
        //    Este es el constructor de la clase SupervisorController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public SupervisorController(SgarDbContext context)
        {
            // 4. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 5. public IActionResult Menu()
        //    Se define una acción (método público) llamada Menu que devuelve un IActionResult.
        //    IActionResult es una interfaz que representa el resultado de una acción del controlador.
        public IActionResult Menu()
        {
            // 6. return View();
            //    Dentro de la acción Menu, se llama al método View(). Esto indica que la acción devolverá
            //    una vista (una página HTML) al navegador. Por convención, buscará una vista llamada
            //    Menu.cshtml en la carpeta Views/Supervisor.
            return View();
        }

        // 7. [AllowAnonymous]
        //    Este atributo aplicado a la acción Login() anula la autorización a nivel de clase para
        //    esta acción específica. Esto significa que cualquier usuario, autenticado o no, puede
        //    acceder a la acción Login.
        [AllowAnonymous]
        // 8. public IActionResult Login()
        //    Se define una acción llamada Login que devuelve un IActionResult. Esta acción probablemente
        //    mostrará un formulario de inicio de sesión al usuario.
        public IActionResult Login()
        {
            // 9. return View();
            //    Similar a la acción Menu, esta línea indica que la acción Login devolverá una vista,
            //    que por convención se llamará Login.cshtml y se encontrará en la carpeta Views/Supervisor.
            return View();
        }

        // 10. [AllowAnonymous]
        //     Al igual que en el punto 7, este atributo permite que usuarios no autenticados accedan
        //     a esta acción Login que maneja el envío del formulario.
        [AllowAnonymous]
        // 11. [HttpPost]
        //     Este atributo indica que esta sobrecarga de la acción Login solo se ejecutará cuando la
        //     solicitud HTTP sea de tipo POST. Esto es típico para el envío de formularios, donde los
        //     datos se envían al servidor.
        [HttpPost]
        // 12. public async Task<IActionResult> Login(Supervisor supervisor)
        //     Se define una acción Login que acepta un objeto del tipo Supervisor como parámetro.
        //     ASP.NET Core intentará crear e inicializar este objeto a partir de los datos del formulario
        //     enviados en la solicitud POST (model binding). La palabra clave async indica que este método
        //     puede realizar operaciones asíncronas, y devuelve un Task<IActionResult> que representa
        //     el resultado de la operación asíncrona.
        public async Task<IActionResult> Login(Supervisor supervisor)
        {
            // 13. try { ... } catch { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir
            //     durante el proceso de inicio de sesión.
            try
            {
                // 14. supervisor.Password = GenerarHash256(supervisor.Password);
                //     Se asume que existe una función llamada GenerarHash256 que toma la contraseña
                //     proporcionada por el usuario y genera su hash (probablemente utilizando un algoritmo
                //     seguro como SHA-256). La contraseña del objeto supervisor se reemplaza con su
                //     versión hasheada antes de compararla con la base de datos.
                supervisor.Password = GenerarHash256(supervisor.Password);
                // 15. var supervisorAuth = await _context.Supervisores.FirstOrDefaultAsync(s => s.CorreoLaboral == supervisor.CorreoLaboral && s.Password == supervisor.Password);
                //     Se realiza una consulta asíncrona a la base de datos utilizando Entity Framework Core
                //     a través del _context. Se busca en la tabla Supervisores el primer registro (si existe)
                //     cuyo CorreoLaboral coincida con el correo electrónico proporcionado por el usuario
                //     y cuya Password (ya hasheada) coincida con la contraseña hasheada. FirstOrDefaultAsync
                //     devuelve null si no se encuentra ningún registro coincidente.
                var supervisorAuth = await _context.Supervisores.FirstOrDefaultAsync(s =>
                    s.CorreoLaboral == supervisor.CorreoLaboral && s.Password == supervisor.Password);

                // 16. if (supervisorAuth != null && supervisorAuth.Id > 0 && supervisorAuth.CorreoLaboral == supervisor.CorreoLaboral)
                //     Se verifica si se encontró un supervisor en la base de datos (supervisorAuth != null),
                //     si su Id es mayor que 0 (lo que indica que es un registro válido) y si el correo
                //     electrónico recuperado coincide con el proporcionado (una comprobación redundante pero
                //     potencialmente útil).
                if (supervisorAuth != null && supervisorAuth.Id > 0 && supervisorAuth.CorreoLaboral == supervisor.CorreoLaboral)
                {
                    // 17. var claims = new[] { ... };
                    //     Si se encuentra un supervisor válido, se crea un array de objetos Claim.
                    //     Las "claims" son piezas de información sobre el usuario autenticado.
                    var claims = new[] {
                    // 18. new Claim("Id", supervisorAuth.Id.ToString()),
                    //     Se crea una claim con el tipo "Id" y el valor del ID del supervisor.
                    new Claim("Id", supervisorAuth.Id.ToString()),
                    // 19. new Claim("Alcaldia", supervisorAuth.IdAlcaldia.ToString()),
                    //     Se crea una claim con el tipo "Alcaldia" y el valor del ID de la alcaldía
                    //     asociada al supervisor.
                    new Claim("Alcaldia", supervisorAuth.IdAlcaldia.ToString()),
                    // 20. new Claim("Nombre", supervisorAuth.Nombre + " " + supervisorAuth.Apellido),
                    //     Se crea una claim con el tipo "Nombre" y el valor del nombre completo del supervisor.
                    new Claim("Nombre", supervisorAuth.Nombre + " " + supervisorAuth.Apellido),
                    // 21. new Claim(ClaimTypes.Role, supervisorAuth.GetType().Name)
                    //     Se crea una claim con el tipo estándar ClaimTypes.Role y el valor del nombre
                    //     del tipo del objeto supervisorAuth (probablemente "Supervisor"). Esto se utiliza
                    //     para la autorización basada en roles.
                    new Claim(ClaimTypes.Role, supervisorAuth.GetType().Name)
                };
                    // 22. var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    //     Se crea un objeto ClaimsIdentity a partir del array de claims y el esquema de
                    //     autenticación por cookies (CookieAuthenticationDefaults.AuthenticationScheme).
                    //     ClaimsIdentity representa la identidad del usuario.
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // 23. await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    //     Se realiza una operación asíncrona para iniciar la sesión del usuario. Se utiliza
                    //     el esquema de autenticación por cookies y se crea un ClaimsPrincipal a partir del
                    //     ClaimsIdentity. El HttpContext.SignInAsync crea y configura la cookie de autenticación
                    //     en el navegador del usuario.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    // 24. return RedirectToAction("Menu", "Supervisor");
                    //     Después de iniciar sesión exitosamente, se redirige al usuario a la acción Menu del
                    //     SupervisorController.
                    return RedirectToAction("Menu", "Supervisor");
                }
                // 25. else { ... }
                //     Si no se encuentra un supervisor válido en la base de datos o las credenciales no
                //     coinciden, se ejecuta este bloque else.
                else
                {
                    // 26. ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    //     Se agrega un error al ModelState. ModelState contiene información sobre el estado
                    //     del modelo y los errores de validación. El primer argumento vacío indica un error
                    //     a nivel de modelo (no asociado a una propiedad específica), y el segundo argumento
                    //     es el mensaje de error que se mostrará al usuario.
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    // 27. return View();
                    //     Se devuelve la vista Login nuevamente. Como se agregó un error al ModelState,
                    //     la vista (si está correctamente implementada) mostrará el mensaje de error al usuario.
                    return View();
                }
            }
            // 28. catch { ... }
            //     Si ocurre alguna excepción dentro del bloque try (por ejemplo, un error de conexión
            //     a la base de datos), se ejecuta este bloque catch.
            catch
            {
                // 29. return View(supervisor);
                //     En caso de excepción, se devuelve la vista Login nuevamente, pasando el objeto supervisor
                //     que el usuario intentó enviar. Esto puede ser útil para que el usuario no tenga que
                //     volver a ingresar el correo electrónico. Sin embargo, es importante tener en cuenta
                //     que no se debe mostrar información sensible como la contraseña en este caso.
                return View(supervisor);
            }
        }

        // 1. [AllowAnonymous]
        //    Este atributo permite que cualquier usuario, autenticado o no, acceda a la acción CerrarSesion.
        [AllowAnonymous]
        // 2. public async Task<IActionResult> CerrarSesion()
        //    Se define una acción asíncrona llamada CerrarSesion que devuelve un IActionResult.
        //    La palabra clave async permite operaciones asíncronas y Task<IActionResult> representa el resultado.
        public async Task<IActionResult> CerrarSesion()
        {
            // 3. await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //    Se llama al método SignOutAsync del HttpContext para finalizar la sesión del usuario actual.
            //    Se especifica el esquema de autenticación por cookies (CookieAuthenticationDefaults.AuthenticationScheme)
            //    para indicar que se debe eliminar la cookie de autenticación. La palabra clave await asegura
            //    que esta operación asíncrona se complete antes de continuar.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // 4. return RedirectToAction("Index", "Home");
            //    Después de cerrar la sesión, se redirige al usuario a la acción "Index" del controlador "Home".
            //    Esto típicamente lleva a la página principal de la aplicación.
            return RedirectToAction("Index", "Home");
        }

        // 5. // GET: Supervisor
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta base del controlador "Supervisor" (ej. /Supervisor o /Supervisor/Index).
        // 6. public async Task<IActionResult> Index(Supervisor supervisor, int topRegistro = 10)
        //    Se define una acción asíncrona llamada Index que devuelve un IActionResult.
        //    Recibe un objeto Supervisor como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> Index(Supervisor supervisor, int topRegistro = 10)
        {
            // 7. var query = _context.Supervisores.AsQueryable();
            //    Se crea una variable query del tipo IQueryable<Supervisor> a partir del DbSet de Supervisores
            //    en el contexto de la base de datos (_context). AsQueryable() permite construir consultas LINQ
            //    de forma diferida.
            var query = _context.Supervisores.AsQueryable();
            // 8. if (!string.IsNullOrWhiteSpace(supervisor.Codigo))
            //    Se verifica si la propiedad Codigo del objeto supervisor no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(supervisor.Codigo))
                // 9. query = query.Where(s => s.Codigo.Contains(supervisor.Codigo));
                //    Si la propiedad Codigo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //    los supervisores cuyo Código contenga el valor proporcionado.
                query = query.Where(s => s.Codigo.Contains(supervisor.Codigo));
            // 10. if (!string.IsNullOrWhiteSpace(supervisor.Dui))
            //     Se verifica si la propiedad Dui del objeto supervisor no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(supervisor.Dui))
                // 11. query = query.Where(s => s.Dui.Contains(supervisor.Dui));
                //     Si la propiedad Dui tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     los supervisores cuyo Dui contenga el valor proporcionado.
                query = query.Where(s => s.Dui.Contains(supervisor.Dui));
            // 12. if (topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 13. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 14. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar los supervisores de forma
            //     descendente según su propiedad Id.
            query = query.OrderByDescending(s => s.Id);
            // 15. if (!(User.FindFirst("Id").Value == "1"))
            //     Se verifica si el valor de la claim "Id" del usuario actual NO es igual a "1".
            //     Se asume que User representa el usuario autenticado y FindFirst("Id") busca la primera claim con el tipo "Id".
            if (!(User.FindFirst("Id").Value == "1"))
                // 16. query = query.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value));
                //     Si el ID del usuario no es "1", se agrega una cláusula Where a la consulta para filtrar
                //     los supervisores cuya propiedad IdAlcaldia coincida con el valor de la claim "Id" del usuario actual,
                //     convertido a un entero. Esto sugiere que los usuarios (excepto el usuario con ID "1") solo pueden
                //     ver supervisores de su misma alcaldía.
                query = query.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value));
            // 17. return View(await query.ToListAsync());
            //     Se ejecuta la consulta LINQ de forma asíncrona utilizando ToListAsync() para obtener una lista de
            //     objetos Supervisor. Esta lista se pasa a la vista asociada a la acción Index para ser mostrada al usuario.
            return View(await query.ToListAsync());
        }

        // 18. // GET: Supervisor/Details/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Supervisor/Details/{id}, donde {id} es un parámetro.
        // 19. public async Task<IActionResult> Details(int? id)
        //     Se define una acción asíncrona llamada Details que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?). El signo de interrogación indica que puede ser nulo.
        public async Task<IActionResult> Details(int? id)
        {
            // 20. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 21. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que el recurso no se encontró.
                return NotFound();
            }

            // 22. var supervisor = await _context.Supervisores
            //         .Include(o => o.IdAlcaldiaNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //     Se realiza una consulta asíncrona a la base de datos para obtener un único supervisor cuyo Id
            //     coincida con el valor del parámetro id. Se utiliza Include para cargar de forma eager la propiedad
            //     de navegación IdAlcaldiaNavigation (asumiendo que representa la alcaldía del supervisor).
            //     FirstOrDefaultAsync devuelve el primer supervisor que coincida con el predicado o null si no se encuentra ninguno.
            var supervisor = await _context.Supervisores
                .Include(o => o.IdAlcaldiaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 23. var referentes = _context.ReferentesSupervisores
            //         .Where(s => s.IdSupervisor == supervisor.Id).ToList();
            //     Se realiza una consulta síncrona a la base de datos para obtener una lista de objetos ReferentesSupervisores
            //     cuyo IdSupervisor coincida con el Id del supervisor recuperado en el paso anterior.
            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            // 24. supervisor.ReferentesSupervisores = referentes;
            //     Se asigna la lista de referentes recuperada a la propiedad ReferentesSupervisores del objeto supervisor.
            //     Esto permite que la vista muestre los referentes asociados al supervisor.
            supervisor.ReferentesSupervisores = referentes;

            // 25. var tipos = new SortedList<int, string>();
            //     Se crea una nueva instancia de SortedList<int, string> llamada tipos. Esta colección almacena pares
            //     clave-valor ordenados por clave. Se utilizará para representar los tipos de referentes.
            var tipos = new SortedList<int, string>();
            // 26. if (supervisor == null)
            //     Se verifica si el objeto supervisor recuperado de la base de datos es nulo (no se encontró ningún supervisor con el ID proporcionado).
            if (supervisor == null)
            {
                // 27. return NotFound();
                //     Si supervisor es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }
            // 28. if (supervisor.ReferentesSupervisores != null)
            //     Se verifica si la propiedad ReferentesSupervisores del objeto supervisor no es nula.
            if (supervisor.ReferentesSupervisores != null)
            {
                // 29. int numItem = 1;
                //     Se inicializa una variable entera numItem en 1. Se utilizará para numerar los elementos de la lista de referentes en la vista.
                int numItem = 1;
                // 30. foreach (var item in supervisor.ReferentesSupervisores)
                //     Se inicia un bucle foreach para iterar a través de cada objeto item en la lista ReferentesSupervisores del supervisor.
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    // 31. item.NumItem = numItem;
                    //     Se asigna el valor actual de numItem a la propiedad NumItem del objeto item (ReferenteSupervisor).
                    //     Se asume que la clase ReferenteSupervisor tiene una propiedad NumItem para mostrar un número de orden.
                    item.NumItem = numItem;
                    // 32. numItem++;
                    //     Se incrementa el valor de numItem para el siguiente referente.
                    numItem++;
                }
                // 33. tipos.Add(1, "Personal");
                //     Se agrega un nuevo par clave-valor a la lista tipos. La clave es 1 y el valor es "Personal".
                tipos.Add(1, "Personal");
                // 34. tipos.Add(2, "Laboral");
                //     Se agrega otro par clave-valor a la lista tipos. La clave es 2 y el valor es "Laboral".
                tipos.Add(2, "Laboral");
                // 35. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                //     Se crea un nuevo objeto SelectList a partir de la lista tipos. "Key" se utilizará como el valor de las opciones
                //     en un control desplegable, "Value" como el texto visible de las opciones, y 1 se establece como el valor seleccionado por defecto.
                //     Este SelectList se almacena en el ViewBag para que pueda ser accedido y utilizado en la vista para crear un control desplegable
                //     para seleccionar el tipo de referente.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            // 36. return View(supervisor);
            //     Se devuelve la vista asociada a la acción Details, pasando el objeto supervisor (que ahora incluye su alcaldía y sus referentes)
            //     para que pueda ser mostrado al usuario.
            return View(supervisor);
        }

        // 1. // GET: Supervisor/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta /Supervisor/Create. Esta acción típicamente muestra un formulario para crear un nuevo supervisor.
        // 2. public IActionResult Create()
        //    Se define una acción pública llamada Create que devuelve un IActionResult.
        public IActionResult Create()
        {
            // 3. return View(new Supervisor());
            //    Se devuelve la vista asociada a la acción Create, pasando una nueva instancia vacía de la clase Supervisor como modelo.
            //    Esto permite que el formulario en la vista esté fuertemente tipado al modelo Supervisor.
            return View(new Supervisor());
        }

        // 4. // POST: Supervisor/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a la ruta /Supervisor/Create. Esta acción se invoca cuando se envía el formulario de creación.
        // 5. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //    Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 6. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 7. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 8. [ValidateAntiForgeryToken]
        //    Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 9. public async Task<IActionResult> Create(Supervisor supervisor)
        //    Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //    Recibe un objeto Supervisor como parámetro. ASP.NET Core intentará crear e inicializar este objeto
        //    a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Create(Supervisor supervisor)
        {
            // 10. var tipos = new SortedList<int, string>();
            //     Se declara e inicializa una nueva instancia de SortedList<int, string> llamada tipos.
            //     Aunque se declara aquí, solo se usa dentro del bloque catch para pasar datos a la vista en caso de error.
            var tipos = new SortedList<int, string>();
            // 11. try { ... } catch { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de creación del supervisor.
            try
            {
                // 12. if (supervisor.FotoFile != null)
                //     Se verifica si la propiedad FotoFile del objeto supervisor (que se espera que sea un IFormFile para la carga de archivos) no es nula.
                if (supervisor.FotoFile != null)
                {
                    // 13. supervisor.Foto = await GenerarByteImage(supervisor.FotoFile);
                    //     Se llama a una función asíncrona llamada GenerarByteImage (se asume que existe en el proyecto)
                    //     que toma el archivo de imagen cargado (supervisor.FotoFile) y devuelve su representación como un array de bytes.
                    //     El resultado se asigna a la propiedad Foto del objeto supervisor.
                    supervisor.Foto = await GenerarByteImage(supervisor.FotoFile);
                }
                // 14. supervisor.IdAlcaldia = Convert.ToInt32(User.FindFirst("Id").Value);
                //     Se obtiene el valor de la claim "Id" del usuario actual (se asume que representa el ID de la alcaldía)
                //     y se convierte a un entero. Este valor se asigna a la propiedad IdAlcaldia del objeto supervisor.
                //     Esto sugiere que al crear un supervisor, se asocia automáticamente a la alcaldía del usuario que lo está creando.
                supervisor.IdAlcaldia = Convert.ToInt32(User.FindFirst("Id").Value);
                // 15. supervisor.Password = GenerarHash256(supervisor.Password);
                //     Se llama a una función llamada GenerarHash256 (se asume que existe en el proyecto) para generar un hash
                //     de la contraseña proporcionada por el usuario en el objeto supervisor. La contraseña original se reemplaza con su hash
                //     antes de guardarla en la base de datos por seguridad.
                supervisor.Password = GenerarHash256(supervisor.Password);

                // 16. _context.Add(supervisor);
                //     Se agrega el objeto supervisor al contexto de la base de datos (_context). Esto marca el objeto para ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.Add(supervisor);
                // 17. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la inserción del nuevo supervisor en la tabla Supervisores.
                await _context.SaveChangesAsync();

                // 18. var supervisorId = _context.Supervisores.FirstOrDefault(s => s.Codigo == supervisor.Codigo).Id;
                //     Se realiza una consulta síncrona a la base de datos para obtener el ID del supervisor recién creado.
                //     Se busca el primer supervisor cuyo Código coincida con el Código del objeto supervisor que se acaba de guardar.
                //     Se asume que el Código es único o al menos lo suficientemente distintivo para identificar el registro correcto.
                var supervisorId = _context.Supervisores.FirstOrDefault(s => s.Codigo == supervisor.Codigo).Id;

                // 19. foreach (var item in supervisor.ReferentesSupervisores)
                //     Se inicia un bucle foreach para iterar a través de cada objeto item en la colección ReferentesSupervisores del objeto supervisor.
                //     Se asume que al crear un supervisor, también se pueden agregar referentes asociados a él a través del formulario.
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    // 20. item.IdSupervisor = supervisorId;
                    //     Se asigna el ID del supervisor recién creado (supervisorId) a la propiedad IdSupervisor de cada objeto referente.
                    //     Esto establece la relación entre el supervisor y sus referentes en la base de datos.
                    item.IdSupervisor = supervisorId;
                    // 21. _context.ReferentesSupervisores.Add(item);
                    //     Se agrega cada objeto referente al DbSet de ReferentesSupervisores en el contexto de la base de datos, marcándolos para su inserción.
                    _context.ReferentesSupervisores.Add(item);
                    // 22. await _context.SaveChangesAsync();
                    //     Se guardan de forma asíncrona los cambios en el contexto de la base de datos, lo que incluye la inserción de los nuevos referentes en la tabla ReferentesSupervisores.
                    await _context.SaveChangesAsync();
                }
                // 23. return RedirectToAction(nameof(Index));
                //     Si la creación del supervisor y sus referentes se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Supervisor, que probablemente muestra la lista de supervisores.
                return RedirectToAction(nameof(Index));

            }
            // 24. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 25. tipos.Add(1, "Personal");
                //     Se agrega un par clave-valor a la lista tipos (clave 1, valor "Personal"). Esto se hace para preparar el ViewBag en caso de error.
                tipos.Add(1, "Personal");
                // 26. tipos.Add(2, "Laboral");
                //     Se agrega otro par clave-valor a la lista tipos (clave 2, valor "Laboral").
                tipos.Add(2, "Laboral");
                // 27. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                //     Se crea un nuevo objeto SelectList a partir de la lista tipos para ser utilizado en un control desplegable en la vista.
                //     Se establece "Key" como el valor de las opciones y "Value" como el texto visible, con la opción con clave 1 seleccionada por defecto.
                //     Este SelectList se almacena en el ViewBag para que pueda ser accedido en la vista.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                // 28. return View(supervisor);
                //     En caso de excepción durante el proceso de creación, se devuelve la vista asociada a la acción Create,
                //     pasando el objeto supervisor que se intentó crear como modelo. Esto permite mostrar los errores de validación
                //     y los datos que el usuario ya había ingresado, facilitando la corrección y reintento.
                return View(supervisor);
            }
        }

        // 29. // GET: Supervisor/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Supervisor/Edit/{id}, donde {id} es el ID del supervisor a editar.
        //     Esta acción típicamente muestra un formulario para editar un supervisor existente.
        // 30. public async Task<IActionResult> Edit(int? id)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID del supervisor a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 31. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 32. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 33. var supervisor = await _context.Supervisores.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Supervisores utilizando el método FindAsync del contexto de la base de datos
            //     para obtener el supervisor con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var supervisor = await _context.Supervisores.FindAsync(id);

            // 34. var referentes = _context.ReferentesSupervisores
            //         .Where(s => s.IdSupervisor == supervisor.Id).ToList();
            //     Se realiza una consulta síncrona a la base de datos para obtener una lista de objetos ReferentesSupervisores
            //     cuyo IdSupervisor coincida con el Id del supervisor recuperado. Esto carga los referentes asociados al supervisor para su edición.
            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            // 35. supervisor.ReferentesSupervisores = referentes;
            //     Se asigna la lista de referentes recuperada a la propiedad ReferentesSupervisores del objeto supervisor.
            //     Esto permite que la vista de edición muestre y permita modificar los referentes asociados al supervisor.
            supervisor.ReferentesSupervisores = referentes;

            // 36. var tipos = new SortedList<int, string>();
            //     Se declara e inicializa una nueva instancia de SortedList<int, string> llamada tipos.
            //     Se utilizará para crear un SelectList para el tipo de referente en la vista de edición.
            var tipos = new SortedList<int, string>();
            // 37. if (supervisor == null)
            //     Se verifica si el objeto supervisor recuperado de la base de datos es nulo (no se encontró ningún supervisor con el ID proporcionado).
            if (supervisor == null)
            {
                // 38. return NotFound();
                //     Si supervisor es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }
            // 39. if (supervisor.ReferentesSupervisores != null)
            //     Se verifica si la propiedad ReferentesSupervisores del objeto supervisor no es nula.
            if (supervisor.ReferentesSupervisores != null)
            {
                // 40. int numItem = 1;
                //     Se inicializa una variable entera numItem en 1. Se utilizará para numerar los elementos de la lista de referentes en la vista de edición.
                int numItem = 1;
                // 41. foreach (var item in supervisor.ReferentesSupervisores)
                //     Se inicia un bucle foreach para iterar a través de cada referente asociado al supervisor.
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    // 42. item.NumItem = numItem;
                    //     Se asigna el valor actual de numItem a la propiedad NumItem de cada referente.
                    item.NumItem = numItem;
                    // 43. numItem++;
                    //     Se incrementa el contador numItem para el siguiente referente.
                    numItem++;
                }
                // 44. tipos.Add(1, "Personal");
                //     Se agrega un par clave-valor a la lista tipos (clave 1, valor "Personal").
                tipos.Add(1, "Personal");
                // 45. tipos.Add(2, "Laboral");
                //     Se agrega otro par clave-valor a la lista tipos (clave 2, valor "Laboral").
                tipos.Add(2, "Laboral");
                // 46. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                //     Se crea un objeto SelectList a partir de la lista tipos para ser utilizado en un control desplegable en la vista de edición.
                //     Se establece "Key" como el valor de las opciones y "Value" como el texto visible, con la opción con clave 1 seleccionada por defecto.
                //     Este SelectList se almacena en el ViewBag para que pueda ser accedido en la vista.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }
            // 47. return View(supervisor);
            //     Se devuelve la vista asociada a la acción Edit, pasando el objeto supervisor (que incluye su alcaldía y sus referentes)
            //     como modelo para que el formulario de edición pueda mostrar los datos existentes.
            return View(supervisor);
        }

        // 1. // POST: Supervisor/Edit/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a una ruta como /Supervisor/Edit/{id}, donde {id} es el ID del supervisor a editar.
        //    Esta acción se invoca cuando se envía el formulario de edición.
        // 2. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //    Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 3. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 4. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 5. [ValidateAntiForgeryToken]
        //    Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 6. public async Task<IActionResult> Edit(int id, Supervisor supervisor)
        //    Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //    Recibe dos parámetros:
        //    - id: El ID del supervisor a editar, que se espera que coincida con el ID en la ruta.
        //    - supervisor: Un objeto Supervisor que contiene los datos modificados del formulario, que ASP.NET Core intenta crear
        //                  e inicializar a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Edit(int id, Supervisor supervisor)
        {
            // 7. if (id != supervisor.Id)
            //    Se verifica si el ID proporcionado en la ruta (id) no coincide con el ID del supervisor que se está intentando editar (supervisor.Id).
            if (id != supervisor.Id)
            {
                // 8. return NotFound();
                //    Si los IDs no coinciden, se devuelve un resultado NotFound (código de estado HTTP 404), indicando una posible manipulación de la solicitud.
                return NotFound();
            }

            // 9. var supervisorUpdate = await _context.Supervisores
            //         .FirstOrDefaultAsync(o => o.Id == supervisor.Id);
            //    Se realiza una consulta asíncrona a la base de datos para obtener el registro del supervisor existente
            //    cuyo ID coincida con el ID del supervisor que se está editando. Se utiliza FirstOrDefaultAsync para obtener el primer resultado o null si no se encuentra ninguno.
            var supervisorUpdate = await _context.Supervisores
                .FirstOrDefaultAsync(o => o.Id == supervisor.Id);

            // 10. try { ... } catch (DbUpdateConcurrencyException) { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de edición del supervisor,
            //     específicamente capturando DbUpdateConcurrencyException, que ocurre cuando varios usuarios intentan editar el mismo registro simultáneamente.
            try
            {
                // 11. supervisorUpdate.Nombre = supervisor.Nombre;
                //     Se actualiza la propiedad Nombre del objeto supervisor existente (supervisorUpdate) con el valor proporcionado en el objeto supervisor recibido del formulario.
                supervisorUpdate.Nombre = supervisor.Nombre;
                // 12. supervisorUpdate.Apellido = supervisor.Apellido;
                //     Se actualiza la propiedad Apellido de manera similar.
                supervisorUpdate.Apellido = supervisor.Apellido;
                // 13. supervisorUpdate.Telefono = supervisor.Telefono;
                //     Se actualiza la propiedad Telefono.
                supervisorUpdate.Telefono = supervisor.Telefono;
                // 14. supervisorUpdate.CorreoPersonal = supervisor.CorreoPersonal;
                //     Se actualiza la propiedad CorreoPersonal.
                supervisorUpdate.CorreoPersonal = supervisor.CorreoPersonal;
                // 15. supervisorUpdate.Dui = supervisor.Dui;
                //     Se actualiza la propiedad Dui.
                supervisorUpdate.Dui = supervisor.Dui;
                // 16. supervisorUpdate.Codigo = supervisor.Codigo;
                //     Se actualiza la propiedad Codigo.
                supervisorUpdate.Codigo = supervisor.Codigo;
                // 17. supervisorUpdate.CorreoLaboral = supervisor.CorreoLaboral;
                //     Se actualiza la propiedad CorreoLaboral.
                supervisorUpdate.CorreoLaboral = supervisor.CorreoLaboral;
                // 18. supervisorUpdate.TelefonoLaboral = supervisor.TelefonoLaboral;
                //     Se actualiza la propiedad TelefonoLaboral.
                supervisorUpdate.TelefonoLaboral = supervisor.TelefonoLaboral;

                // 19. var fotoAnterior = await _context.Supervisores
                //         .Where(s => s.Id == supervisor.Id)
                //         .Select(s => s.Foto).FirstOrDefaultAsync();
                //     Se realiza una consulta asíncrona para obtener la foto existente del supervisor desde la base de datos.
                //     Se utiliza Select para proyectar solo la propiedad Foto y FirstOrDefaultAsync para obtener el valor o null si no existe.
                var fotoAnterior = await _context.Supervisores
                    .Where(s => s.Id == supervisor.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                // 20. supervisorUpdate.Foto = await GenerarByteImage(supervisor.FotoFile, fotoAnterior);
                //     Se llama a una función asíncrona llamada GenerarByteImage (se asume que existe en el proyecto)
                //     para procesar el archivo de imagen cargado (supervisor.FotoFile). Se pasa también la foto anterior.
                //     Esta función probablemente manejará la lógica para actualizar la foto (si se cargó una nueva) o mantener la anterior si no se cargó nada.
                //     El resultado (el nuevo array de bytes de la foto o el anterior) se asigna a la propiedad Foto de supervisorUpdate.
                supervisorUpdate.Foto = await GenerarByteImage(supervisor.FotoFile, fotoAnterior);

                // 21. var listaIds = supervisor.ReferentesSupervisores.Select(s => s.Id).ToList();
                //     Se crea una lista (listaIds) que contiene los IDs de todos los referentes asociados al objeto supervisor que se recibió del formulario.
                //     Se utiliza LINQ para seleccionar la propiedad Id de cada referente y luego ToList() para convertir el resultado en una lista.
                var listaIds = supervisor.ReferentesSupervisores.Select(s => s.Id).ToList();
                // 22. var referentes = await _context.ReferentesSupervisores.Where(s => s.IdSupervisor == supervisor.Id).Select(s => s.Id).ToListAsync();
                //     Se realiza una consulta asíncrona para obtener una lista (referentes) de los IDs de todos los referentes actualmente asociados al supervisor en la base de datos.
                var referentes = await _context.ReferentesSupervisores.Where(s => s.IdSupervisor == supervisor.Id).Select(s => s.Id).ToListAsync();
                // 23. _context.Update(supervisorUpdate);
                //     Se marca el objeto supervisor existente (supervisorUpdate) en el contexto de la base de datos como modificado.
                _context.Update(supervisorUpdate);
                // 24. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona los cambios realizados en el contexto de la base de datos, lo que incluye la actualización de los datos del supervisor en la tabla Supervisores.
                await _context.SaveChangesAsync();

                // 25. var referentDel = new List<ReferentesSupervisor>();
                //     Se crea una nueva lista vacía llamada referentDel del tipo ReferentesSupervisor. Esta lista se utilizará para almacenar los referentes que deben ser eliminados.
                var referentDel = new List<ReferentesSupervisor>();
                // 26. foreach (var referente in referentes)
                //     Se inicia un bucle foreach para iterar a través de cada ID de referente actualmente asociado al supervisor en la base de datos (obtenido en el paso 22).
                foreach (var referente in referentes)
                {
                    // 27. var existe = listaIds.FirstOrDefault(s => s == referente);
                    //     Se verifica si el ID del referente actual (de la base de datos) existe en la lista de IDs de referentes proporcionados en el formulario (listaIds).
                    var existe = listaIds.FirstOrDefault(s => s == referente);
                    // 28. if (!(existe > 0))
                    //     Si el ID del referente de la base de datos NO se encuentra en la lista de IDs del formulario (es decir, el referente fue eliminado en el formulario), se ejecuta este bloque.
                    if (!(existe > 0))
                    {
                        // 29. var find = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        //     Se realiza una consulta asíncrona para obtener el objeto ReferentesSupervisor completo correspondiente al ID que debe ser eliminado.
                        var find = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        // 30. if (find != null)
                        //     Se verifica si se encontró el referente en la base de datos.
                        if (find != null)
                            // 31. referentDel.Add(find);
                            //     Si se encontró el referente, se agrega a la lista referentDel para su posterior eliminación.
                            referentDel.Add(find);
                    }
                    // 32. else
                    //     Si el ID del referente de la base de datos SÍ se encuentra en la lista de IDs del formulario (es decir, el referente existe y posiblemente fue modificado), se ejecuta este bloque.
                    else
                    {
                        // 33. var fetch = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        //     Se realiza una consulta asíncrona para obtener el objeto ReferentesSupervisor existente de la base de datos.
                        var fetch = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        // 34. var refe = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.Id == referente);
                        //     Se busca el objeto ReferentesSupervisor correspondiente en la lista de referentes proporcionada en el formulario.
                        var refe = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.Id == referente);

                        // 35. fetch.Parentesco = refe.Parentesco;
                        //     Se actualiza la propiedad Parentesco del objeto referente existente en la base de datos con el valor proporcionado en el formulario.
                        fetch.Parentesco = refe.Parentesco;
                        // 36. fetch.Tipo = refe.Tipo;
                        //     Se actualiza la propiedad Tipo.
                        fetch.Tipo = refe.Tipo;
                        // 37. fetch.Nombre = refe.Nombre;
                        //     Se actualiza la propiedad Nombre.
                        fetch.Nombre = refe.Nombre;
                        // 38. _context.ReferentesSupervisores.Update(fetch);
                        //     Se marca el objeto referente existente en la base de datos como modificado.
                        _context.ReferentesSupervisores.Update(fetch);
                        // 39. await _context.SaveChangesAsync();
                        //     Se guardan de forma asíncrona los cambios en el contexto de la base de datos, lo que incluye la actualización de los datos del referente.
                        await _context.SaveChangesAsync();
                    }
                }

                // 40. if (referentDel.Count > 0)
                //     Se verifica si la lista referentDel contiene algún referente para eliminar.
                if (referentDel.Count > 0)
                {
                    // 41. foreach (var item in referentDel)
                    //     Se inicia un bucle foreach para iterar a través de cada objeto ReferentesSupervisor en la lista referentDel.
                    foreach (var item in referentDel)
                        // 42. _context.ReferentesSupervisores.Remove(item);
                        //     Se marca cada referente en la lista para su eliminación del contexto de la base de datos.
                        _context.ReferentesSupervisores.Remove(item);
                    // 43. await _context.SaveChangesAsync();
                    //     Se guardan de forma asíncrona los cambios en el contexto de la base de datos, lo que incluye la eliminación de los referentes marcados.
                    await _context.SaveChangesAsync();
                }

                // 44. return RedirectToAction(nameof(Index));
                //     Si la edición del supervisor y sus referentes se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Supervisor, que probablemente muestra la lista de supervisores.
                return RedirectToAction(nameof(Index));
            }
            // 45. catch (DbUpdateConcurrencyException)
            //     Se captura la excepción DbUpdateConcurrencyException, que ocurre cuando los datos han sido modificados por otro usuario desde que se cargaron para su edición.
            catch (DbUpdateConcurrencyException)
            {
                // 46. if (!SupervisorExists(supervisor.Id))
                //     Se llama a una función (se asume que existe en el proyecto) llamada SupervisorExists para verificar si el supervisor con el ID proporcionado todavía existe en la base de datos.
                if (!SupervisorExists(supervisor.Id))
                {
                    // 47. return NotFound();
                    //     Si el supervisor ya no existe, se devuelve un resultado NotFound.
                    return NotFound();
                }
                // 48. else
                //     Si el supervisor todavía existe (la excepción se debió a una edición concurrente), se ejecuta este bloque.
                else
                {
                    // 49. return View(supervisor);
                    //     Se devuelve la vista de edición nuevamente, pasando el objeto supervisor recibido del formulario como modelo.
                    //     Esto permite mostrar un mensaje al usuario indicando que los datos han sido modificados por otro usuario y que debe revisar los cambios.
                    return View(supervisor);
                }
            }
        }

        // 1. // GET: Supervisor/Delete/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a una ruta como /Supervisor/Delete/{id}, donde {id} es el ID del supervisor a eliminar.
        //    Esta acción típicamente muestra una confirmación antes de la eliminación.
        // 2. public async Task<IActionResult> Delete(int? id)
        //    Se define una acción asíncrona llamada Delete que devuelve un IActionResult.
        //    Recibe un parámetro opcional id del tipo entero (int?), que representa el ID del supervisor a eliminar.
        public async Task<IActionResult> Delete(int? id)
        {
            // 3. if (id == null)
            //    Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 4. return NotFound();
                //    Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 5. var supervisor = await _context.Supervisores
            //         .Include(o => o.IdAlcaldiaNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //    Se realiza una consulta asíncrona a la base de datos para obtener el supervisor cuyo Id coincide con el valor del parámetro id.
            //    Se utiliza Include para cargar de forma eager la propiedad de navegación IdAlcaldiaNavigation (asumiendo que representa la alcaldía del supervisor).
            //    FirstOrDefaultAsync devuelve el primer supervisor que coincida con el predicado o null si no se encuentra ninguno.
            var supervisor = await _context.Supervisores
                .Include(o => o.IdAlcaldiaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 6. var referentes = _context.ReferentesSupervisores
            //         .Where(s => s.IdSupervisor == supervisor.Id).ToList();
            //    Se realiza una consulta síncrona a la base de datos para obtener una lista de objetos ReferentesSupervisores
            //    cuyo IdSupervisor coincida con el Id del supervisor recuperado. Esto carga los referentes asociados al supervisor para mostrarlos en la confirmación de eliminación.
            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            // 7. supervisor.ReferentesSupervisores = referentes;
            //    Se asigna la lista de referentes recuperada a la propiedad ReferentesSupervisores del objeto supervisor.
            //    Esto permite que la vista de confirmación de eliminación muestre los referentes asociados al supervisor.
            supervisor.ReferentesSupervisores = referentes;

            // 8. var tipos = new SortedList<int, string>();
            //    Se declara e inicializa una nueva instancia de SortedList<int, string> llamada tipos.
            //    Aunque se declara aquí, parece que no se utiliza en esta acción GET de Delete.
            var tipos = new SortedList<int, string>();
            // 9. if (supervisor == null)
            //    Se verifica si el objeto supervisor recuperado de la base de datos es nulo (no se encontró ningún supervisor con el ID proporcionado).
            if (supervisor == null)
            {
                // 10. return NotFound();
                //     Si supervisor es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }
            // 11. if (supervisor.ReferentesSupervisores != null)
            //     Se verifica si la propiedad ReferentesSupervisores del objeto supervisor no es nula.
            if (supervisor.ReferentesSupervisores != null)
            {
                // 12. int numItem = 1;
                //     Se inicializa una variable entera numItem en 1. Se utilizará para numerar los elementos de la lista de referentes en la vista de confirmación.
                int numItem = 1;
                // 13. foreach (var item in supervisor.ReferentesSupervisores)
                //     Se inicia un bucle foreach para iterar a través de cada referente asociado al supervisor.
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    // 14. item.NumItem = numItem;
                    //     Se asigna el valor actual de numItem a la propiedad NumItem de cada referente.
                    item.NumItem = numItem;
                    // 15. numItem++;
                    //     Se incrementa el contador numItem para el siguiente referente.
                    numItem++;
                }
                // 16. tipos.Add(1, "Personal");
                //     Se agrega un par clave-valor a la lista tipos (clave 1, valor "Personal"). Aunque se agrega, ViewBag.Tipos no se utiliza en esta acción GET.
                tipos.Add(1, "Personal");
                // 17. tipos.Add(2, "Laboral");
                //     Se agrega otro par clave-valor a la lista tipos (clave 2, valor "Laboral").
                tipos.Add(2, "Laboral");
                // 18. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                //     Se crea un objeto SelectList a partir de la lista tipos. Sin embargo, este ViewBag.Tipos no parece ser utilizado en la vista de confirmación de eliminación estándar.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            // 19. return View(supervisor);
            //     Se devuelve la vista asociada a la acción Delete, pasando el objeto supervisor (que incluye su alcaldía y sus referentes)
            //     como modelo para que se muestren los detalles del supervisor que se va a eliminar y se pida confirmación al usuario.
            return View(supervisor);
        }

        // 20. // POST: Supervisor/Delete/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a la ruta /Supervisor/Delete/{id}. Esta acción se invoca cuando el usuario confirma la eliminación.
        // 21. [HttpPost, ActionName("Delete")]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //     ActionName("Delete") especifica que aunque el nombre del método es DeleteConfirmed, la acción responde a la ruta /Supervisor/Delete.
        // 22. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes para proteger contra ataques CSRF.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 23. public async Task<IActionResult> DeleteConfirmed(int id)
        //     Se define una acción asíncrona llamada DeleteConfirmed que devuelve un IActionResult.
        //     Recibe un parámetro id del tipo entero, que es el ID del supervisor a eliminar (generalmente el mismo ID que se pasó en la acción GET).
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 24. var supervisor = await _context.Supervisores.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Supervisores utilizando el método FindAsync del contexto de la base de datos
            //     para obtener el supervisor con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var supervisor = await _context.Supervisores.FindAsync(id);
            // 25. if (supervisor != null)
            //     Se verifica si se encontró un supervisor con el ID proporcionado en la base de datos.
            if (supervisor != null)
            {
                // 26. _context.Supervisores.Remove(supervisor);
                //     Si se encontró el supervisor, se marca para su eliminación del contexto de la base de datos.
                _context.Supervisores.Remove(supervisor);
            }
            // 27. await _context.SaveChangesAsync();
            //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la eliminación del supervisor (si se encontró) de la tabla Supervisores.
            await _context.SaveChangesAsync();
            // 28. return RedirectToAction(nameof(Index));
            //     Después de la eliminación (o intento de eliminación), se redirige al usuario a la acción Index del mismo controlador Supervisor, que probablemente muestra la lista de supervisores actualizada.
            return RedirectToAction(nameof(Index));
        }

        // 29. private bool SupervisorExists(int id)
        //     Se define un método privado llamado SupervisorExists que devuelve un valor booleano.
        //     Este método se utiliza para verificar si un supervisor con el ID especificado existe en la base de datos.
        // 30. return _context.Supervisores.Any(e => e.Id == id);
        //     Se realiza una consulta a la base de datos utilizando LINQ y el método Any para verificar si existe algún registro en la tabla Supervisores
        //     cuya propiedad Id coincida con el ID proporcionado. Devuelve true si existe al menos un registro coincidente, y false en caso contrario.
        private bool SupervisorExists(int id)
        {
            return _context.Supervisores.Any(e => e.Id == id);
        }

        // 1. public static string GenerarHash256(string input)
        //    Se define un método público estático llamado GenerarHash256 que toma una cadena (input) como parámetro
        //    y devuelve una cadena que representa el hash SHA-256 de la entrada. La palabra clave static
        //    indica que este método se puede llamar directamente en la clase sin necesidad de crear una instancia.
        public static string GenerarHash256(string input)
        {
            // 2. using (SHA256 sha256Hash = SHA256.Create())
            //    Se crea una instancia de la clase SHA256 utilizando la instrucción using. Esto asegura que los recursos
            //    utilizados por el objeto SHA256 (que implementa la interfaz IDisposable) se liberen correctamente
            //    una vez que se complete el bloque using. SHA256.Create() crea una instancia de la implementación
            //    predeterminada del algoritmo hash SHA-256.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 3. byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                //    Se convierte la cadena de entrada (input) a un array de bytes utilizando la codificación UTF-8
                //    (Encoding.UTF8.GetBytes(input)). Luego, se llama al método ComputeHash del objeto SHA256,
                //    que calcula el hash SHA-256 del array de bytes y devuelve el resultado como otro array de bytes (bytes).
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // 4. StringBuilder builder = new StringBuilder();
                //    Se crea una nueva instancia de la clase StringBuilder. StringBuilder se utiliza para construir cadenas
                //    de manera eficiente, especialmente cuando se realizan múltiples operaciones de concatenación.
                StringBuilder builder = new StringBuilder();
                // 5. for (int i = 0; i < bytes.Length; i++)
                //    Se inicia un bucle for que itera a través de cada byte en el array de bytes que contiene el hash.
                for (int i = 0; i < bytes.Length; i++)
                {
                    // 6. builder.Append(bytes[i].ToString("x2"));
                    //    Para cada byte en el array de hash, se convierte a su representación hexadecimal de dos caracteres
                    //    (ToString("x2")). La "x" indica formato hexadecimal y el "2" indica que siempre se deben usar dos dígitos
                    //    (anteponiendo un cero si es necesario). Este valor hexadecimal se agrega al StringBuilder.
                    builder.Append(bytes[i].ToString("x2"));
                }
                // 7. return builder.ToString();
                //    Finalmente, se convierte el contenido del StringBuilder a una cadena (que ahora contiene la representación
                //    hexadecimal del hash SHA-256) y se devuelve.
                return builder.ToString();
            }
        }

        // 8. public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        //    Se define un método público asíncrono llamado GenerarByteImage que toma dos parámetros:
        //    - file: Un objeto IFormFile opcional (puede ser nulo) que representa un archivo cargado a través de un formulario.
        //    - bytesImage: Un array de bytes opcional (puede ser nulo) que representa una imagen existente (por ejemplo, para mantener la imagen anterior si no se carga una nueva).
        //    El método devuelve un Task que contiene un array de bytes nullable (byte[]?), que representa los bytes de la imagen.
        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            // 9. byte[]? bytes = bytesImage;
            //    Se inicializa una variable local bytes con el valor del parámetro bytesImage. Esto significa que por defecto,
            //    si no se carga un nuevo archivo, se mantendrá la imagen existente (si se proporcionó).
            byte[]? bytes = bytesImage;
            // 10. if (file != null && file.Length > 0)
            //     Se verifica si el parámetro file no es nulo y si la longitud del archivo cargado es mayor que 0 (es decir, se cargó un archivo).
            if (file != null && file.Length > 0)
            {
                // 11. // Construir la ruta del archivo
                //     Este es un comentario (aunque la ruta del archivo no se construye explícitamente en este método;
                //     los bytes del archivo se leen directamente del IFormFile).
                // 12. using (var memoryStream = new MemoryStream())
                //     Se crea una nueva instancia de MemoryStream utilizando la instrucción using. MemoryStream es un flujo
                //     que almacena datos en la memoria. La instrucción using asegura que el MemoryStream se cierre y
                //     se liberen sus recursos correctamente al finalizar el bloque.
                using (var memoryStream = new MemoryStream())
                {
                    // 13. await file.CopyToAsync(memoryStream);
                    //     Se llama al método asíncrono CopyToAsync del objeto IFormFile (file) para copiar el contenido
                    //     del archivo cargado al MemoryStream. La palabra clave await asegura que esta operación asíncrona
                    //     se complete antes de continuar.
                    await file.CopyToAsync(memoryStream);
                    // 14. bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                    //     Una vez que el contenido del archivo se ha copiado al MemoryStream, se llama al método ToArray()
                    //     del MemoryStream para obtener los datos como un array de bytes. Este array de bytes se asigna
                    //     a la variable bytes, sobrescribiendo el valor inicial de bytesImage (si se cargó un nuevo archivo).
                    bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                }
            }
            // 15. return bytes;
            //     Finalmente, se devuelve el array de bytes que representa la imagen. Si se cargó un nuevo archivo,
            //     serán los bytes de ese archivo. Si no se cargó ningún archivo, se devolverá el valor original de
            //     bytesImage (la imagen existente) o null si bytesImage también era nulo.
            return bytes;
        }

        // 1. public IActionResult AddReferenteSu(Supervisor supervisor)
        //    Se define una acción pública llamada AddReferenteSu que devuelve un IActionResult.
        //    Recibe un objeto Supervisor como parámetro. ASP.NET Core intentará crear e inicializar este objeto
        //    a partir de los datos de la solicitud (model binding), aunque en este caso, se utiliza principalmente
        //    para acceder a su colección ReferentesSupervisores.
        public IActionResult AddReferenteSu(Supervisor supervisor)
        {
            // 2. var tipos = new SortedList<int, string>();
            //    Se declara e inicializa una nueva instancia de SortedList<int, string> llamada tipos.
            //    Esta colección se utilizará para crear un SelectList para el tipo de referente.
            var tipos = new SortedList<int, string>();

            // 3. if (supervisor.ReferentesSupervisores == null)
            //    Se verifica si la propiedad ReferentesSupervisores del objeto supervisor es nula.
            if (supervisor.ReferentesSupervisores == null)
                // 4. supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
                //    Si la colección ReferentesSupervisores es nula, se crea una nueva instancia de List<ReferentesSupervisor>
                //    y se asigna a la propiedad. Esto asegura que se pueda agregar un nuevo referente.
                supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
            // 5. supervisor.ReferentesSupervisores.Add(new ReferentesSupervisor { ... });
            //    Se crea una nueva instancia de la clase ReferentesSupervisor y se agrega a la colección
            //    ReferentesSupervisores del objeto supervisor.
            supervisor.ReferentesSupervisores.Add(new ReferentesSupervisor
            {
                // 6. Nombre = "",
                //    Se inicializa la propiedad Nombre del nuevo referente con una cadena vacía.
                Nombre = "",
                // 7. NumItem = supervisor.ReferentesSupervisores.Count + 1,
                //    Se asigna un número de ítem al nuevo referente. Este número es igual a la cantidad actual
                //    de referentes en la colección más uno, lo que asegura que cada referente tenga un número único y secuencial.
                NumItem = supervisor.ReferentesSupervisores.Count + 1,
                // 8. Parentesco = "",
                //    Se inicializa la propiedad Parentesco del nuevo referente con una cadena vacía.
                Parentesco = "",
                // 9. Tipo = 1
                //    Se inicializa la propiedad Tipo del nuevo referente con el valor 1 (probablemente representando un tipo por defecto).
                Tipo = 1
            });

            // 10. tipos.Add(1, "Personal");
            //     Se agrega un par clave-valor a la lista tipos (clave 1, valor "Personal").
            tipos.Add(1, "Personal");
            // 11. tipos.Add(2, "Laboral");
            //     Se agrega otro par clave-valor a la lista tipos (clave 2, valor "Laboral").
            tipos.Add(2, "Laboral");
            // 12. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            //     Se crea un nuevo objeto SelectList a partir de la lista tipos para ser utilizado en un control desplegable en la vista parcial.
            //     Se establece "Key" como el valor de las opciones y "Value" como el texto visible, con la opción con clave 1 seleccionada por defecto.
            //     Este SelectList se almacena en el ViewBag para que pueda ser accedido en la vista parcial.
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);

            // 13. return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
            //     Se devuelve una vista parcial llamada "_ReferentesSupervisor". Se pasa la colección actualizada
            //     ReferentesSupervisores del objeto supervisor como modelo a esta vista parcial. Esto permite
            //     renderizar dinámicamente la lista de referentes en la interfaz de usuario (probablemente al agregar un nuevo campo).
            return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
        }

        // 14. public IActionResult DeleteReferenteSu(int id, Supervisor supervisor)
        //     Se define una acción pública llamada DeleteReferenteSu que devuelve un IActionResult.
        //     Recibe dos parámetros:
        //     - id: Un entero que representa el número de ítem (NumItem) del referente que se va a eliminar.
        //     - supervisor: Un objeto Supervisor cuyos referentes se van a modificar.
        public IActionResult DeleteReferenteSu(int id, Supervisor supervisor)
        {
            // 15. var tipos = new SortedList<int, string>();
            //     Se declara e inicializa una nueva instancia de SortedList<int, string> llamada tipos.
            //     Se utilizará para crear un SelectList para el tipo de referente.
            var tipos = new SortedList<int, string>();
            // 16. int num = id;
            //     Se asigna el valor del parámetro id a una variable local num. Este valor representa el NumItem del referente a eliminar.
            int num = id;
            // 17. if (supervisor.ReferentesSupervisores.Count == 0)
            //     Se verifica si la colección ReferentesSupervisores del objeto supervisor está vacía.
            if (supervisor.ReferentesSupervisores.Count == 0)
                // 18. supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
                //     Si la colección está vacía (aunque esto no debería ocurrir si se está intentando eliminar un referente),
                //     se crea una nueva lista vacía. Esto podría ser una medida de seguridad para evitar errores.
                supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
            // 19. var referenteDel = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.NumItem == num);
            //     Se busca en la colección ReferentesSupervisores el primer referente cuyo NumItem coincida con el valor de num (el ID proporcionado).
            //     FirstOrDefault devuelve el objeto ReferentesSupervisor encontrado o null si no se encuentra ninguno.
            var referenteDel = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.NumItem == num);
            // 20. if (referenteDel != null)
            //     Se verifica si se encontró un referente para eliminar (es decir, referenteDel no es nulo).
            if (referenteDel != null)
            {
                // 21. supervisor.ReferentesSupervisores.Remove(referenteDel);
                //     Se elimina el objeto referente encontrado (referenteDel) de la colección ReferentesSupervisores del objeto supervisor.
                supervisor.ReferentesSupervisores.Remove(referenteDel);
                // 22. int numItemNew = 1;
                //     Se inicializa una variable entera numItemNew en 1. Se utilizará para re-numerar los elementos restantes en la colección.
                int numItemNew = 1;
                // 23. foreach (var item in supervisor.ReferentesSupervisores)
                //     Se inicia un bucle foreach para iterar a través de cada referente restante en la colección.
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    // 24. item.NumItem = numItemNew;
                    //     Se asigna el valor actual de numItemNew a la propiedad NumItem del referente actual.
                    item.NumItem = numItemNew;
                    // 25. numItemNew++;
                    //     Se incrementa el valor de numItemNew para el siguiente referente. Esto asegura que los NumItem de los referentes restantes se actualicen secuencialmente después de la eliminación.
                    numItemNew++;
                }
            }
            // 26. tipos.Add(1, "Personal");
            //     Se agrega un par clave-valor a la lista tipos (clave 1, valor "Personal").
            tipos.Add(1, "Personal");
            // 27. tipos.Add(2, "Laboral");
            //     Se agrega otro par clave-valor a la lista tipos (clave 2, valor "Laboral").
            tipos.Add(2, "Laboral");
            // 28. ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            //     Se crea un nuevo objeto SelectList a partir de la lista tipos para ser utilizado en un control desplegable en la vista parcial.
            //     Se establece "Key" como el valor de las opciones y "Value" como el texto visible, con la opción con clave 1 seleccionada por defecto.
            //     Este SelectList se almacena en el ViewBag para que pueda ser accedido en la vista parcial.
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            // 29. return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
            //     Se devuelve una vista parcial llamada "_ReferentesSupervisor". Se pasa la colección actualizada
            //     ReferentesSupervisores del objeto supervisor como modelo a esta vista parcial. Esto permite
            //     renderizar dinámicamente la lista de referentes en la interfaz de usuario después de eliminar uno.
            return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
        }
    }
}

