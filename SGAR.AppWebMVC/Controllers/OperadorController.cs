using System;
using System.Collections;
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
using Microsoft.SqlServer.Server;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Alcaldia, Operador")]
    public class OperadorController : Controller
    {
        private readonly SgarDbContext _context;

        public OperadorController(SgarDbContext context)
        {
            _context = context;
        }

        public IActionResult Menu()
        {
            // Declara un método síncrono llamado Menu que devuelve un IActionResult.
            // Este método probablemente se utiliza para mostrar la vista del menú principal de la aplicación.

            return View();
            // Devuelve la vista predeterminada asociada a la acción "Menu".
            // Se espera que esta vista contenga la estructura y los enlaces del menú principal de la aplicación,
            // permitiendo a los usuarios navegar a diferentes secciones o funcionalidades.
        }


        [AllowAnonymous]
        public IActionResult Login()
        {
            // Declara un método síncrono llamado Login que devuelve un IActionResult.
            // El atributo [AllowAnonymous] indica que este método puede ser accedido por usuarios no autenticados.
            // Este método probablemente se utiliza para mostrar la página de inicio de sesión de la aplicación.

            return View();
            // Devuelve la vista predeterminada asociada a la acción "Login".
            // Se espera que esta vista contenga el formulario para que los usuarios ingresen sus credenciales (nombre de usuario y contraseña)
            // y se autentiquen en la aplicación.
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(Operador operador)
        {
            // Declara un método asíncrono llamado Login que devuelve un IActionResult.
            // El atributo [AllowAnonymous] indica que este método puede ser accedido por usuarios no autenticados.
            // El atributo [HttpPost] indica que este método responde a solicitudes HTTP POST (envío del formulario de login).
            // Recibe un objeto Operador como parámetro, que se espera que contenga las credenciales del usuario (correo y contraseña).

            try
            {
                // Bloque try para envolver el código que puede generar excepciones.

                operador.Password = GenerarHash256(operador.Password);
                // Hashea la contraseña proporcionada por el usuario utilizando el método GenerarHash256.
                // Esto asegura que la contraseña se compare con la versión hasheada almacenada en la base de datos.

                var operadorAuth = await _context.Operadores.FirstOrDefaultAsync(s => s.CorreoLaboral == operador.CorreoLaboral && s.Password == operador.Password);
                // Busca en la base de datos un operador cuyo CorreoLaboral y Password (hasheado) coincidan con los proporcionados por el usuario.
                // _context.Operadores accede a la tabla de Operadores.
                // FirstOrDefaultAsync devuelve el primer operador que coincida con la condición o null si no se encuentra ninguno.
                // La búsqueda se realiza de forma asíncrona.

                if (operadorAuth != null && operadorAuth.Id > 0 && operadorAuth.CorreoLaboral == operador.CorreoLaboral)
                {
                    // Verifica si se encontró un operador autenticado (operadorAuth no es null, tiene un Id mayor que 0 y el correo coincide).
                    var claims = new[] {
                 new Claim("Id", operadorAuth.Id.ToString()),
                 new Claim("Alcaldia", operadorAuth.IdAlcaldia.ToString()),
                 new Claim("Nombre", operadorAuth.Nombre + " " + operadorAuth.Apellido),
                 new Claim(ClaimTypes.Role, operadorAuth.GetType().Name)
             };
                    // Crea una colección de Claims (afirmaciones sobre el usuario autenticado).
                    // - "Id": El ID del operador.
                    // - "Alcaldia": El ID de la alcaldía a la que pertenece el operador.
                    // - "Nombre": El nombre completo del operador.
                    // - ClaimTypes.Role: El nombre del tipo de la entidad Operador, que se utilizará como rol del usuario.

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // Crea una ClaimsIdentity a partir de la colección de claims y el esquema de autenticación por cookies.
                    // ClaimsIdentity representa la identidad del usuario.

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    // Realiza el inicio de sesión del usuario de forma asíncrona utilizando el esquema de autenticación por cookies y el principal de claims creado.
                    // HttpContext.SignInAsync crea una cookie de autenticación que se enviará al navegador del usuario.

                    return RedirectToAction("Menu", "Operador");
                    // Redirige al usuario autenticado a la acción "Menu" del controlador "Operador".
                }
                else
                {
                    // Si no se encontró un operador autenticado o las credenciales son incorrectas.
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    // Agrega un error al ModelState (estado del modelo) con un mensaje genérico de error de autenticación.
                    // ModelState se utiliza para comunicar errores a la vista.
                    return View();
                    // Devuelve la vista "Login" (sin redirigir), lo que mostrará el mensaje de error al usuario.
                }
            }
            catch
            {
                // Bloque catch que se ejecuta si ocurre alguna excepción dentro del bloque try (por ejemplo, un error de base de datos).
                return View(operador);
                // Devuelve la vista "Login" pasando el objeto 'operador' que el usuario intentó usar para iniciar sesión.
                // Esto podría ayudar a mantener los datos ingresados en el formulario.
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            // Declara un método asíncrono llamado CerrarSesion que devuelve un IActionResult.
            // El atributo [AllowAnonymous] indica que este método puede ser accedido por usuarios no autenticados.
            // Aunque parezca contradictorio, esto permite que incluso los usuarios no logueados puedan acceder a la página de cierre de sesión (aunque no tenga mucho sentido en ese caso).
            // Lo más probable es que los usuarios autenticados sean los que accedan a esta acción para cerrar su sesión.

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Realiza el cierre de sesión del usuario de forma asíncrona utilizando el esquema de autenticación por cookies.
            // HttpContext.SignOutAsync elimina la cookie de autenticación del navegador del usuario, invalidando su sesión.

            return RedirectToAction("Index", "Home");
            // Redirige al usuario a la acción "Index" del controlador "Home" después de cerrar la sesión.
            // Esta es una práctica común para llevar al usuario a la página principal o de inicio de la aplicación después de cerrar sesión.
        }

        // GET: Operador
        [Authorize(Roles = "Alcaldia")]
        public async Task<IActionResult> Index(Operador operador, int topRegistro = 10)
        {
            // Declara un método asíncrono llamado Index que devuelve un IActionResult.
            // El atributo [Authorize(Roles = "Alcaldia")] indica que solo los usuarios con el rol "Alcaldia" pueden acceder a este método.
            // Recibe un objeto Operador (que puede contener criterios de filtrado) y un entero opcional topRegistro (para limitar el número de resultados) como parámetros.

            var query = _context.Operadores.AsQueryable();
            // Inicializa una consulta IQueryable para la tabla de Operadores.
            // AsQueryable permite construir la consulta de forma dinámica antes de ejecutarla en la base de datos.

            if (!string.IsNullOrWhiteSpace(operador.CodigoOperador))
                query = query.Where(s => s.CodigoOperador.Contains(operador.CodigoOperador));
            // Si la propiedad CodigoOperador del objeto 'operador' no es nula, vacía o solo contiene espacios en blanco,
            // agrega una cláusula Where a la consulta para filtrar los operadores cuyo CodigoOperador contenga el valor proporcionado.

            if (!string.IsNullOrWhiteSpace(operador.Dui))
                query = query.Where(s => s.Dui.Contains(operador.Dui));
            // Si la propiedad Dui del objeto 'operador' no es nula, vacía o solo contiene espacios en blanco,
            // agrega una cláusula Where a la consulta para filtrar los operadores cuyo Dui contenga el valor proporcionado.

            if (topRegistro > 0)
                query = query.Take(topRegistro);
            // Si el valor de topRegistro es mayor que 0, agrega una cláusula Take a la consulta para limitar el número de resultados
            // a la cantidad especificada en topRegistro.

            query = query.OrderByDescending(s => s.Id);
            // Agrega una cláusula OrderByDescending a la consulta para ordenar los resultados de forma descendente según la propiedad Id.

            if (!(User.FindFirst("Id").Value == "1"))
                query = query.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value));
            // Si el valor del claim "Id" del usuario actual no es "1" (se asume que "1" podría ser un ID de administrador o superusuario que ve todos),
            // agrega una cláusula Where a la consulta para filtrar los operadores cuya IdAlcaldia coincida con el ID de la alcaldía del usuario actual.
            // Esto restringe la vista de los operadores a aquellos que pertenecen a la misma alcaldía del usuario logueado.

            return View(await query.ToListAsync());
            // Ejecuta la consulta construida de forma asíncrona y convierte los resultados en una lista.
            // Luego, devuelve una vista (probablemente una vista de Razor) y le pasa la lista de operadores como modelo para que se muestren en la página.
        }


        // GET: Operador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Declara un método asíncrono llamado Details que devuelve un IActionResult.
            // Recibe un entero nullable 'id' como parámetro, que representa el ID del operador a mostrar.

            if (id == null)
            {
                // Verifica si el parámetro 'id' es nulo.
                return NotFound();
                // Si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                // indicando que no se proporcionó un ID válido.
            }

            var operador = await _context.Operadores
                // Obtiene un registro de la tabla "Operadores" del contexto de la base de datos (_context).
                .Include(o => o.IdAlcaldiaNavigation)
                // Incluye los datos de la entidad relacionada "IdAlcaldiaNavigation" (Alcaldía) para el operador.
                .Include(o => o.Vehiculo)
                // Incluye los datos de la entidad relacionada "Vehiculo" para el operador.
                .FirstOrDefaultAsync(m => m.Id == id);
            // Busca de forma asíncrona el primer operador cuyo Id coincida con el 'id' proporcionado.
            // FirstOrDefaultAsync devuelve null si no se encuentra ningún operador con ese Id.

            var referentes = _context.ReferentesOperadores
                // Obtiene una colección de registros de la tabla "ReferentesOperadores".
                .Where(s => s.IdOperador == operador.Id).ToList();
            // Filtra la colección para incluir solo los referentes cuyo IdOperador coincida con el Id del operador encontrado.
            // ToList() ejecuta la consulta y convierte el resultado en una lista.

            operador.ReferentesOperador = referentes;
            // Asigna la lista de referentes obtenida a la propiedad ReferentesOperador del objeto 'operador'.
            // Esto permite acceder a los referentes directamente desde el objeto operador en la vista.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (con clave entera y valor string).

            if (operador == null)
            {
                // Vuelve a verificar si el operador es nulo (esto podría ocurrir si la búsqueda asíncrona falló).
                return NotFound();
                // Si el operador es nulo, devuelve un resultado NotFound.
            }

            if (operador.ReferentesOperador != null)
            {
                // Verifica si la propiedad ReferentesOperador del operador no es nula.
                int numItem = 1;
                // Inicializa un contador para numerar los elementos referentes.
                foreach (var item in operador.ReferentesOperador)
                {
                    // Itera sobre cada objeto referente en la lista ReferentesOperador.
                    item.NumItem = numItem;
                    // Asigna el valor actual del contador a la propiedad NumItem del objeto referente.
                    numItem++;
                    // Incrementa el contador para el siguiente elemento.
                }
                tipos.Add(1, "Personal");
                // Agrega un tipo de referente "Personal" al SortedList con la clave 1.
                tipos.Add(2, "Laboral");
                // Agrega un tipo de referente "Laboral" al SortedList con la clave 2.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                // Crea un objeto SelectList a partir del SortedList 'tipos' para ser utilizado en un dropdown list en la vista.
                // "Key" se especifica como el valor de cada opción, "Value" como el texto visible, y 1 como el valor preseleccionado (Personal).
                // ViewBag permite pasar datos dinámicamente a la vista.
            }

            return View(operador);
            // Devuelve la vista predeterminada asociada a la acción "Details", pasando el objeto 'operador' (con sus propiedades de navegación y referentes cargados) como modelo.
            // La vista utilizará este modelo para mostrar los detalles del operador, incluyendo su alcaldía, vehículo y lista de referentes con sus tipos y números de ítem.
        }

        // GET: Operador/Create
        public IActionResult Create()
        {
            // Declara un método síncrono llamado Create que devuelve un IActionResult.
            // Este método se encarga de mostrar el formulario para crear un nuevo operador.

            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Codigo");
            // Prepara una lista de vehículos para ser utilizada en un dropdown list en la vista de creación del operador.
            // - _context.Vehiculos accede a la tabla de Vehículos.
            // - "Id" se especifica como el valor de cada opción en el dropdown list.
            // - "Codigo" se especifica como el texto visible de cada opción en el dropdown list.
            // - El resultado SelectList se almacena en ViewData con la clave "VehiculoId", para que pueda ser accedido en la vista.

            return View(new Operador());
            // Devuelve la vista predeterminada asociada a la acción "Create".
            // Se crea y se pasa una nueva instancia vacía de la clase Operador como modelo a la vista.
            // Esto permite que el formulario en la vista esté fuertemente tipado al modelo Operador.
        }

        // POST: Operador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Operador operador)
        {
            // Declara un método asíncrono llamado Create que devuelve un IActionResult.
            // Recibe un objeto Operador como parámetro, que contendrá los datos del nuevo operador a crear.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (Personal y Laboral).

            try
            {
                // Bloque try para envolver el código que puede generar excepciones durante la creación del operador.

                if (operador.SolvenciaFile != null)
                {
                    // Verifica si se ha adjuntado un archivo para la solvencia.
                    operador.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile);
                    // Llama a un método asíncrono GenerarByteImage para convertir el archivo de solvencia en un array de bytes
                    // y lo asigna a la propiedad SolvenciaDoc del objeto operador.
                }

                if (operador.LicenciaFile != null)
                {
                    // Verifica si se ha adjuntado un archivo para la licencia.
                    operador.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile);
                    // Llama a GenerarByteImage para convertir el archivo de licencia en un array de bytes
                    // y lo asigna a la propiedad LicenciaDoc del objeto operador.
                }

                if (operador.AntecedentesFile != null)
                {
                    // Verifica si se ha adjuntado un archivo para los antecedentes.
                    operador.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile);
                    // Llama a GenerarByteImage para convertir el archivo de antecedentes en un array de bytes
                    // y lo asigna a la propiedad AntecedentesDoc del objeto operador.
                }

                if (operador.FotoFile != null)
                {
                    // Verifica si se ha adjuntado un archivo para la foto.
                    operador.Foto = await GenerarByteImage(operador.FotoFile);
                    // Llama a GenerarByteImage para convertir el archivo de foto en un array de bytes
                    // y lo asigna a la propiedad Foto del objeto operador.
                }
                operador.IdAlcaldia = Convert.ToInt32(User.FindFirst("Id").Value);
                // Obtiene el ID de la alcaldía del usuario actual del claim "Id" y lo asigna a la propiedad IdAlcaldia del operador.
                operador.Password = GenerarHash256(operador.Password);
                // Hashea la contraseña proporcionada por el usuario utilizando el método GenerarHash256
                // y la asigna a la propiedad Password del operador antes de guardarla en la base de datos.

                _context.Add(operador);
                // Agrega el objeto 'operador' al contexto de la base de datos para ser insertado.
                await _context.SaveChangesAsync();
                // Guarda los cambios en la base de datos de forma asíncrona, lo que inserta el nuevo operador.

                var operadorId = _context.Operadores.FirstOrDefault(s => s.CodigoOperador == operador.CodigoOperador).Id;
                // Después de guardar el operador, busca su ID en la base de datos utilizando su CodigoOperador.
                // Esto es necesario para asignar el IdOperador correcto a los referentes.

                foreach (var item in operador.ReferentesOperador)
                {
                    // Itera sobre la colección de referentes del operador (si la hay).
                    item.IdOperador = operadorId;
                    // Asigna el ID del operador recién creado a la propiedad IdOperador de cada referente.
                    _context.ReferentesOperadores.Add(item);
                    // Agrega cada referente al contexto de la base de datos para ser insertado.
                    await _context.SaveChangesAsync();
                    // Guarda los cambios en la base de datos de forma asíncrona, lo que inserta los referentes asociados al operador.
                }
                return RedirectToAction(nameof(Index));
                // Si la creación del operador y sus referentes es exitosa, redirige a la acción "Index" del mismo controlador (OperadorController).
            }
            catch
            {
                // Bloque catch que se ejecuta si ocurre alguna excepción durante el proceso de creación.
                ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Codigo", operador.VehiculoId);
                // Vuelve a cargar la lista de vehículos en ViewData para el dropdown list en la vista de creación.
                // Se mantiene la selección anterior del usuario en caso de error.

                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                // Vuelve a cargar la lista de tipos de referentes en ViewBag para el dropdown list en la vista.
                // Se selecciona por defecto "Personal".

                return View(operador);
                // Devuelve la vista "Create" con el objeto 'operador' que se intentó crear.
                // Esto permite al usuario ver los datos que ingresó y los mensajes de error (si los hay) para corregirlos.
            }
        }



        // GET: Operador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Declara un método asíncrono llamado Edit que devuelve un IActionResult.
            // Recibe un entero nullable 'id' como parámetro, que representa el ID del operador a editar.

            if (id == null)
            {
                // Verifica si el parámetro 'id' es nulo.
                return NotFound();
                // Si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                // indicando que no se proporcionó un ID válido.
            }

            var operador = await _context.Operadores.FindAsync(id);
            // Busca de forma asíncrona un operador en la base de datos utilizando el ID proporcionado.
            // FindAsync es eficiente para buscar entidades por su clave primaria.

            var referentes = _context.ReferentesOperadores
                // Obtiene una colección de registros de la tabla "ReferentesOperadores".
                .Where(s => s.IdOperador == operador.Id).ToList();
            // Filtra la colección para incluir solo los referentes cuyo IdOperador coincida con el Id del operador encontrado.
            // ToList() ejecuta la consulta y convierte el resultado en una lista.

            operador.ReferentesOperador = referentes;
            // Asigna la lista de referentes obtenida a la propiedad ReferentesOperador del objeto 'operador'.
            // Esto permite acceder a los referentes directamente desde el objeto operador en la vista de edición.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (Personal y Laboral).

            if (operador == null)
            {
                // Vuelve a verificar si el operador es nulo (esto podría ocurrir si la búsqueda asíncrona falló).
                return NotFound();
                // Si el operador es nulo, devuelve un resultado NotFound.
            }

            if (operador.ReferentesOperador != null)
            {
                // Verifica si la propiedad ReferentesOperador del operador no es nula.
                int numItem = 1;
                // Inicializa un contador para numerar los elementos referentes.
                foreach (var item in operador.ReferentesOperador)
                {
                    // Itera sobre cada objeto referente en la lista ReferentesOperador.
                    item.NumItem = numItem;
                    // Asigna el valor actual del contador a la propiedad NumItem del objeto referente.
                    numItem++;
                    // Incrementa el contador para el siguiente elemento.
                }
                tipos.Add(1, "Personal");
                // Agrega el tipo de referente "Personal" al SortedList con la clave 1.
                tipos.Add(2, "Laboral");
                // Agrega el tipo de referente "Laboral" al SortedList con la clave 2.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                // Crea un objeto SelectList a partir del SortedList 'tipos' para ser utilizado en un dropdown list en la vista de edición.
                // "Key" se especifica como el valor de cada opción, "Value" como el texto visible, y 1 como el valor preseleccionado (Personal).
                // ViewBag permite pasar datos dinámicamente a la vista.
            }

            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Codigo", operador.VehiculoId);
            // Prepara una lista de vehículos para ser utilizada en un dropdown list en la vista de edición del operador.
            // - _context.Vehiculos accede a la tabla de Vehículos.
            // - "Id" se especifica como el valor de cada opción.
            // - "Codigo" se especifica como el texto visible de cada opción.
            // - operador.VehiculoId se utiliza como el valor preseleccionado en el dropdown list,
            //   mostrando el vehículo actualmente asignado al operador.
            // - El resultado SelectList se almacena en ViewData con la clave "VehiculoId".

            return View(operador);
            // Devuelve la vista predeterminada asociada a la acción "Edit", pasando el objeto 'operador' (con sus referentes cargados y el vehículo preseleccionado) como modelo.
            // La vista utilizará este modelo para mostrar el formulario de edición con los datos actuales del operador.
        }

        // POST: Operador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Operador operador)
        {
            // Declara un método asíncrono llamado Edit que devuelve un IActionResult.
            // Recibe un entero 'id' (de la ruta) y un objeto Operador 'operador' (del cuerpo de la solicitud) como parámetros.
            // Este método se encarga de procesar la edición de un operador existente.

            if (id != operador.Id)
            {
                // Verifica si el ID proporcionado en la ruta no coincide con el ID del operador recibido en el cuerpo.
                return NotFound();
                // Si los IDs no coinciden, devuelve un resultado NotFound (código de estado HTTP 404),
                // indicando una inconsistencia en la solicitud.
            }

            var operadorUpdate = await _context.Operadores
                // Busca de forma asíncrona el operador existente en la base de datos utilizando su ID.
                .FirstOrDefaultAsync(o => o.Id == operador.Id);

            try
            {
                // Bloque try para envolver el código que puede generar excepciones durante la actualización del operador.

                operadorUpdate.Nombre = operador.Nombre;
                operadorUpdate.Apellido = operador.Apellido;
                operadorUpdate.TelefonoPersonal = operador.TelefonoPersonal;
                operadorUpdate.CorreoPersonal = operador.CorreoPersonal;
                operadorUpdate.Dui = operador.Dui;
                operadorUpdate.Ayudantes = operador.Ayudantes;
                operadorUpdate.CodigoOperador = operador.CodigoOperador;
                operadorUpdate.CorreoLaboral = operador.CorreoLaboral;
                operadorUpdate.TelefonoLaboral = operador.TelefonoLaboral;
                operadorUpdate.VehiculoId = operador.VehiculoId;
                // Actualiza las propiedades escalares del operador existente con los valores proporcionados en el objeto 'operador'.

                var solvenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.SolvenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile, solvenciaAnterior);
                // Obtiene el documento de solvencia anterior del operador y llama a GenerarByteImage para actualizarlo
                // con el nuevo archivo proporcionado (si lo hay), manteniendo el anterior si no se proporciona uno nuevo.

                var licenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.LicenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile, licenciaAnterior);
                // Similar a la solvencia, actualiza el documento de licencia.

                var antecendeAnterior = await _context.Operadores // Se corrige la propiedad
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.AntecedentesDoc).FirstOrDefaultAsync();
                operadorUpdate.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile, antecendeAnterior);
                // Similar a la solvencia y licencia, actualiza el documento de antecedentes.

                var fotoAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                operadorUpdate.Foto = await GenerarByteImage(operador.FotoFile, fotoAnterior);
                // Similar a los documentos anteriores, actualiza la foto del operador.

                var listaIds = operador.ReferentesOperador.Select(s => s.Id).ToList();
                var referentes = await _context.ReferentesOperadores.Where(s => s.IdOperador == operador.Id).Select(s => s.Id).ToListAsync();
                // Obtiene una lista de los IDs de los referentes proporcionados en el objeto 'operador'
                // y una lista de los IDs de los referentes actualmente asociados al operador en la base de datos.

                _context.Update(operadorUpdate);
                await _context.SaveChangesAsync();
                // Marca el objeto 'operadorUpdate' como modificado en el contexto y guarda los cambios en la base de datos,
                // actualizando la información principal del operador.

                var referentDel = new List<ReferentesOperador>();
                foreach (var referente in referentes)
                {
                    var existe = listaIds.FirstOrDefault(s => s == referente);
                    if (!(existe > 0))
                    {
                        var find = await _context.ReferentesOperadores.FirstOrDefaultAsync(s => s.Id == referente);
                        if (find != null)
                            referentDel.Add(find);
                    }
                    else
                    {
                        var fetch = await _context.ReferentesOperadores.FirstOrDefaultAsync(s => s.Id == referente);
                        var refe = operador.ReferentesOperador.FirstOrDefault(s => s.Id == referente);

                        fetch.Parentesco = refe.Parentesco;
                        fetch.Tipo = refe.Tipo;
                        fetch.Nombre = refe.Nombre;
                        _context.ReferentesOperadores.Update(fetch);
                        await _context.SaveChangesAsync();
                    }
                }
                // Itera sobre los IDs de los referentes existentes en la base de datos.
                // - Si un ID no se encuentra en la lista de IDs proporcionados en 'operador', se marca el referente para eliminación.
                // - Si un ID se encuentra en ambas listas, se busca el referente en la base de datos y se actualizan sus propiedades
                //   con los valores del referente correspondiente en el objeto 'operador'.

                if (referentDel.Count > 0)
                {
                    foreach (var item in referentDel)
                        _context.ReferentesOperadores.Remove(item);
                    await _context.SaveChangesAsync();
                }
                // Si hay referentes marcados para eliminación, se eliminan del contexto y se guardan los cambios en la base de datos.

                return RedirectToAction(nameof(Index));
                // Si la actualización del operador y sus referentes es exitosa, redirige a la acción "Index" del mismo controlador.
            }
            catch (DbUpdateConcurrencyException)
            {
                // Captura una excepción de concurrencia de base de datos, que ocurre cuando varios usuarios intentan modificar la misma entidad al mismo tiempo.
                if (!OperadorExists(operador.Id))
                {
                    // Verifica si el operador con el ID especificado todavía existe en la base de datos.
                    return NotFound();
                    // Si no existe, devuelve NotFound.
                }
                else
                {
                    // Si el operador existe pero hubo un conflicto de concurrencia,
                    ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Codigo", operador.VehiculoId);
                    // Vuelve a cargar la lista de vehículos en ViewData para mostrarla en la vista.
                    return View(operador);
                    // Devuelve la vista "Edit" con el objeto 'operador', permitiendo al usuario intentar la edición nuevamente,
                    // posiblemente después de recargar los datos más recientes.
                }
            }
        }

        public static string GenerarHash256(string input)
        {
            // Declara un método público estático llamado GenerarHash256 que recibe una cadena 'input' como parámetro.
            // Este método tiene como objetivo generar un hash SHA256 de la cadena de entrada y devolverlo como una cadena hexadecimal.

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Crea una instancia del algoritmo de hash SHA256 utilizando un bloque 'using'.
                // El bloque 'using' asegura que el objeto SHA256 se libere correctamente después de su uso.

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                // Convierte la cadena de entrada 'input' a un array de bytes utilizando la codificación UTF-8.
                // Luego, calcula el hash SHA256 de este array de bytes utilizando el método ComputeHash del objeto SHA256.
                // El resultado es un nuevo array de bytes que representa el hash.

                StringBuilder builder = new StringBuilder();
                // Crea una instancia de StringBuilder para construir la representación hexadecimal del hash.
                // StringBuilder es más eficiente para concatenar cadenas en bucles.

                for (int i = 0; i < bytes.Length; i++)
                {
                    // Itera sobre cada byte en el array de bytes del hash.
                    builder.Append(bytes[i].ToString("x2"));
                    // Convierte cada byte a su representación hexadecimal de dos caracteres (minúsculas) utilizando el formato "x2".
                    // Append agrega esta representación hexadecimal al StringBuilder.
                }

                return builder.ToString();
                // Convierte el contenido del StringBuilder a una cadena y la devuelve.
                // Esta cadena es la representación hexadecimal del hash SHA256 de la cadena de entrada.
            }
        }

        // GET: Operador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Declara un método asíncrono llamado Delete que devuelve un IActionResult.
            // Recibe un entero nullable 'id' como parámetro, que representa el ID del operador a eliminar.
            // Este método se encarga de mostrar una confirmación antes de eliminar el operador.

            if (id == null)
            {
                // Verifica si el parámetro 'id' es nulo.
                return NotFound();
                // Si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                // indicando que no se proporcionó un ID válido.
            }

            var operador = await _context.Operadores
                // Obtiene un registro de la tabla "Operadores" del contexto de la base de datos (_context).
                .Include(o => o.IdAlcaldiaNavigation)
                // Incluye los datos de la entidad relacionada "IdAlcaldiaNavigation" (Alcaldía) para el operador.
                .Include(o => o.Vehiculo)
                // Incluye los datos de la entidad relacionada "Vehiculo" para el operador.
                .FirstOrDefaultAsync(m => m.Id == id);
            // Busca de forma asíncrona el primer operador cuyo Id coincida con el 'id' proporcionado.
            // FirstOrDefaultAsync devuelve null si no se encuentra ningún operador con ese Id.

            var referentes = _context.ReferentesOperadores
                // Obtiene una colección de registros de la tabla "ReferentesOperadores".
                .Where(s => s.IdOperador == operador.Id).ToList();
            // Filtra la colección para incluir solo los referentes cuyo IdOperador coincida con el Id del operador encontrado.
            // ToList() ejecuta la consulta y convierte el resultado en una lista.

            operador.ReferentesOperador = referentes;
            // Asigna la lista de referentes obtenida a la propiedad ReferentesOperador del objeto 'operador'.
            // Esto permite acceder a los referentes directamente desde el objeto operador en la vista de eliminación.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (Personal y Laboral).

            if (operador == null)
            {
                // Vuelve a verificar si el operador es nulo (esto podría ocurrir si la búsqueda asíncrona falló).
                return NotFound();
                // Si el operador es nulo, devuelve un resultado NotFound.
            }

            if (operador.ReferentesOperador != null)
            {
                // Verifica si la propiedad ReferentesOperador del operador no es nula.
                int numItem = 1;
                // Inicializa un contador para numerar los elementos referentes.
                foreach (var item in operador.ReferentesOperador)
                {
                    // Itera sobre cada objeto referente en la lista ReferentesOperador.
                    item.NumItem = numItem;
                    // Asigna el valor actual del contador a la propiedad NumItem del objeto referente.
                    numItem++;
                    // Incrementa el contador para el siguiente elemento.
                }
                tipos.Add(1, "Personal");
                // Agrega el tipo de referente "Personal" al SortedList con la clave 1.
                tipos.Add(2, "Laboral");
                // Agrega el tipo de referente "Laboral" al SortedList con la clave 2.
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                // Crea un objeto SelectList a partir del SortedList 'tipos' para ser utilizado en la vista de eliminación.
                // Aunque aquí se crea un SelectList de tipos, probablemente no se utilice directamente en la vista de eliminación,
                // ya que el propósito principal de esta acción es mostrar los detalles del operador a eliminar para su confirmación.
                // La información de los tipos de referentes podría ser mostrada de otra manera en la vista.
            }

            return View(operador);
            // Devuelve la vista predeterminada asociada a la acción "Delete", pasando el objeto 'operador' (con sus propiedades de navegación y referentes cargados) como modelo.
            // La vista utilizará este modelo para mostrar los detalles del operador que se va a eliminar,
            // permitiendo al usuario confirmar si realmente desea eliminarlo.
        }

        // POST: Operador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Declara un método asíncrono llamado DeleteConfirmed que devuelve un IActionResult.
            // Recibe un entero 'id' como parámetro, que representa el ID del operador a eliminar.
            // Este método se encarga de eliminar el operador de la base de datos después de que el usuario ha confirmado la eliminación.

            var operador = await _context.Operadores.FindAsync(id);
            // Busca de forma asíncrona el operador con el ID especificado en la base de datos utilizando el método FindAsync.
            // FindAsync es eficiente para buscar por la clave primaria.

            if (operador != null)
            {
                // Verifica si se encontró un operador con el ID proporcionado.
                var horarios = await _context.Horarios.Where(s => s.IdOperador == operador.Id).ToListAsync();
                // Busca de forma asíncrona todos los horarios asociados al operador que se va a eliminar.
                // Esto se hace para evitar errores de clave externa en la base de datos.

                foreach (var horario in horarios)
                {
                    // Itera sobre cada horario encontrado para el operador.
                    _context.Horarios.Remove(horario);
                    // Marca el objeto 'horario' para ser eliminado de la base de datos.
                    await _context.SaveChangesAsync();
                    // Guarda los cambios en la base de datos de forma asíncrona, eliminando cada horario asociado.
                }
                _context.Operadores.Remove(operador);
                // Marca el objeto 'operador' para ser eliminado de la base de datos.
            }
            // Si el operador con el ID especificado no se encuentra (operador es null), no se realiza ninguna acción de eliminación.

            await _context.SaveChangesAsync();
            // Guarda todos los cambios realizados en el contexto de la base de datos de forma asíncrona.
            // Esto incluye la eliminación de los horarios asociados (si los hubo) y la eliminación del operador.

            return RedirectToAction(nameof(Index));
            // Redirige al usuario a la acción "Index" del mismo controlador (OperadorController),
            // que probablemente muestra la lista de operadores actualizada después de la eliminación.
        }

        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            // Declara un método asíncrono llamado GenerarByteImage que devuelve un array de bytes nullable (byte[]?).
            // Recibe un IFormFile nullable 'file' (representando un archivo subido) y un array de bytes nullable 'bytesImage' como parámetros.
            // Este método tiene como objetivo convertir el archivo subido a un array de bytes.
            // El parámetro 'bytesImage' permite mantener una imagen existente si no se proporciona un nuevo archivo.

            byte[]? bytes = bytesImage;
            // Inicializa una variable 'bytes' con el valor de 'bytesImage'.
            // Esto significa que si no se proporciona un nuevo archivo, se conservará la imagen existente.

            if (file != null && file.Length > 0)
            {
                // Verifica si se ha proporcionado un archivo ('file' no es nulo) y si el archivo tiene contenido (su longitud es mayor que 0).
                // Construir la ruta del archivo (Este comentario parece ser un remanente y no se utiliza en el código actual)

                using (var memoryStream = new MemoryStream())
                {
                    // Crea una instancia de MemoryStream utilizando un bloque 'using'.
                    // MemoryStream proporciona una secuencia en memoria para trabajar con datos como un flujo.
                    // El bloque 'using' asegura que el MemoryStream se libere correctamente después de su uso.

                    await file.CopyToAsync(memoryStream);
                    // Copia el contenido del archivo subido ('file') al MemoryStream de forma asíncrona.
                    // Esto lee el contenido del archivo y lo escribe en la memoria.

                    bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                                                    // Convierte el contenido del MemoryStream (que ahora contiene los datos del archivo) a un array de bytes
                                                    // y lo asigna a la variable 'bytes'.
                                                    // Si se proporcionó un nuevo archivo, 'bytes' ahora contendrá los bytes de ese nuevo archivo,
                                                    // reemplazando el valor original de 'bytesImage'.
                }
            }
            return bytes;
            // Devuelve el array de bytes.
            // Si se proporcionó un nuevo archivo, este array contendrá los bytes del nuevo archivo.
            // Si no se proporcionó un nuevo archivo, este array contendrá el valor original de 'bytesImage' (la imagen existente, o null si no había ninguna).
        }

        private bool OperadorExists(int id)
        {
            // Declara un método privado llamado OperadorExists que recibe un entero 'id' como parámetro.
            // Este método tiene como objetivo verificar si existe algún registro de Operador en la base de datos
            // cuyo Id coincida con el 'id' proporcionado.

            return _context.Operadores.Any(e => e.Id == id);
            // Utiliza el método de extensión Any() de LINQ para verificar la existencia de al menos un elemento
            // en la colección _context.Operadores que cumpla con la condición especificada en la expresión lambda (e => e.Id == id).
            // - _context.Operadores accede a la tabla de Operadores a través del contexto de la base de datos.
            // - Any() devuelve true si se encuentra al menos un operador con el Id especificado, y false en caso contrario.
            // Este método es útil para verificar la existencia de una entidad antes de intentar realizar operaciones como editar o eliminar.
        }


        public IActionResult AddReferenteOp(Operador operador)
        {
            // Declara un método síncrono llamado AddReferenteOp que devuelve un IActionResult.
            // Recibe un objeto Operador como parámetro.
            // Este método se utiliza para agregar dinámicamente un nuevo referente a la lista de referentes de un operador
            // y devuelve una vista parcial para actualizar la interfaz de usuario.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (Personal y Laboral).

            if (operador.ReferentesOperador == null)
                operador.ReferentesOperador = new List<ReferentesOperador>();
            // Verifica si la lista de referentes del operador es nula. Si lo es, crea una nueva lista vacía.

            operador.ReferentesOperador.Add(new ReferentesOperador
            {
                Nombre = "",
                NumItem = operador.ReferentesOperador.Count + 1,
                Parentesco = "",
                Tipo = 1
            });
            // Crea una nueva instancia de ReferentesOperador con valores predeterminados (nombre y parentesco vacíos,
            // NumItem basado en la cantidad actual de referentes + 1, y Tipo establecido en 1 (Personal)).
            // Este nuevo referente se agrega a la lista de ReferentesOperador del objeto 'operador'.

            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            // Crea un objeto SelectList a partir del SortedList 'tipos' para ser utilizado en un dropdown list en la vista parcial.
            // "Key" se especifica como el valor de cada opción, "Value" como el texto visible, y 1 como el valor preseleccionado (Personal).
            // ViewBag permite pasar datos dinámicamente a la vista parcial.

            return PartialView("_ReferentesOperador", operador.ReferentesOperador);
            // Devuelve una vista parcial llamada "_ReferentesOperador" pasando la lista actualizada de ReferentesOperador del objeto 'operador' como modelo.
            // PartialView es utilizado para renderizar una porción de la vista sin la estructura completa de una página.
            // Se espera que esta vista parcial contenga el HTML para mostrar la lista de referentes, incluyendo el nuevo referente agregado.
            // Este método se suele invocar mediante una solicitud AJAX para actualizar dinámicamente una sección del formulario en la página de creación o edición del operador.
        }

        public IActionResult DeleteReferenteOp(int id, Operador operador)
        {
            // Declara un método síncrono llamado DeleteReferenteOp que devuelve un IActionResult.
            // Recibe un entero 'id' (que representa el NumItem del referente a eliminar) y un objeto Operador como parámetros.
            // Este método se utiliza para eliminar dinámicamente un referente de la lista de referentes de un operador
            // y devuelve una vista parcial para actualizar la interfaz de usuario.

            var tipos = new SortedList<int, string>();
            // Inicializa una nueva SortedList para almacenar los tipos de referentes (Personal y Laboral).

            int num = id;
            // Asigna el valor del parámetro 'id' a una variable local 'num'.
            // Se asume que 'id' corresponde al NumItem del referente que se desea eliminar.

            if (operador.ReferentesOperador.Count == 0)
                operador.ReferentesOperador = new List<ReferentesOperador>();
            // Verifica si la lista de referentes del operador está vacía. Si lo está, crea una nueva lista vacía.
            // Esto podría ser una medida de seguridad para evitar errores si se intenta eliminar de una lista vacía.

            var referenteDel = operador.ReferentesOperador.FirstOrDefault(s => s.NumItem == num);
            // Busca el primer referente en la lista ReferentesOperador del operador cuyo NumItem coincida con el valor de 'num' (el ID proporcionado).

            if (referenteDel != null)
            {
                // Verifica si se encontró un referente para eliminar.
                operador.ReferentesOperador.Remove(referenteDel);
                // Elimina el referente encontrado de la lista ReferentesOperador del operador.

                int numItemNew = 1;
                // Inicializa un nuevo contador para re-numerar los referentes restantes.
                foreach (var item in operador.ReferentesOperador)
                {
                    // Itera sobre la lista actualizada de referentes.
                    item.NumItem = numItemNew;
                    // Asigna el valor actual del contador a la propiedad NumItem de cada referente.
                    numItemNew++;
                    // Incrementa el contador para el siguiente referente.
                }
                // Este bucle re-numera los referentes después de la eliminación para mantener una secuencia lógica en la interfaz de usuario.
            }

            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            // Crea un objeto SelectList a partir del SortedList 'tipos' para ser utilizado en un dropdown list en la vista parcial.
            // "Key" se especifica como el valor de cada opción, "Value" como el texto visible, y 1 como el valor preseleccionado (Personal).
            // ViewBag permite pasar datos dinámicamente a la vista parcial.

            return PartialView("_ReferentesOperador", operador.ReferentesOperador);
            // Devuelve una vista parcial llamada "_ReferentesOperador" pasando la lista actualizada de ReferentesOperador del objeto 'operador' como modelo.
            // PartialView es utilizado para renderizar una porción de la vista sin la estructura completa de una página.
            // Se espera que esta vista parcial contenga el HTML para mostrar la lista de referentes actualizada después de la eliminación y re-numeración.
            // Este método se suele invocar mediante una solicitud AJAX para actualizar dinámicamente una sección del formulario en la página de creación o edición del operador.
        }

        [HttpGet]
     
        public IActionResult GetOperadores()
        {
            // Declara un método síncrono llamado GetOperadores que devuelve un IActionResult.
            // Este método se encarga de obtener una lista simplificada de operadores de la base de datos
            // y devolverla en formato JSON.

            var operadores = _context.Operadores
                // Accede a la tabla "Operadores" a través del contexto de la base de datos (_context).
                .Select(o => new { id = o.Id, nombre = o.Nombre })
                // Proyecta los resultados en una nueva colección anónima que contiene solo las propiedades 'Id' y 'Nombre' de cada operador.
                // Esto es útil para enviar solo la información necesaria al cliente, por ejemplo, para llenar un dropdown list.
                .ToList();
            // Ejecuta la consulta a la base de datos y convierte los resultados en una lista.

            return Json(operadores);
            // Devuelve la lista de operadores en formato JSON.
            // Esto permite que una solicitud AJAX desde el cliente (probablemente una página web)
            // reciba los datos de los operadores y los utilice para actualizar dinámicamente la interfaz de usuario.
        }

    }
}
