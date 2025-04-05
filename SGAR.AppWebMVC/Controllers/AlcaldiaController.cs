using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using System.Security.Policy;

namespace SGAR.AppWebMVC.Controllers
{
    // 1. [Authorize(Roles = "Alcaldia")]
    //    Este atributo de autorización a nivel de clase indica que para acceder a cualquier acción (método público)
    //    dentro de este AlcaldiaController, el usuario debe estar autenticado y pertenecer al rol "Alcaldia".
    [Authorize(Roles = "Alcaldia")]
    public class AlcaldiaController : Controller
    {
        // 2. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public AlcaldiaController(SgarDbContext context)
        //    Este es el constructor de la clase AlcaldiaController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public AlcaldiaController(SgarDbContext context)
        {
            // 4. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 5. [AllowAnonymous]
        //    Este atributo permite que usuarios no autenticados (anónimos) accedan a esta acción Login.
        [AllowAnonymous]
        // 6. public IActionResult Login()
        //    Se define una acción pública llamada Login que devuelve un IActionResult.
        //    Esta acción probablemente mostrará el formulario de inicio de sesión para las alcaldías.
        public IActionResult Login()
        {
            // 7. return View();
            //    Se devuelve la vista asociada a la acción Login. Por convención, buscará una vista llamada Login.cshtml
            //    en la carpeta Views/Alcaldia. Esta vista contendrá el formulario de inicio de sesión.
            return View();
        }

        // 8. [AllowAnonymous]
        //    Este atributo permite que usuarios no autenticados (anónimos) accedan a esta acción Login (POST).
        [AllowAnonymous]
        // 9. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //    Se utiliza para recibir los datos del formulario de inicio de sesión.
        [HttpPost]
        // 10. public async Task<IActionResult> Login(Alcaldia alcaldia)
        //     Se define una acción asíncrona llamada Login que devuelve un IActionResult.
        //     Recibe un objeto Alcaldia como parámetro. ASP.NET Core intentará crear e inicializar este objeto
        //     a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Login(Alcaldia alcaldia)
        {
            // 11. alcaldia.Password = GenerarHash256(alcaldia.Password);
            //     Se llama a una función (se asume que existe en el proyecto, aunque no se muestra aquí) llamada
            //     GenerarHash256 para aplicar un hash SHA-256 a la contraseña ingresada por el usuario antes de
            //     compararla con la contraseña almacenada en la base de datos. Esto es una práctica de seguridad común.
            alcaldia.Password = GenerarHash256(alcaldia.Password);
            // 12. var alcaldiaAuth = await _context.Alcaldias
            //         .FirstOrDefaultAsync(s => s.Correo == alcaldia.Correo && s.Password == alcaldia.Password);
            //     Se realiza una consulta asíncrona a la base de datos en la tabla Alcaldias.
            //     Se utiliza FirstOrDefaultAsync para obtener la primera alcaldía cuyo Correo y Password
            //     coincidan con los valores proporcionados en el objeto alcaldia (después de aplicar el hash).
            //     Si no se encuentra ninguna coincidencia, alcaldiaAuth será null.
            var alcaldiaAuth = await _context.Alcaldias
                .FirstOrDefaultAsync(s => s.Correo == alcaldia.Correo && s.Password == alcaldia.Password);
            // 13. var municipio = await _context.Municipios.FirstOrDefaultAsync(s => s.Id == alcaldiaAuth.IdMunicipio);
            //     Si se encontró una alcaldía autenticada (alcaldiaAuth no es null), se realiza otra consulta asíncrona
            //     a la base de datos en la tabla Municipios para obtener el municipio asociado a esa alcaldía
            //     a través de su IdMunicipio.
            var municipio = await _context.Municipios.FirstOrDefaultAsync(s => s.Id == alcaldiaAuth.IdMunicipio);
            // 14. if (alcaldiaAuth != null && alcaldiaAuth.Id > 0 && alcaldiaAuth.Correo == alcaldia.Correo)
            //     Se verifica si se encontró una alcaldía autenticada (alcaldiaAuth no es null), si su Id es mayor que 0 (para asegurar que es un registro válido)
            //     y si el correo electrónico coincide (una verificación adicional, aunque redundante con la consulta).
            if (alcaldiaAuth != null && alcaldiaAuth.Id > 0 && alcaldiaAuth.Correo == alcaldia.Correo)
            {
                // 15. var claims = new[] { ... };
                //     Se crea un array de objetos Claim. Las claims son piezas de información sobre el usuario autenticado.
                var claims = new[] {
                // 16. new Claim("Id", alcaldiaAuth.Id.ToString()),
                //     Se crea una claim con el tipo "Id" y el valor del ID de la alcaldía autenticada (convertido a string).
                new Claim("Id", alcaldiaAuth.Id.ToString()),
                // 17. new Claim("Municipio", municipio.Nombre),
                //     Se crea una claim con el tipo "Municipio" y el valor del nombre del municipio asociado a la alcaldía.
                new Claim("Municipio", municipio.Nombre),
                // 18. new Claim(ClaimTypes.Role, alcaldiaAuth.GetType().Name)
                //     Se crea una claim con el tipo ClaimTypes.Role (que representa el rol del usuario) y el nombre
                //     de la clase de la alcaldía (en este caso, "Alcaldia"). Esto asigna el rol "Alcaldia" al usuario.
                new Claim(ClaimTypes.Role, alcaldiaAuth.GetType().Name)
                };
                // 19. var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //     Se crea un objeto ClaimsIdentity a partir del array de claims y el esquema de autenticación por cookies.
                //     ClaimsIdentity representa la identidad del usuario autenticado.
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                // 20. await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                //     Se realiza el proceso de autenticación del usuario. HttpContext.SignInAsync crea una cookie de autenticación
                //     utilizando el esquema especificado (CookieAuthenticationDefaults.AuthenticationScheme) y almacena el
                //     ClaimsPrincipal (que contiene la identidad del usuario) en esa cookie.
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                // 21. return RedirectToAction("Index", "Alcaldia");
                //     Después de la autenticación exitosa, se redirige al usuario a la acción "Index" del controlador "Alcaldia".
                return RedirectToAction("Index", "Alcaldia");
            }
            // 22. else
            //     Si la autenticación falla (alcaldiaAuth es null o no cumple las condiciones), se ejecuta este bloque.
            else
            {
                // 23. ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                //     Se agrega un error al ModelState (el estado del modelo). El primer parámetro es una clave vacía,
                //     lo que significa que el error se mostrará como un error de validación general en la vista.
                //     El segundo parámetro es el mensaje de error que se mostrará al usuario.
                ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                // 24. return View();
                //     Se devuelve la vista de inicio de sesión (Login.cshtml) nuevamente, lo que permitirá mostrar el
                //     mensaje de error al usuario para que pueda intentar iniciar sesión de nuevo.
                return View();
            }
        }

        // 25. [AllowAnonymous]
        //     Este atributo permite que usuarios no autenticados (anónimos) accedan a esta acción CerrarSesion.
        [AllowAnonymous]
        // 26. public async Task<IActionResult> CerrarSesion()
        //     Se define una acción asíncrona llamada CerrarSesion que devuelve un IActionResult.
        //     Esta acción se encarga de cerrar la sesión del usuario autenticado.
        public async Task<IActionResult> CerrarSesion()
        {
            // 27. await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //     Se realiza el proceso de cierre de sesión del usuario. HttpContext.SignOutAsync elimina la cookie
            //     de autenticación del usuario, invalidando su sesión.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // 28. return RedirectToAction("Index", "Home");
            //     Después de cerrar la sesión, se redirige al usuario a la acción "Index" del controlador "Home".
            return RedirectToAction("Index", "Home");
        }

        // 29. public IActionResult Index()
        //     Se define una acción pública llamada Index que devuelve un IActionResult.
        //     Esta acción, al estar dentro de un controlador con el atributo [Authorize(Roles = "Alcaldia")],
        //     solo será accesible para usuarios autenticados con el rol "Alcaldia".
        public IActionResult Index()
        {
            // 30. return View();
            //     Se devuelve la vista asociada a la acción Index. Por convención, buscará una vista llamada Index.cshtml
            //     en la carpeta Views/Alcaldia. Esta vista probablemente mostrará el panel principal o la página de inicio
            //     para los usuarios con el rol "Alcaldia".
            return View();
        }

        // 1. [Authorize(Policy = "Admin")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
        //    pueden acceder a esta acción List. La política "Admin" debe estar configurada en la startup de la aplicación.
        [Authorize(Policy = "Admin")]
        // 2. public async Task<IActionResult> List(Alcaldia alcaldia, int topRegistro = 10)
        //    Se define una acción asíncrona llamada List que devuelve un IActionResult.
        //    Recibe un objeto Alcaldia como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> List(Alcaldia alcaldia, int topRegistro = 10)
        {
            // 3. var query = _context.Alcaldias.AsQueryable();
            //    Se crea una consulta LINQ (IQueryable) sobre la tabla Alcaldias del contexto de la base de datos.
            //    AsQueryable() permite construir la consulta de forma diferida y eficiente.
            var query = _context.Alcaldias.AsQueryable();

            // 4. if (!string.IsNullOrWhiteSpace(alcaldia.Correo))
            //    Se verifica si la propiedad Correo del objeto alcaldia (que puede contener criterios de filtro proporcionados
            //    por el usuario a través del formulario) no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(alcaldia.Correo))
                // 5. query = query.Where(s => s.Correo.Contains(alcaldia.Correo));
                //    Si la propiedad Correo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //    las alcaldías cuyo Correo contenga el valor proporcionado.
                query = query.Where(s => s.Correo.Contains(alcaldia.Correo));

            // 6. if (alcaldia.IdMunicipio > 0)
            //    Se verifica si la propiedad IdMunicipio del objeto alcaldia es mayor que 0, lo que indica que se ha seleccionado un municipio para filtrar.
            if (alcaldia.IdMunicipio > 0)
                // 7. query = query.Where(s => s.IdMunicipio == alcaldia.IdMunicipio);
                //    Si se ha seleccionado un IdMunicipio, se agrega una cláusula Where a la consulta para filtrar
                //    las alcaldías cuyo IdMunicipio coincida con el valor proporcionado.
                query = query.Where(s => s.IdMunicipio == alcaldia.IdMunicipio);

            // 8. if (topRegistro > 0)
            //    Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 9. query = query.Take(topRegistro);
                //    Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //    de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);

            // 10. query = query.Include(p => p.IdMunicipioNavigation);
            //     Se agrega una cláusula Include a la consulta para cargar de forma eager la propiedad de navegación
            //     IdMunicipioNavigation del objeto Alcaldia. Esto asume que existe una relación entre Alcaldia y Municipio
            //     y que IdMunicipioNavigation permite acceder a la información del municipio asociado a la alcaldía.
            query = query.Include(p => p.IdMunicipioNavigation);
            // 11. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar las alcaldías de forma
            //     descendente según su propiedad Id, mostrando las más recientes primero (asumiendo que Id es autoincremental).
            query = query.OrderByDescending(s => s.Id);

            // 12. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            //     Se crea una nueva lista genérica llamada municipios del tipo Municipio y se inicializa con un nuevo objeto Municipio.
            //     Este objeto tiene un Nombre de "SELECCIONAR" y un Id de 0, que se utilizará como opción predeterminada en un dropdown.
            //     **Nota:** La sintaxis de inicialización de la lista parece incorrecta en C#. Debería ser `new List<Municipio> { ... }`.
            List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
            // 13. var departamentos = _context.Departamentos.ToList();
            //     Se realiza una consulta síncrona a la base de datos para obtener todos los registros de la tabla Departamentos
            //     y se convierten en una lista (List<Departamento>).
            var departamentos = _context.Departamentos.ToList();
            // 14. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            //     Se agrega un nuevo objeto Departamento a la lista departamentos. Este objeto tiene un Nombre de "SELECCIONAR"
            //     y un Id de 0, que se utilizará como opción predeterminada en un dropdown.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            // 15. var alcaldias = _context.Municipios.ToList();
            //     Se realiza una consulta síncrona a la base de datos para obtener todos los registros de la tabla Municipios
            //     y se convierten en una lista (List<Municipio>). **Nota:** La variable se llama "alcaldias" pero contiene municipios,
            //     lo cual puede ser confuso. Probablemente debería llamarse "municipiosList" o algo similar para evitar la ambigüedad.
            var alcaldias = _context.Municipios.ToList();

            // 16. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            //     Se agrega un objeto SelectList al ViewData con la clave "MunicipioId". Este SelectList se crea a partir de la lista
            //     municipios, utilizando la propiedad "Id" como el valor de cada opción y la propiedad "Nombre" como el texto visible.
            //     El tercer parámetro (0) establece la opción con el valor 0 como la opción seleccionada por defecto.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // 17. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            //     Se agrega un objeto SelectList al ViewData con la clave "DepartamentoId". Este SelectList se crea a partir de la lista
            //     departamentos, utilizando la propiedad "Id" como el valor de cada opción y la propiedad "Nombre" como el texto visible.
            //     El tercer parámetro (0) establece la opción con el valor 0 como la opción seleccionada por defecto.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // 18. ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");
            //     Se agrega un objeto SelectList al ViewData con la clave "AlcaldiaId". Este SelectList se crea a partir de la lista
            //     alcaldias (que en realidad contiene municipios), utilizando la propiedad "Id" como el valor de cada opción y la
            //     propiedad "Nombre" como el texto visible. No se especifica una opción seleccionada por defecto.
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            // 19. return View(await query.OrderByDescending(s => s.Id).ToListAsync());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Alcaldia>) de forma asíncrona (ToListAsync()). Esta lista de alcaldías se pasa a la vista
            //     asociada a la acción List para ser mostrada al administrador.
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // 20. public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        //     Se define una acción pública llamada GetMunicipiosFromDepartamentoId que devuelve un JsonResult.
        //     JsonResult se utiliza para enviar datos en formato JSON como respuesta HTTP.
        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            // 21. return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
            //     Se realiza una consulta síncrona a la base de datos en la tabla Municipios.
            //     Se utiliza LINQ y el método Where para seleccionar todos los municipios cuyo IdDepartamento
            //     coincida con el valor del parámetro departamentoId.
            //     El resultado de esta consulta (una lista de municipios) se convierte a formato JSON y se devuelve
            //     como respuesta HTTP. Esta acción probablemente se utiliza para cargar dinámicamente los municipios
            //     en un dropdown cuando se selecciona un departamento en un formulario.
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());

        }
        // 22. [Authorize(Policy = "Admin")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
        //     pueden acceder a esta acción Details.
        [Authorize(Policy = "Admin")]
        // 23. public async Task<IActionResult> Details(int? id)
        //     Se define una acción asíncrona llamada Details que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la alcaldía a mostrar en detalle.
        public async Task<IActionResult> Details(int? id)
        {
            // 24. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 25. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 26. var alcaldia = await _context.Alcaldias
            //         .Include(a => a.IdMunicipioNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //     Se realiza una consulta asíncrona a la base de datos en la tabla Alcaldias para obtener la alcaldía cuyo Id coincide con el valor del parámetro id.
            //     Se utiliza Include para cargar de forma eager la propiedad de navegación IdMunicipioNavigation (que representa el municipio asociado a la alcaldía).
            //     FirstOrDefaultAsync devuelve la primera alcaldía que coincida con el predicado o null si no se encuentra ninguna.
            var alcaldia = await _context.Alcaldias
                .Include(a => a.IdMunicipioNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 27. if (alcaldia == null)
            //     Se verifica si la alcaldía recuperada de la base de datos es nula (no se encontró ninguna alcaldía con el ID proporcionado).
            if (alcaldia == null)
            {
                // 28. return NotFound();
                //     Si alcaldia es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 29. return View(alcaldia);
            //     Se devuelve la vista asociada a la acción Details, pasando el objeto alcaldia (que incluye la información del municipio)
            //     como modelo para que se muestren los detalles de la alcaldía al administrador.
            return View(alcaldia);
        }

        // 1. [Authorize(Policy = "Admin")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
        //    pueden acceder a esta acción Create (GET). La política "Admin" debe estar configurada en la startup de la aplicación.
        [Authorize(Policy = "Admin")]
        // 2. public IActionResult Create()
        //    Se define una acción pública llamada Create que devuelve un IActionResult.
        //    Esta acción mostrará el formulario para crear una nueva alcaldía.
        public IActionResult Create()
        {
            // 3. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            //    Se crea una nueva lista genérica llamada municipios del tipo Municipio y se inicializa con un nuevo objeto Municipio.
            //    Este objeto tiene un Nombre de "SELECCIONAR" y un Id de 0, que se utilizará como opción predeterminada en un dropdown.
            //    **Nota:** La sintaxis de inicialización de la lista parece incorrecta en C#. Debería ser `new List<Municipio> { ... }`.
            List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
            // 4. var departamentos = _context.Departamentos.ToList();
            //    Se realiza una consulta síncrona a la base de datos para obtener todos los registros de la tabla Departamentos
            //    y se convierten en una lista (List<Departamento>).
            var departamentos = _context.Departamentos.ToList();
            // 5. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            //    Se agrega un nuevo objeto Departamento a la lista departamentos. Este objeto tiene un Nombre de "SELECCIONAR"
            //    y un Id de 0, que se utilizará como opción predeterminada en un dropdown.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // 6. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            //    Se agrega un objeto SelectList al ViewData con la clave "MunicipioId". Este SelectList se crea a partir de la lista
            //    municipios, utilizando la propiedad "Id" como el valor de cada opción y la propiedad "Nombre" como el texto visible.
            //    El cuarto parámetro (0) establece la opción con el valor 0 como la opción seleccionada por defecto.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // 7. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            //    Se agrega un objeto SelectList al ViewData con la clave "DepartamentoId". Este SelectList se crea a partir de la lista
            //    departamentos, utilizando la propiedad "Id" como el valor de cada opción y la propiedad "Nombre" como el texto visible.
            //    El cuarto parámetro (0) establece la opción con el valor 0 como la opción seleccionada por defecto.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // 8. return View();
            //    Se devuelve la vista asociada a la acción Create. Por convención, buscará una vista llamada Create.cshtml
            //    en la carpeta Views/Alcaldia. Esta vista contendrá el formulario para crear una nueva alcaldía,
            //    utilizando los SelectList creados para los dropdowns de Municipio y Departamento.
            return View();
        }

        // 9. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //    Se utiliza para recibir los datos del formulario de creación de una alcaldía.
        // 10. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 11. [Authorize(Policy = "Admin")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
        //     pueden acceder a esta acción Create (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        // 12. public async Task<IActionResult> Create([Bind("Id,IdMunicipio,Correo,Password")] Alcaldia alcaldia)
        //     Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //     Recibe un objeto Alcaldia como parámetro. El atributo [Bind] especifica las propiedades del objeto Alcaldia
        //     que se deben incluir para el model binding desde los datos del formulario. Esto ayuda a prevenir ataques de sobrepublicación.
        public async Task<IActionResult> Create([Bind("Id,IdMunicipio,Correo,Password")] Alcaldia alcaldia)
        {
            // 13. try { ... } catch { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso
            //     de creación de la alcaldía.
            try
            {
                // 14. alcaldia.Password = GenerarHash256(alcaldia.Password);
                //     Se llama a una función (se asume que existe en el proyecto) llamada GenerarHash256 para aplicar un
                //     hash a la contraseña ingresada por el administrador antes de guardarla en la base de datos.
                alcaldia.Password = GenerarHash256(alcaldia.Password);
                // 15. _context.Add(alcaldia);
                //     Se agrega el objeto alcaldia al contexto de la base de datos (_context). Esto marca el objeto para
                //     ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.Add(alcaldia);
                // 16. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos.
                //     Esto incluye la inserción de la nueva alcaldía en la tabla Alcaldias.
                await _context.SaveChangesAsync();
                // 17. return RedirectToAction(nameof(List));
                //     Si la creación de la alcaldía se realiza con éxito, se redirige al administrador a la acción List
                //     del mismo controlador Alcaldia, que probablemente muestra la lista de alcaldías.
                return RedirectToAction(nameof(List));
            }
            // 18. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 19. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                //     En caso de excepción durante el proceso de creación, se crea nuevamente la lista de municipios para el dropdown.
                //     **Nota:** La sintaxis de inicialización de la lista parece incorrecta en C#. Debería ser `new List<Municipio> { ... }`.
                List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
                // 20. var departamentos = _context.Departamentos.ToList();
                //     Se obtienen nuevamente todos los departamentos de la base de datos.
                var departamentos = _context.Departamentos.ToList();
                // 21. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                //     Se agrega nuevamente la opción predeterminada al dropdown de departamentos.
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                // 22. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                //     Se crea nuevamente el SelectList para el dropdown de municipios y se asigna al ViewData.
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                // 23. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                //     Se crea nuevamente el SelectList para el dropdown de departamentos y se asigna al ViewData.
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                // 24. return View(alcaldia);
                //     Se devuelve la vista de creación (Create.cshtml) nuevamente, pasando el objeto alcaldia como modelo.
                //     Esto permite mostrar los errores de validación y los datos que el administrador ya había ingresado,
                //     facilitando la corrección y reintento.
                return View(alcaldia);
            }
        }

        // 25. [Authorize(Policy = "Admin")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
        //     pueden acceder a esta acción Edit (GET).
        [Authorize(Policy = "Admin")]
        // 26. public async Task<IActionResult> Edit(int? id)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la alcaldía a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 27. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 28. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 29. var alcaldia = await _context.Alcaldias.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Alcaldias utilizando el método FindAsync del contexto de la base de datos
            //     para obtener la alcaldía con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var alcaldia = await _context.Alcaldias.FindAsync(id);
            // 30. if (alcaldia == null)
            //     Se verifica si la alcaldía recuperada de la base de datos es nula (no se encontró ninguna alcaldía con el ID proporcionado).
            if (alcaldia == null)
            {
                // 31. return NotFound();
                //     Si alcaldia es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 32. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            //     Se crea una nueva lista genérica llamada municipios del tipo Municipio y se inicializa con un nuevo objeto Municipio
            //     como opción predeterminada.
            //     **Nota:** La sintaxis de inicialización de la lista parece incorrecta en C#. Debería ser `new List<Municipio> { ... }`.
            List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
            // 33. var departamentos = _context.Departamentos.ToList();
            //     Se obtienen todos los departamentos de la base de datos.
            var departamentos = _context.Departamentos.ToList();
            // 34. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            //     Se agrega la opción predeterminada al dropdown de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // 35. var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == alcaldia.IdMunicipio);
            //     Se realiza una consulta síncrona a la base de datos para obtener el municipio asociado a la alcaldía que se va a editar,
            //     buscando por el IdMunicipio de la alcaldía. FirstOrDefault devuelve el primer municipio que coincida o null si no se encuentra.
            var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == alcaldia.IdMunicipio);
            // 36. municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });
            //     Se agrega el municipio actualmente asignado a la alcaldía a la lista de municipios para que esté disponible en el dropdown.
            municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });

            // 37. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            //     Se crea un SelectList para el dropdown de municipios, utilizando la lista municipios, "Id" como valor, "Nombre" como texto visible,
            //     y se establece el Id del municipio actual (miMunicipio.Id) como la opción seleccionada por defecto.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            // 38. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            //     Se crea un SelectList para el dropdown de departamentos, utilizando la lista departamentos, "Id" como valor, "Nombre" como texto visible,
            //     y se establece la opción con el valor 0 como la opción seleccionada por defecto.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // 39. return View(alcaldia);
            //     Se devuelve la vista asociada a la acción Edit, pasando el objeto alcaldia como modelo para que el formulario de edición
            //     pueda mostrar los datos existentes de la alcaldía y los dropdowns estén correctamente inicializados.
            return View(alcaldia);
        }

        // [HttpPost]: Indica que este método responde a solicitudes HTTP POST.
        [HttpPost]
        // [ValidateAntiForgeryToken]: Genera y valida un token para prevenir ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // [Authorize(Policy = "Admin")]: Requiere que el usuario esté autenticado y cumpla con la política de autorización llamada "Admin".
        [Authorize(Policy = "Admin")]
        // public async Task<IActionResult> Edit(int id, [Bind("Id,IdMunicipio,Correo")] Alcaldia alcaldia):
        // Declara un método asíncrono que devuelve un IActionResult (resultado de la acción).
        // Recibe un parámetro 'id' (entero) y un objeto 'alcaldia' del tipo Alcaldia.
        // [Bind("Id,IdMunicipio,Correo")] limita las propiedades del objeto 'alcaldia' que se llenarán desde los datos del formulario, por seguridad.
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMunicipio,Correo")] Alcaldia alcaldia)
        {
            // 1. if (id != alcaldia.Id):
            // Comprueba si el 'id' recibido en la ruta o consulta coincide con el 'Id' de la Alcaldia que se intentó enlazar desde el formulario.
            if (id != alcaldia.Id)
            {
                // 2. return NotFound():
                // Si los IDs no coinciden, devuelve un resultado NotFound (código de estado HTTP 404), indicando que el recurso no se encontró.
                return NotFound();
            }

            // 3. try:
            // Inicia un bloque try para manejar posibles excepciones que puedan ocurrir durante la actualización de la base de datos.
            try
            {
                // 4. var alcaldiaUpdate = _context.Alcaldias.FirstOrDefault(s => s.Id == id);
                // Busca la entidad Alcaldia existente en la base de datos cuyo 'Id' coincida con el 'id' recibido.
                // FirstOrDefault devuelve el primer elemento encontrado o null si no se encuentra ninguno.
                var alcaldiaUpdate = await _context.Alcaldias.FirstOrDefaultAsync(s => s.Id == id);

                // 5. if (alcaldiaUpdate == null)
                // Comprueba si se encontró la alcaldía a actualizar.
                if (alcaldiaUpdate == null)
                {
                    return NotFound(); // Si no se encuentra, devuelve NotFound.
                }

                // 6. alcaldiaUpdate.IdMunicipio = alcaldia.IdMunicipio;
                // Actualiza la propiedad 'IdMunicipio' de la entidad existente con el valor del objeto 'alcaldia' recibido.
                alcaldiaUpdate.IdMunicipio = alcaldia.IdMunicipio;

                // 7. alcaldiaUpdate.Correo = alcaldia.Correo;
                // Actualiza la propiedad 'Correo' de la entidad existente con el valor del objeto 'alcaldia' recibido.
                alcaldiaUpdate.Correo = alcaldia.Correo;

                // 8. _context.Update(alcaldiaUpdate);
                // Marca la entidad 'alcaldiaUpdate' en el contexto como modificada. Entity Framework Core rastreará los cambios.
                _context.Update(alcaldiaUpdate);

                // 9. await _context.SaveChangesAsync();
                // Guarda de forma asíncrona todos los cambios realizados en el contexto en la base de datos.
                await _context.SaveChangesAsync();

                // 10. return RedirectToAction(nameof(Index));
                // Si la actualización fue exitosa, redirige al usuario a la acción 'Index' de este controlador (generalmente la lista de alcaldías).
                return RedirectToAction(nameof(Index));
            }
            // 11. catch (DbUpdateConcurrencyException):
            // Captura la excepción DbUpdateConcurrencyException, que ocurre cuando varios usuarios intentan modificar la misma entidad simultáneamente.
            catch (DbUpdateConcurrencyException)
            {
                // 12. if (!AlcaldiaExists(alcaldia.Id)):
                // Llama a un método (que debes implementar) para verificar si la Alcaldia con el 'id' proporcionado todavía existe en la base de datos.
                if (!AlcaldiaExists(alcaldia.Id))
                {
                    // 13. return NotFound():
                    // Si la Alcaldia ya no existe (fue eliminada por otro usuario), devuelve NotFound.
                    return NotFound();
                }
                // 14. else:
                // Si la Alcaldia todavía existe, significa que hubo un conflicto de concurrencia (sus propiedades fueron modificadas por otro usuario).
                else
                {
                    // 15. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                    // Crea una lista de objetos Municipio para llenar un desplegable en la vista.
                    // Agrega un elemento "SELECCIONAR" como opción predeterminada.
                    List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
                    // **Nota:** Asegúrate de que tu modelo Municipio tenga las propiedades Nombre, Id e IdDepartamento.
                    municipios.AddRange(await _context.Municipios.ToListAsync()); // Carga los municipios reales desde la base de datos.

                    // 16. var departamentos = _context.Departamentos.ToList();
                    // Obtiene todos los objetos Departamento de la base de datos.
                    var departamentos = await _context.Departamentos.ToListAsync();
                    // **Nota:** Asegúrate de que tu modelo Departamento tenga las propiedades Nombre e Id.
                    departamentos.Insert(0, new Departamento { Nombre = "SELECCIONAR", Id = 0 }); // Agrega "SELECCIONAR" al inicio.

                    // 17. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                    // Crea un objeto SelectList a partir de la lista de municipios para usar en un control desplegable en la vista.
                    // "Id" será el valor de cada opción, "Nombre" será el texto visible, y 0 será el valor preseleccionado.
                    ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", alcaldia.IdMunicipio); // Preselecciona el IdMunicipio actual.

                    // 18. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                    // Crea un objeto SelectList a partir de la lista de departamentos para usar en un control desplegable en la vista.
                    ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0); // Puedes ajustar la preselección si es necesario.

                    // 19. return View(alcaldia);
                    // Devuelve la vista asociada a la acción 'Edit', pasando el objeto 'alcaldia' original a la vista.
                    // Esto permite mostrar los valores que el usuario intentó guardar y posiblemente un mensaje de error de concurrencia.
                    return View(alcaldia);
                }
            }
        }

        // [Authorize(Policy = "Admin")]: Este atributo a nivel de método (para la acción Delete)
// indica que solo los usuarios que cumplan con la política de autorización llamada "Admin"
// podrán acceder a esta acción. Si un usuario no autorizado intenta acceder, se le
// redirigirá a una página de inicio de sesión o se le mostrará un error de acceso denegado.
[Authorize(Policy = "Admin")]
// public async Task<IActionResult> Delete(int? id):
// Declara un método asíncrono que devuelve un IActionResult (resultado de la acción).
// Este método responde a una solicitud HTTP GET y se utiliza para mostrar la vista de confirmación de eliminación.
// Recibe un parámetro 'id' de tipo entero nullable (int?), que representa el identificador de la Alcaldia a eliminar.
public async Task<IActionResult> Delete(int? id)
{
    // 1. if (id == null):
    // Comprueba si el parámetro 'id' es nulo. Esto podría ocurrir si no se proporciona un ID en la ruta.
    if (id == null)
    {
        // 2. return NotFound():
        // Si el 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
        // indicando que el recurso solicitado no se encontró.
        return NotFound();
    }

    // 3. var alcaldia = await _context.Alcaldias
    // Busca de forma asíncrona en la tabla 'Alcaldias' del contexto de la base de datos.
    var alcaldia = await _context.Alcaldias
        // 4. .Include(a => a.IdMunicipioNavigation):
        // Realiza una carga "eager" de la propiedad de navegación 'IdMunicipioNavigation' de la entidad Alcaldia.
        // Esto significa que cuando se cargue la Alcaldia, también se cargarán los datos del Municipio asociado.
        .Include(a => a.IdMunicipioNavigation) 
        // 5. .FirstOrDefaultAsync(m => m.Id == id);
        // Busca la primera Alcaldia que cumpla con la condición de que su propiedad 'Id' sea igual al 'id' recibido.
        // FirstOrDefaultAsync devuelve el primer elemento encontrado o null si no se encuentra ninguno.
        .FirstOrDefaultAsync(m => m.Id == id);

    // 6. if (alcaldia == null):
    // Comprueba si se encontró una Alcaldia con el 'id' proporcionado en la base de datos.
    if (alcaldia == null)
    {
        // 7. return NotFound():
        // Si no se encuentra ninguna Alcaldia con ese 'id', devuelve un resultado NotFound.
        return NotFound();
    }

    // 8. return View(alcaldia);
    // Si se encontró la Alcaldia, devuelve la vista asociada a esta acción ('Delete.cshtml' por convención),
    // pasando el objeto 'alcaldia' como modelo a la vista. La vista mostrará los detalles de la Alcaldia
    // y pedirá confirmación antes de la eliminación.
    return View(alcaldia);
}

// [HttpPost, ActionName("Delete")]:
// Indica que este método responde a solicitudes HTTP POST.
// ActionName("Delete") especifica que aunque el nombre del método es DeleteConfirmed,
// la acción se invoca cuando se realiza un POST a la ruta de la acción "Delete".
[HttpPost, ActionName("Delete")]
// [ValidateAntiForgeryToken]:
// Habilita la protección contra la falsificación de solicitudes entre sitios (CSRF).
// Asegura que la solicitud POST provenga del formulario generado por la vista de eliminación.
[ValidateAntiForgeryToken]
// [Authorize(Policy = "Admin")]:
// Al igual que la acción GET Delete, esta acción POST también requiere que el usuario
// cumpla con la política de autorización "Admin".
[Authorize(Policy = "Admin")]
// public async Task<IActionResult> DeleteConfirmed(int id):
// Declara un método asíncrono que devuelve un IActionResult.
// Este método se invoca cuando el usuario confirma la eliminación en la vista.
// Recibe un parámetro 'id' (entero) que es el identificador de la Alcaldia a eliminar.
public async Task<IActionResult> DeleteConfirmed(int id)
{
    // 1. var alcaldia = await _context.Alcaldias.FindAsync(id);
    // Busca de forma asíncrona la entidad Alcaldia en la base de datos utilizando su clave primaria ('Id').
    // FindAsync es una forma más eficiente de buscar por clave primaria.
    var alcaldia = await _context.Alcaldias.FindAsync(id);
    // 2. if (alcaldia != null):
    // Comprueba si se encontró la Alcaldia con el 'id' proporcionado.
    if (alcaldia != null)
    {
        // 3. _context.Alcaldias.Remove(alcaldia);
        // Marca la entidad 'alcaldia' para ser eliminada del contexto de la base de datos.
        // Los cambios no se guardan en la base de datos hasta que se llama a SaveChangesAsync().
        _context.Alcaldias.Remove(alcaldia);
    }
    // 4. await _context.SaveChangesAsync();
    // Guarda de forma asíncrona todos los cambios realizados en el contexto (en este caso, la eliminación)
    // en la base de datos.

    // 5. return RedirectToAction(nameof(Index));
    // Después de eliminar la Alcaldia (si se encontró), redirige al usuario a la acción 'Index'
    // de este controlador (generalmente la lista de alcaldías).
    return RedirectToAction(nameof(Index));
}

// private bool AlcaldiaExists(int id):
// Declara un método privado que devuelve un valor booleano.
// Este método se utiliza internamente para verificar si una Alcaldia existe en la base de datos por su 'Id'.
private bool AlcaldiaExists(int id)
{
    // 1. return _context.Alcaldias.Any(e => e.Id == id);
    // Utiliza el método LINQ 'Any' para verificar si existe alguna entidad Alcaldia en la tabla
    // 'Alcaldias' del contexto cuya propiedad 'Id' coincida con el 'id' proporcionado.
    // Devuelve 'true' si existe al menos una Alcaldia con ese 'id', y 'false' en caso contrario.
    return _context.Alcaldias.Any(e => e.Id == id);
}

// public static string GenerarHash256(string input):
// Declara un método público, estático (se puede llamar directamente desde la clase sin crear una instancia)
// que devuelve una cadena (string).
// Este método toma una cadena 'input' como parámetro y genera su hash SHA256.
public static string GenerarHash256(string input)
{
    // 1. using (SHA256 sha256Hash = SHA256.Create()):
    // Crea una instancia del algoritmo de hash SHA256 dentro de un bloque 'using'.
    // El bloque 'using' asegura que los recursos (en este caso, el objeto SHA256) se liberen correctamente
    // una vez que se complete el bloque.
    using (SHA256 sha256Hash = SHA256.Create())
    {
        // 2. byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        // Convierte la cadena de entrada 'input' a una secuencia de bytes utilizando la codificación UTF-8.
        // Luego, calcula el hash SHA256 de esos bytes utilizando el método 'ComputeHash' del objeto SHA256.
        // El resultado es un array de bytes que representa el hash.
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // 3. StringBuilder builder = new StringBuilder();
        // Crea una instancia de la clase StringBuilder, que es más eficiente para construir cadenas
        // mediante la concatenación repetida, especialmente dentro de bucles.
        StringBuilder builder = new StringBuilder();
        // 4. for (int i = 0; i < bytes.Length; i++):
        // Itera a través de cada byte del array de bytes que contiene el hash.
        for (int i = 0; i < bytes.Length; i++)
        {
            // 5. builder.Append(bytes[i].ToString("x2"));
            // Convierte cada byte a su representación hexadecimal en formato de dos caracteres (por ejemplo, "0a", "ff").
            // La "x2" especifica que se utilicen letras minúsculas para los dígitos hexadecimales (a-f)
            // y que se asegure que cada byte se represente con dos caracteres (agregando un cero inicial si es necesario).
            // Luego, agrega esta representación hexadecimal al StringBuilder.
            builder.Append(bytes[i].ToString("x2"));
        }
        // 6. return builder.ToString();
        // Convierte el contenido del StringBuilder a una cadena y la devuelve. Esta cadena es la representación
        // hexadecimal del hash SHA256 de la cadena de entrada original.
        return builder.ToString();
    }
}
    }
}
