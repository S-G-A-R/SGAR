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
    // 1. [Authorize(Roles = "Ciudadano, Alcaldia")]: Este atributo a nivel de clase indica que
    //    todos los métodos de acción dentro de este controlador requerirán que el usuario esté
    //    autenticado y tenga asignado el rol de "Ciudadano" o "Alcaldia".
    public class CiudadanoController : Controller
    {
        // 2. private readonly SgarDbContext _context;: Declara una variable privada de solo lectura
        //    llamada _context que contendrá una instancia de la clase SgarDbContext. Se asume que
        //    SgarDbContext es el contexto de Entity Framework Core utilizado para interactuar con la base de datos.
        private readonly SgarDbContext _context;

        // 3. public CiudadanoController(SgarDbContext context): Este es el constructor del controlador.
        //    Recibe una instancia de SgarDbContext a través de la inyección de dependencias.
        //    La instancia recibida se asigna a la variable privada _context, permitiendo que el
        //    controlador acceda a la base de datos.
        public CiudadanoController(SgarDbContext context)
        {
            _context = context;
        }

        // 4. public IActionResult Menu(): Declara un método de acción público llamado Menu que devuelve
        //    un IActionResult. Este método se utiliza para mostrar el menú principal para los usuarios
        //    autenticados con los roles permitidos (Ciudadano o Alcaldia).
        public IActionResult Menu()
        {
            // 5. return View(): Devuelve la vista asociada a la acción Menu (por convención, "Menu.cshtml").
            return View();
        }

        // 6. [AllowAnonymous]: Este atributo a nivel de método sobreescribe la autorización a nivel de clase.
        //    Indica que esta acción (Login) puede ser accedida por usuarios no autenticados.
        [AllowAnonymous]
        // 7. public IActionResult Login(): Declara un método de acción público llamado Login que devuelve
        //    un IActionResult. Este método se utiliza para mostrar el formulario de inicio de sesión.
        public IActionResult Login()
        {
            // 8. return View(): Devuelve la vista asociada a la acción Login (por convención, "Login.cshtml").
            return View();
        }

        // 9. [AllowAnonymous]: Al igual que la acción GET Login, este atributo permite que usuarios no
        //    autenticados accedan a esta acción de inicio de sesión que procesa el envío del formulario.
        [AllowAnonymous]
        // 10. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //     Esto significa que se espera que un formulario HTML envíe datos a esta URL utilizando el método POST.
        [HttpPost]
        // 11. public async Task<IActionResult> Login(Ciudadano ciudadano): Declara un método de acción
        //     asíncrono que devuelve un IActionResult. Este método recibe un objeto de tipo Ciudadano
        //     que se espera que contenga los datos del formulario de inicio de sesión (correo y contraseña).
        public async Task<IActionResult> Login(Ciudadano ciudadano)
        {
            // 12. try: Inicia un bloque try para envolver el código que podría generar excepciones.
            try
            {
                // 13. ciudadano.Password = GenerarHash256(ciudadano.Password);: Llama a un método
                //     (se asume que existe en otra parte del código, como en el controlador anterior)
                //     llamado GenerarHash256 para calcular el hash SHA256 de la contraseña ingresada
                //     por el usuario. La contraseña en texto plano se reemplaza con su hash antes de
                //     compararla con la contraseña almacenada en la base de datos.
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                // 14. var ciudadanoAuth = await _context.Ciudadanos.FirstOrDefaultAsync(s => s.Correo == ciudadano.Correo && s.Password == ciudadano.Password);:
                //     Realiza una consulta asíncrona a la tabla Ciudadanos en la base de datos a través
                //     del contexto _context. Busca el primer ciudadano cuyo correo electrónico y contraseña
                //     (ya hasheada) coincidan con los valores proporcionados en el objeto 'ciudadano'.
                //     FirstOrDefaultAsync devuelve el primer elemento que cumple la condición o null si no se encuentra ninguno.
                var ciudadanoAuth = await _context.Ciudadanos.FirstOrDefaultAsync(s => s.Correo == ciudadano.Correo && s.Password == ciudadano.Password);
                // 15. var zona = await _context.Zonas.FirstOrDefaultAsync(s => s.Id == ciudadanoAuth.ZonaId);:
                //     Si se encontró un ciudadano autenticado ('ciudadanoAuth' no es null), realiza otra
                //     consulta asíncrona a la tabla Zonas para obtener la zona asociada al ciudadano
                //     autenticado, utilizando la propiedad ZonaId del ciudadano.
                var zona = await _context.Zonas.FirstOrDefaultAsync(s => s.Id == ciudadanoAuth.ZonaId);
                // 16. var alcaldia = await _context.Alcaldias.FirstOrDefaultAsync(s => s.Id == zona.IdAlcaldia);:
                //     Realiza una tercera consulta asíncrona a la tabla Alcaldias para obtener la alcaldía
                //     asociada a la zona obtenida en el paso anterior, utilizando la propiedad IdAlcaldia de la zona.
                var alcaldia = await _context.Alcaldias.FirstOrDefaultAsync(s => s.Id == zona.IdAlcaldia);
                // 17. if (ciudadanoAuth != null && ciudadanoAuth.Id > 0 && ciudadanoAuth.Correo == ciudadano.Correo):
                //     Comprueba si se encontró un ciudadano autenticado ('ciudadanoAuth' no es null), si su Id es mayor que 0
                //     (una comprobación adicional de validez), y si su correo electrónico coincide con el correo
                //     electrónico proporcionado en el formulario.
                if (ciudadanoAuth != null && ciudadanoAuth.Id > 0 && ciudadanoAuth.Correo == ciudadano.Correo)
                {
                    // 18. var claims = new[] { ... };: Crea un array de objetos Claim. Las Claims son piezas
                    //     de información sobre el usuario autenticado (su ID, roles, etc.).
                    var claims = new[] {
                    // 19. new Claim("Id", ciudadanoAuth.Id.ToString()),: Crea una Claim con el tipo "Id"
                    //     y el valor del ID del ciudadano autenticado (convertido a string).
                    new Claim("Id", ciudadanoAuth.Id.ToString()),
                    // 20. new Claim("Zona", zona.Nombre),: Crea una Claim con el tipo "Zona" y el valor
                    //     del nombre de la zona asociada al ciudadano.
                    new Claim("Zona", zona.Nombre),
                    // 21. new Claim("Alcaldia", alcaldia.Id.ToString()),: Crea una Claim con el tipo "Alcaldia"
                    //     y el valor del ID de la alcaldía asociada a la zona (convertido a string).
                    new Claim("Alcaldia", alcaldia.Id.ToString()),
                    // 22. new Claim("Nombre", ciudadanoAuth.Nombre + " " + ciudadanoAuth.Apellido),: Crea
                    //     una Claim con el tipo "Nombre" y el valor del nombre completo del ciudadano.
                    new Claim("Nombre", ciudadanoAuth.Nombre + " " + ciudadanoAuth.Apellido),
                    // 23. new Claim(ClaimTypes.Role, ciudadanoAuth.GetType().Name): Crea una Claim con
                    //     el tipo ClaimTypes.Role (que indica el rol del usuario) y el valor del nombre
                    //     del tipo de la clase Ciudadano (por ejemplo, "Ciudadano"). Esto establece el rol del usuario.
                    new Claim(ClaimTypes.Role, ciudadanoAuth.GetType().Name)
                };
                    // 24. var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);:
                    //     Crea un objeto ClaimsIdentity a partir del array de Claims creado anteriormente y el
                    //     esquema de autenticación por cookies (CookieAuthenticationDefaults.AuthenticationScheme).
                    //     ClaimsIdentity representa la identidad del usuario autenticado.
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // 25. await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));:
                    //     Realiza el inicio de sesión del usuario de forma asíncrona utilizando el esquema de
                    //     autenticación por cookies y un nuevo ClaimsPrincipal creado a partir de la ClaimsIdentity.
                    //     Esto crea una cookie de autenticación en el navegador del usuario.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    // 26. return RedirectToAction("Menu", "Ciudadano");: Redirige al usuario autenticado a la
                    //     acción "Menu" del controlador "Ciudadano".
                    return RedirectToAction("Menu", "Ciudadano");
                }
                // 27. else: Si no se encuentra un ciudadano autenticado con las credenciales proporcionadas.
                else
                {
                    // 28. ModelState.AddModelError("", "El email o contraseña estan incorrectos");: Agrega un
                    //     error al ModelState (el estado del modelo). El primer argumento es una clave (vacía
                    //     para errores a nivel de formulario) y el segundo es el mensaje de error que se mostrará al usuario.
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    // 29. return View(): Devuelve la vista de inicio de sesión ("Login.cshtml") nuevamente,
                    //     mostrando los errores de validación al usuario.
                    return View();
                }
            }
            // 30. catch: Captura cualquier excepción que pueda ocurrir dentro del bloque try.
            catch
            {
                // 31. return View(ciudadano);: En caso de excepción, devuelve la vista de inicio de sesión
                //     ("Login.cshtml") nuevamente, posiblemente mostrando los datos que el usuario intentó ingresar.
                return View(ciudadano);
            }
        }

        // 1. [AllowAnonymous]: Este atributo a nivel de método indica que esta acción (CerrarSesion)
        //    puede ser accedida por usuarios no autenticados. Esto es necesario para que los usuarios
        //    puedan cerrar sesión sin necesidad de estar autenticados previamente.
        [AllowAnonymous]
        // 2. public async Task<IActionResult> CerrarSesion(): Declara un método de acción asíncrono
        //    que devuelve un IActionResult. Este método se encarga de finalizar la sesión del usuario.
        public async Task<IActionResult> CerrarSesion()
        {
            // 3. await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);:
            //    Realiza el cierre de sesión del usuario de forma asíncrona utilizando el esquema de
            //    autenticación por cookies (CookieAuthenticationDefaults.AuthenticationScheme). Esto elimina
            //    la cookie de autenticación del navegador del usuario, invalidando su sesión.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // 4. return RedirectToAction("Index", "Home");: Redirige al usuario a la acción "Index" del
            //    controlador "Home" después de cerrar la sesión. Esta suele ser la página principal del sitio.
            return RedirectToAction("Index", "Home");
        }

        // 5. [Authorize(Policy = "Admin")]: Este atributo a nivel de método indica que solo los usuarios
        //    que cumplan con la política de autorización llamada "Admin" podrán acceder a esta acción (Details).
        [Authorize(Policy = "Admin")]
        // 6. public async Task<IActionResult> Details(int? id): Declara un método de acción asíncrono
        //    que devuelve un IActionResult. Este método se utiliza para mostrar los detalles de un
        //    ciudadano específico. Recibe un parámetro 'id' de tipo entero nullable (int?), que representa
        //    el identificador del ciudadano a mostrar.
        public async Task<IActionResult> Details(int? id)
        {
            // 7. if (id == null): Comprueba si el parámetro 'id' es nulo. Esto podría ocurrir si no se
            //    proporciona un ID en la ruta.
            if (id == null)
            {
                // 8. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound (código de estado
                //    HTTP 404), indicando que el recurso solicitado no se encontró.
                return NotFound();
            }

            // 9. var ciudadano = await _context.Ciudadanos
            //    Busca de forma asíncrona en la tabla 'Ciudadanos' del contexto de la base de datos.
            var ciudadano = await _context.Ciudadanos
                // 10. .Include(c => c.Zona): Realiza una carga "eager" de la propiedad de navegación 'Zona'
                //     de la entidad Ciudadano. Esto significa que cuando se cargue el Ciudadano, también se
                //     cargarán los datos de la Zona asociada.
                .Include(c => c.Zona)
                // 11. .FirstOrDefaultAsync(m => m.Id == id);: Busca el primer Ciudadano que cumpla con la
                //     condición de que su propiedad 'Id' sea igual al 'id' recibido. FirstOrDefaultAsync
                //     devuelve el primer elemento encontrado o null si no se encuentra ninguno.
                .FirstOrDefaultAsync(m => m.Id == id);

            // 12. if (ciudadano == null): Comprueba si se encontró un Ciudadano con el 'id' proporcionado
            //     en la base de datos.
            if (ciudadano == null)
            {
                // 13. return NotFound(): Si no se encuentra ningún Ciudadano con ese 'id', devuelve un
                //     resultado NotFound.
                return NotFound();
            }

            // 14. return View(ciudadano);: Si se encontró el Ciudadano, devuelve la vista asociada a esta
            //     acción ('Details.cshtml' por convención), pasando el objeto 'ciudadano' como modelo a la vista.
            //     La vista mostrará los detalles del ciudadano.
            return View(ciudadano);
        }

        // 15. [AllowAnonymous]: Este atributo a nivel de método indica que esta acción (GetMunicipiosFromDepartamentoId)
        //     puede ser accedida por usuarios no autenticados. Esto es útil para cargar dinámicamente los municipios
        //     en un formulario sin requerir autenticación.
        [AllowAnonymous]
        // 16. public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId): Declara un método de
        //     acción público que devuelve un JsonResult. Este método se utiliza para obtener una lista de
        //     municipios basada en el ID de un departamento. Recibe un parámetro 'departamentoId' de tipo entero.
        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            // 17. return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());:
            //     Realiza una consulta LINQ a la tabla Municipios del contexto _context. Filtra los municipios
            //     cuya propiedad 'IdDepartamento' coincida con el 'departamentoId' recibido. Luego, convierte
            //     el resultado a una lista y lo devuelve como un objeto JSON. Esto permite que el cliente
            //     (generalmente JavaScript en la vista) consuma esta lista de municipios.
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        // 18. [AllowAnonymous]: Similar a la acción anterior, permite el acceso no autenticado para obtener
        //     dinámicamente los distritos.
        [AllowAnonymous]
        // 19. public JsonResult GetDistritosFromMunicipioId(int municipioId): Declara un método de acción
        //     público que devuelve un JsonResult. Este método obtiene una lista de distritos basada en el
        //     ID de un municipio. Recibe un parámetro 'municipioId' de tipo entero.
        public JsonResult GetDistritosFromMunicipioId(int municipioId)
        {
            // 20. return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());:
            //     Realiza una consulta LINQ a la tabla Distritos del contexto _context. Filtra los distritos
            //     cuya propiedad 'IdMunicipio' coincida con el 'municipioId' recibido. Convierte el resultado
            //     a una lista y lo devuelve como un objeto JSON.
            return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());
        }

        // 21. [AllowAnonymous]: Permite el acceso no autenticado para obtener dinámicamente las zonas.
        [AllowAnonymous]
        // 22. public JsonResult GetZonasFromDistritoId(int distritoId): Declara un método de acción público
        //     que devuelve un JsonResult. Este método obtiene una lista de zonas basada en el ID de un distrito.
        //     Recibe un parámetro 'distritoId' de tipo entero.
        public JsonResult GetZonasFromDistritoId(int distritoId)
        {
            // 23. return Json(_context.Zonas.Where(m => m.IdDistrito == distritoId).ToList());:
            //     Realiza una consulta LINQ a la tabla Zonas del contexto _context. Filtra las zonas cuya
            //     propiedad 'IdDistrito' coincida con el 'distritoId' recibido. Convierte el resultado a
            //     una lista y lo devuelve como un objeto JSON.
            return Json(_context.Zonas.Where(m => m.IdDistrito == distritoId).ToList());
        }

        // 24. [AllowAnonymous]: Permite el acceso no autenticado a la página de creación de un nuevo ciudadano.
        [AllowAnonymous]
        // 25. public IActionResult Create(): Declara un método de acción público que devuelve un IActionResult.
        //     Este método se utiliza para mostrar el formulario de creación de un nuevo ciudadano.
        public IActionResult Create()
        {
            // 26. List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];:
            //     Crea una nueva lista de objetos Zona y agrega un elemento inicial con un nombre "SELECCIONAR"
            //     y un ID de 0. Esto se utiliza para la opción por defecto en un desplegable.
            List<Zona> zonas = new List<Zona> { new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 } };
            // 27. List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];:
            //     Crea una nueva lista de objetos Distrito y agrega un elemento inicial "SELECCIONAR" con ID 0.
            List<Distrito> distritos = new List<Distrito> { new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 } };
            // 28. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];:
            //     Crea una nueva lista de objetos Municipio y agrega un elemento inicial "SELECCIONAR" con ID 0.
            List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
            // 29. var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();:
            //     Consulta la tabla Departamentos y obtiene todos los departamentos cuyo ID no sea 1,
            //     convirtiéndolos a una lista. Se excluye un departamento específico.
            var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
            // 30. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });:
            //     Agrega un nuevo objeto Departamento a la lista de departamentos con el nombre "SELECCIONAR"
            //     y un ID de 0, para usarlo como opción por defecto en el desplegable.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            // 31. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);:
            //     Crea un objeto SelectList a partir de la lista de municipios para usar en un control
            //     desplegable en la vista. "Id" será el valor, "Nombre" el texto visible y 0 el valor preseleccionado.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // 32. ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);:
            //     Crea un SelectList para los distritos con la misma lógica que para los municipios.
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            // 33. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);:
            //     Crea un SelectList para los departamentos con la misma lógica.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // 34. ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", 0);:
            //     Crea un SelectList para las zonas con la misma lógica.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", 0);
            // 35. return View(new Ciudadano());: Devuelve la vista asociada a la acción "Create"
            //     ("Create.cshtml" por convención), pasando una nueva instancia vacía del modelo Ciudadano
            //     para que el formulario pueda enlazar sus campos a las propiedades de este modelo.
            return View(new Ciudadano());
        }

        // 1. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //    Esto significa que se espera que un formulario HTML envíe datos a esta URL utilizando el método POST.
        [HttpPost]
        // 2. [ValidateAntiForgeryToken]: Este atributo habilita la protección contra la falsificación de
        //    solicitudes entre sitios (CSRF). Cuando se genera un formulario en la vista, se incluye un
        //    token antifalsificación. Este atributo asegura que la solicitud POST entrante contenga el
        //    mismo token, verificando que la solicitud se originó desde el sitio web y no desde un sitio malicioso.
        [ValidateAntiForgeryToken]
        // 3. [AllowAnonymous]: Este atributo a nivel de método indica que esta acción (Create) puede ser
        //    accedida por usuarios no autenticados. Esto es necesario para permitir que nuevos ciudadanos
        //    se registren en el sistema sin necesidad de haber iniciado sesión previamente.
        [AllowAnonymous]
        // 4. public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Dui,Correo,Password,ConfirmarPassword,ZonaId,Notificacion")] Ciudadano ciudadano):
        //    Declara un método de acción asíncrono que devuelve un IActionResult. Este método se encarga
        //    de procesar el formulario de creación de un nuevo ciudadano.
        //    El atributo [Bind] especifica qué propiedades del objeto Ciudadano se deben llenar con los
        //    datos recibidos en la solicitud POST. Esto ayuda a prevenir el "over-posting" de datos.
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Dui,Correo,Password,ConfirmarPassword,ZonaId,Notificacion")] Ciudadano ciudadano)
        {
            // 5. try: Inicia un bloque try para envolver el código que podría generar excepciones durante
            //    el proceso de creación del ciudadano.
            try
            {
                // 6. if (ciudadano.Notificacion.Latitud == null || ...): Realiza una validación para
                //    asegurar que las coordenadas de latitud y longitud de la notificación del ciudadano
                //    hayan sido proporcionadas y no sean valores nulos o cero.
                if (ciudadano.Notificacion.Latitud == null ||
                    ciudadano.Notificacion.Latitud == 0 ||
                    ciudadano.Notificacion.Longitud == null ||
                    ciudadano.Notificacion.Longitud == 0)
                    // 7. throw new Exception("Debe seleccionar su ubicación");: Si las coordenadas no son válidas,
                    //    lanza una excepción con un mensaje indicando que el usuario debe seleccionar su ubicación.
                    throw new Exception("Debe seleccionar su ubicación");
                // 8. ciudadano.Password = GenerarHash256(ciudadano.Password);: Llama a un método (se asume
                //    que existe en otra parte del código) llamado GenerarHash256 para calcular el hash SHA256
                //    de la contraseña proporcionada por el usuario. La contraseña se almacena de forma segura
                //    en la base de datos en su forma hasheada.
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                // 9. _context.Ciudadanos.Add(ciudadano);: Agrega la entidad 'ciudadano' al contexto de la
                //    base de datos, marcándola para ser insertada en la tabla Ciudadanos al guardar los cambios.
                _context.Ciudadanos.Add(ciudadano);
                // 10. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios realizados
                //     en el contexto (en este caso, la adición del nuevo ciudadano) en la base de datos.
                await _context.SaveChangesAsync();

                // 11. var ciudadanoId = _context.Ciudadanos.FirstOrDefault(s => s.Dui == ciudadano.Dui).Id;:
                //     Después de guardar el ciudadano, realiza una consulta a la base de datos para obtener el
                //     ID del ciudadano recién creado, basándose en su número de DUI. FirstOrDefault devuelve el
                //     primer elemento que cumple la condición o null si no se encuentra ninguno. Se asume que
                //     el DUI es único.
                var ciudadanoId = _context.Ciudadanos.FirstOrDefault(s => s.Dui == ciudadano.Dui).Id;

                // 12. ciudadano.Notificacion.IdCiudadano = ciudadanoId;: Asigna el ID del ciudadano recién
                //     creado a la propiedad IdCiudadano del objeto Notificacion asociado. Esto establece la
                //     relación entre el ciudadano y su información de notificación.
                ciudadano.Notificacion.IdCiudadano = ciudadanoId;
                // 13. _context.NotificacionesUbicaciones.Add(ciudadano.Notificacion);: Agrega la entidad
                //     'ciudadano.Notificacion' al contexto de la base de datos, marcándola para ser insertada
                //     en la tabla NotificacionesUbicaciones.
                _context.NotificacionesUbicaciones.Add(ciudadano.Notificacion);
                // 14. await _context.SaveChangesAsync();: Guarda de forma asíncrona los cambios realizados en
                //     el contexto (en este caso, la adición de la información de notificación del ciudadano)
                //     en la base de datos.
                await _context.SaveChangesAsync();

                // 15. return RedirectToAction(nameof(Login));: Si el proceso de creación del ciudadano y su
                //     información de notificación se completa con éxito, redirige al usuario a la acción "Login"
                //     de este controlador para que pueda iniciar sesión con sus nuevas credenciales.
                return RedirectToAction(nameof(Login));
            }
            // 16. catch: Captura cualquier excepción que pueda ocurrir dentro del bloque try.
            catch
            {
                // 17. List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];:
                //     Crea una nueva lista de objetos Zona para el desplegable en la vista, incluyendo una opción por defecto.
                List<Zona> zonas = new List<Zona> { new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 } };
                // 18. List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];:
                //     Crea una nueva lista de objetos Distrito para el desplegable, incluyendo una opción por defecto.
                List<Distrito> distritos = new List<Distrito> { new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 } };
                // 19. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];:
                //     Crea una nueva lista de objetos Municipio para el desplegable, incluyendo una opción por defecto.
                List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
                // 20. var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();:
                //     Obtiene una lista de departamentos desde la base de datos, excluyendo el departamento con ID 1.
                var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
                // 21. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });:
                //     Agrega una opción por defecto ("SELECCIONAR") a la lista de departamentos.
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                // 22. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);:
                //     Crea un objeto SelectList para los municipios para usar en un desplegable en la vista,
                //     con "Id" como valor, "Nombre" como texto visible y 0 como valor preseleccionado.
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                // 23. ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);:
                //     Crea un SelectList para los distritos con la misma lógica.
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                // 24. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);:
                //     Crea un SelectList para los departamentos con la misma lógica.
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                // 25. ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);:
                //     Crea un SelectList para las zonas, preseleccionando la ZonaId del ciudadano que intentó crear.
                ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);
                // 26. return View(ciudadano);: Devuelve la vista asociada a la acción "Create" ("Create.cshtml"),
                //     pasando el objeto 'ciudadano' original a la vista. Esto permite mostrar los datos que el
                //     usuario intentó ingresar y cualquier mensaje de error.
                return View(ciudadano);
            }
        }

        // 27. [Authorize(Roles = "Ciudadano")]: Este atributo a nivel de método indica que solo los usuarios
        //     que tengan asignado el rol de "Ciudadano" podrán acceder a esta acción (Perfil).
        [Authorize(Roles = "Ciudadano")]
        // 28. public async Task<IActionResult> Perfil(int? id): Declara un método de acción asíncrono
        //     que devuelve un IActionResult. Este método se utiliza para mostrar el perfil de un ciudadano
        //     específico. Recibe un parámetro 'id' de tipo entero nullable (int?), que representa el
        //     identificador del ciudadano cuyo perfil se va a mostrar.
        public async Task<IActionResult> Perfil(int? id)
        {
            // 29. if (id == null): Comprueba si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 30. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound.
                return NotFound();
            }

            // 31. var datosCiudadano = await _context.Ciudadanos.FindAsync(id);: Busca de forma asíncrona
            //     la entidad Ciudadano en la base de datos utilizando su clave primaria ('Id'). FindAsync
            //     es una forma eficiente de buscar por clave primaria.
            var datosCiudadano = await _context.Ciudadanos.FindAsync(id);
            // 32. if (datosCiudadano == null): Comprueba si se encontró un ciudadano con el 'id' proporcionado.
            if (datosCiudadano == null)
            {
                // 33. return NotFound(): Si no se encuentra ningún ciudadano con ese 'id', devuelve NotFound.
                return NotFound();
            }

            // 34. var zonas = _context.Zonas.ToList();: Obtiene una lista de todas las zonas desde la base de datos.
            var zonas = _context.Zonas.ToList();
            // 35. ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre");: Crea un objeto SelectList
            //     a partir de la lista de zonas para usar en un desplegable en la vista del perfil, permitiendo
            //     posiblemente la edición de la zona del ciudadano. "Id" será el valor y "Nombre" el texto visible.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre");

            // 36. return View(datosCiudadano);: Devuelve la vista asociada a la acción "Perfil"
            //     ("Perfil.cshtml" por convención), pasando el objeto 'datosCiudadano' como modelo a la vista.
            //     La vista mostrará la información del perfil del ciudadano.
            return View(datosCiudadano);
        }

        // 1. [Authorize(Roles = "Ciudadano")]: Este atributo a nivel de método indica que solo los usuarios
        //    que tengan asignado el rol de "Ciudadano" podrán acceder a esta acción (Edit).
        [Authorize(Roles = "Ciudadano")]
        // 2. public async Task<IActionResult> Edit(int? id): Declara un método de acción asíncrono
        //    que devuelve un IActionResult. Este método se utiliza para mostrar el formulario de edición
        //    del perfil de un ciudadano específico. Recibe un parámetro 'id' de tipo entero nullable (int?),
        //    que representa el identificador del ciudadano a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 3. if (id == null): Comprueba si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 4. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound.
                return NotFound();
            }

            // 5. var ciudadano = await _context.Ciudadanos.FindAsync(id);: Busca de forma asíncrona
            //    la entidad Ciudadano en la base de datos utilizando su clave primaria ('Id').
            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            // 6. if (ciudadano == null): Comprueba si se encontró un ciudadano con el 'id' proporcionado.
            if (ciudadano == null)
            {
                // 7. return NotFound(): Si no se encuentra ningún ciudadano con ese 'id', devuelve NotFound.
                return NotFound();
            }
            // 8. List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];:
            //    Crea una nueva lista de objetos Zona para el desplegable en la vista de edición, incluyendo una opción por defecto.
            List<Zona> zonas = new List<Zona> { new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 } };
            // 9. List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];:
            //    Crea una nueva lista de objetos Distrito para el desplegable, incluyendo una opción por defecto.
            List<Distrito> distritos = new List<Distrito> { new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 } };
            // 10. List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];:
            //     Crea una nueva lista de objetos Municipio para el desplegable, incluyendo una opción por defecto.
            List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
            // 11. var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();:
            //     Obtiene una lista de departamentos desde la base de datos, excluyendo el departamento con ID 1.
            var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
            // 12. departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });:
            //     Agrega una opción por defecto ("SELECCIONAR") a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // 13. var zona = _context.Zonas.FirstOrDefault(s => s.Id == ciudadano.ZonaId);:
            //     Busca la zona actual del ciudadano en la base de datos.
            var zona = _context.Zonas.FirstOrDefault(s => s.Id == ciudadano.ZonaId);
            // 14. zonas.Add(new Zona { Nombre = zona.Nombre, Id = zona.Id, IdAlcaldia = zona.IdAlcaldia, IdDistrito = zona.IdDistrito, Descripcion = zona.Descripcion });:
            //     Agrega la zona actual del ciudadano a la lista de zonas para que esté disponible en el desplegable.
            zonas.Add(new Zona { Nombre = zona.Nombre, Id = zona.Id, IdAlcaldia = zona.IdAlcaldia, IdDistrito = zona.IdDistrito, Descripcion = zona.Descripcion });

            // 15. ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);:
            //     Crea un SelectList para los municipios.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // 16. ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);:
            //     Crea un SelectList para los distritos.
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            // 17. ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);:
            //     Crea un SelectList para los departamentos.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // 18. ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);:
            //     Crea un SelectList para las zonas, preseleccionando la ZonaId del ciudadano.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);
            // 19. return View(ciudadano);: Devuelve la vista asociada a la acción "Edit" ("Edit.cshtml"),
            //     pasando el objeto 'ciudadano' como modelo a la vista para que se muestren los datos actuales.
            return View(ciudadano);
        }

        // 20. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //     Esto significa que se espera que un formulario HTML envíe datos a esta URL utilizando el método POST.
        [HttpPost]
        // 21. [ValidateAntiForgeryToken]: Este atributo habilita la protección contra la falsificación de
        //     solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // 22. [Authorize(Roles = "Ciudadano")]: Este atributo a nivel de método indica que solo los usuarios
        //     que tengan asignado el rol de "Ciudadano" podrán acceder a esta acción (Edit).
        [Authorize(Roles = "Ciudadano")]
        // 23. public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Correo,ZonaId")] Ciudadano ciudadano):
        //     Declara un método de acción asíncrono que devuelve un IActionResult. Este método se encarga
        //     de procesar el formulario de edición del perfil de un ciudadano.
        //     El atributo [Bind] especifica qué propiedades del objeto Ciudadano se deben llenar con los
        //     datos recibidos en la solicitud POST, limitando los campos que se pueden modificar.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Correo,ZonaId")] Ciudadano ciudadano)
        {
            // 24. if (id != ciudadano.Id): Comprueba si el 'id' recibido en la ruta o consulta coincide
            //     con el 'Id' del objeto 'ciudadano' que se intentó enlazar desde el formulario.
            if (id != ciudadano.Id)
            {
                // 25. return NotFound(): Si los IDs no coinciden, devuelve un resultado NotFound, indicando
                //     que ha habido una inconsistencia en la solicitud.
                return NotFound();
            }

            // 26. var ciudadanoUpdate = _context.Ciudadanos.FirstOrDefault(s => s.Id == ciudadano.Id);:
            //     Busca la entidad Ciudadano existente en la base de datos cuyo 'Id' coincida con el 'Id'
            //     del objeto 'ciudadano' recibido. Se utiliza FirstOrDefault para obtener el primer elemento
            //     que cumpla la condición.
            var ciudadanoUpdate = _context.Ciudadanos.FirstOrDefault(s => s.Id == ciudadano.Id);
            // 27. try: Inicia un bloque try para envolver el código que podría generar excepciones durante
            //     la actualización del ciudadano.
            try
            {
                // 28. ciudadanoUpdate.Nombre = ciudadano.Nombre;: Actualiza la propiedad 'Nombre' de la
                //     entidad existente con el valor del objeto 'ciudadano' recibido.
                ciudadanoUpdate.Nombre = ciudadano.Nombre;
                // 29. ciudadanoUpdate.Apellido = ciudadano.Apellido;: Actualiza la propiedad 'Apellido'.
                ciudadanoUpdate.Apellido = ciudadano.Apellido;
                // 30. ciudadanoUpdate.Correo = ciudadano.Correo;: Actualiza la propiedad 'Correo'.
                ciudadanoUpdate.Correo = ciudadano.Correo;
                // 31. ciudadanoUpdate.ZonaId = ciudadano.ZonaId;: Actualiza la propiedad 'ZonaId'.
                ciudadanoUpdate.ZonaId = ciudadano.ZonaId;

                // 32. _context.Update(ciudadanoUpdate);: Marca la entidad 'ciudadanoUpdate' en el contexto
                //     como modificada. Entity Framework Core rastreará los cambios realizados en esta entidad.
                _context.Update(ciudadanoUpdate);
                // 33. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios realizados
                //     en el contexto en la base de datos.
                await _context.SaveChangesAsync();
                // 34. return RedirectToAction("Menu", "Ciudadano");: Si la actualización fue exitosa, redirige
                //     al usuario a la acción "Menu" del controlador "Ciudadano".
                return RedirectToAction("Menu", "Ciudadano");
            }
            // 35. catch (DbUpdateConcurrencyException): Captura la excepción DbUpdateConcurrencyException,
            //     que ocurre cuando se detecta un conflicto de concurrencia al intentar guardar los cambios
            //     en la base de datos (otro usuario ha modificado la misma entidad).
            catch (DbUpdateConcurrencyException)
            {
                // 36. List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];:
                //     Crea listas para los desplegables en caso de error.
                List<Zona> zonas = new List<Zona> { new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 } };
                List<Distrito> distritos = new List<Distrito> { new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 } };
                List<Municipio> municipios = new List<Municipio> { new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 } };
                var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

                var zona = _context.Zonas.FirstOrDefault(s => s.Id == ciudadano.ZonaId);
                zonas.Add(new Zona { Nombre = zona.Nombre, Id = zona.Id, IdAlcaldia = zona.IdAlcaldia, IdDistrito = zona.IdDistrito, Descripcion = zona.Descripcion });

                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);

                // 37. if (!CiudadanoExists(ciudadano.Id)): Llama a un método (que debes implementar) para
                //     verificar si el Ciudadano con el 'id' proporcionado todavía existe en la base de datos.
                if (!CiudadanoExists(ciudadano.Id))
                {
                    // 38. return NotFound(): Si el Ciudadano ya no existe, devuelve NotFound.
                    return NotFound();
                }
                // 39. else: Si el Ciudadano todavía existe, significa que hubo un conflicto de concurrencia.
                else
                {
                    // 40. return View(ciudadano);: Vuelve a mostrar la vista de edición con los datos del
                    //     ciudadano, permitiendo al usuario intentar guardar los cambios nuevamente. Podrías
                    //     agregar un mensaje de error de concurrencia a la vista.
                    return View(ciudadano);
                }
            }
        }

        // 1. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //    Esto significa que se espera que una petición se envíe a esta URL utilizando el método POST.
        [HttpPost]
        // 2. [Authorize(Roles = "Ciudadano")]: Este atributo a nivel de método indica que solo los usuarios
        //    que tengan asignado el rol de "Ciudadano" podrán acceder a esta acción (Delete). Esto asegura
        //    que solo los ciudadanos autenticados puedan eliminar su propia cuenta.
        [Authorize(Roles = "Ciudadano")]
        // 3. public async Task<IActionResult> Delete(int id): Declara un método de acción asíncrono que
        //    devuelve un IActionResult. Este método se encarga de procesar la solicitud de eliminación
        //    de un ciudadano específico, identificado por su 'id'.
        public async Task<IActionResult> Delete(int id)
        {
            // 4. var ciudadano = await _context.Ciudadanos.FindAsync(id);: Busca de forma asíncrona la
            //    entidad Ciudadano en la base de datos utilizando su clave primaria ('Id'). FindAsync es
            //    una forma eficiente de buscar por clave primaria.
            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            // 5. if (ciudadano != null): Comprueba si se encontró un ciudadano con el 'id' proporcionado
            //    en la base de datos.
            if (ciudadano != null)
            {
                // 6. _context.Ciudadanos.Remove(ciudadano);: Marca la entidad 'ciudadano' para ser eliminada
                //    del contexto de la base de datos. Los cambios no se guardan en la base de datos hasta
                //    que se llama a SaveChangesAsync().
                _context.Ciudadanos.Remove(ciudadano);
                // 7. await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);:
                //    Realiza el cierre de sesión del usuario de forma asíncrona utilizando el esquema de
                //    autenticación por cookies. Esto elimina la cookie de autenticación del navegador del
                //    usuario, invalidando su sesión inmediatamente después de eliminar su cuenta.
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // 8. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios realizados
                //    en el contexto (en este caso, la eliminación del ciudadano) en la base de datos.
                await _context.SaveChangesAsync();
                // 9. return Json(new { redirectToUrl = Url.Action("Index", "Home") });: Devuelve una respuesta
                //    JSON. Esto es útil para solicitudes AJAX donde se espera una respuesta en formato JSON.
                //    En este caso, la respuesta contiene un objeto anónimo con una propiedad 'redirectToUrl'
                //    que contiene la URL de la acción "Index" del controlador "Home". Esto indica al cliente
                //    (probablemente JavaScript) a dónde redirigir al usuario después de la eliminación exitosa.
                return Json(new { redirectToUrl = Url.Action("Index", "Home") });
            }
            // 10. return RedirectToAction("Menu", "Ciudadano");: Si no se encontró el ciudadano con el 'id'
            //     proporcionado (lo cual podría indicar un error o que ya fue eliminado), redirige al usuario
            //     a la acción "Menu" del controlador "Ciudadano". Sin embargo, dado que el usuario debería
            //     estar cerrando su propia cuenta, esta redirección podría no ser la más apropiada en todos
            //     los escenarios y podría requerir una lógica de manejo de errores más específica.
            return RedirectToAction("Menu", "Ciudadano");
        }

        // 11. private bool CiudadanoExists(int id): Declara un método privado que devuelve un valor booleano.
        //     Este método se utiliza internamente para verificar si un Ciudadano existe en la base de datos
        //     por su 'Id'.
        private bool CiudadanoExists(int id)
        {
            // 12. return _context.Ciudadanos.Any(e => e.Id == id);: Utiliza el método LINQ 'Any' para
            //     verificar si existe alguna entidad Ciudadano en la tabla 'Ciudadanos' del contexto cuya
            //     propiedad 'Id' coincida con el 'id' proporcionado. Devuelve 'true' si existe al menos
            //     un Ciudadano con ese 'id', y 'false' en caso contrario.
            return _context.Ciudadanos.Any(e => e.Id == id);
        }

        // 13. public static string GenerarHash256(string input): Declara un método público y estático
        //     que devuelve una cadena (string). Este método toma una cadena 'input' como parámetro y
        //     genera su hash SHA256.
        public static string GenerarHash256(string input)
        {
            // 14. using (SHA256 sha256Hash = SHA256.Create()): Crea una instancia del algoritmo de hash
            //     SHA256 dentro de un bloque 'using'. El bloque 'using' asegura que los recursos (en este
            //     caso, el objeto SHA256) se liberen correctamente una vez que se complete el bloque.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 15. byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));: Convierte la
                //     cadena de entrada 'input' a una secuencia de bytes utilizando la codificación UTF-8.
                //     Luego, calcula el hash SHA256 de esos bytes utilizando el método 'ComputeHash' del
                //     objeto SHA256. El resultado es un array de bytes que representa el hash.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // 16. StringBuilder builder = new StringBuilder();: Crea una instancia de la clase
                //     StringBuilder, que es más eficiente para construir cadenas mediante la concatenación
                //     repetida, especialmente dentro de bucles.
                StringBuilder builder = new StringBuilder();
                // 17. for (int i = 0; i < bytes.Length; i++): Itera a través de cada byte del array de
                //     bytes que contiene el hash.
                for (int i = 0; i < bytes.Length; i++)
                {
                    // 18. builder.Append(bytes[i].ToString("x2"));: Convierte cada byte a su representación
                    //     hexadecimal en formato de dos caracteres (por ejemplo, "0a", "ff"). La "x2" especifica
                    //     que se utilicen letras minúsculas para los dígitos hexadecimales (a-f) y que se asegure
                    //     que cada byte se represente con dos caracteres (agregando un cero inicial si es necesario).
                    //     Luego, agrega esta representación hexadecimal al StringBuilder.
                    builder.Append(bytes[i].ToString("x2"));
                }
                // 19. return builder.ToString();: Convierte el contenido del StringBuilder a una cadena y la
                //     devuelve. Esta cadena es la representación hexadecimal del hash SHA256 de la cadena de
                //     entrada original.
                return builder.ToString();
            }
        }

        // 20. [AllowAnonymous]: Este atributo a nivel de método indica que esta acción (SaveLocation)
        //     puede ser accedida por usuarios no autenticados. Esto es útil para permitir que los usuarios
        //     seleccionen su ubicación durante el registro o en otros momentos sin necesidad de estar logueados.
        [AllowAnonymous]
        // 21. public IActionResult SaveLocation(Ciudadano ciudadano): Declara un método de acción público
        //     que devuelve un IActionResult. Este método recibe un objeto Ciudadano como parámetro.
        public IActionResult SaveLocation(Ciudadano ciudadano)
        {
            // 22. if (ciudadano.Notificacion == null): Comprueba si la propiedad Notificacion del objeto
            //     ciudadano es nula.
            if (ciudadano.Notificacion == null)
                // 23. ciudadano.Notificacion = new NotificacionesUbicacion();: Si la propiedad Notificacion
                //     es nula, crea una nueva instancia de la clase NotificacionesUbicacion y la asigna a
                //     la propiedad Notificacion del objeto ciudadano.
                ciudadano.Notificacion = new NotificacionesUbicacion();

            // 24. ciudadano.Notificacion.Titulo = "Ubicacion";: Asigna el valor "Ubicacion" a la propiedad
            //     Titulo del objeto Notificacion.
            ciudadano.Notificacion.Titulo = "Ubicacion";
            // 25. ciudadano.Notificacion.Estado = 1;: Asigna el valor 1 a la propiedad Estado del objeto
            //     Notificacion.
            ciudadano.Notificacion.Estado = 1;
            // 26. ciudadano.Notificacion.DistanciaMetros = 0;: Asigna el valor 0 a la propiedad DistanciaMetros
            //     del objeto Notificacion.
            ciudadano.Notificacion.DistanciaMetros = 0;
            // 27. ciudadano.Notificacion.Longitud = 0;: Asigna el valor 0 a la propiedad Longitud del objeto
            //     Notificacion. Es probable que este valor se actualice posteriormente con la ubicación real.
            ciudadano.Notificacion.Longitud = 0;
            // 28. ciudadano.Notificacion.Latitud = 0;: Asigna el valor 0 a la propiedad Latitud del objeto
            //     Notificacion. Es probable que este valor se actualice posteriormente con la ubicación real.
            ciudadano.Notificacion.Latitud = 0;

            // 29. return PartialView("_SelectUbication", ciudadano.Notificacion);: Devuelve una vista parcial
            //     llamada "_SelectUbication", pasando el objeto Notificacion del ciudadano como modelo a la
            //     vista parcial. Las vistas parciales son útiles para renderizar porciones de HTML dentro de
            //     una vista principal, a menudo utilizadas en escenarios de AJAX para actualizar partes específicas
            //     de la página. Se asume que la vista parcial "_SelectUbication.cshtml" contiene la lógica para
            //     permitir al usuario seleccionar su ubicación.
            return PartialView("_SelectUbication", ciudadano.Notificacion);
        }
    }
}
