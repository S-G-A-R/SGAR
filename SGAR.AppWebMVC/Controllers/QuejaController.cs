using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    // 1. [Authorize(Roles = "Ciudadano, Supervisor")]
    //    Este atributo de autorización a nivel de clase indica que para acceder a cualquier acción (método público)
    //    dentro de este QuejaController, el usuario debe estar autenticado y pertenecer a uno de los roles
    //    especificados: "Ciudadano" o "Supervisor".
    [Authorize(Roles = "Ciudadano, Supervisor")]
    public class QuejaController : Controller
    {
        // 2. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public QuejaController(SgarDbContext context)
        //    Este es el constructor de la clase QuejaController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public QuejaController(SgarDbContext context)
        {
            // 4. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 5. // GET: Queja
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta base del controlador "Queja" (ej. /Queja o /Queja/Index).
        // 6. [Authorize(Roles = "Ciudadano")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //    pueden acceder a esta acción Index, incluso si la autorización a nivel de clase permite también a "Supervisor".
        [Authorize(Roles = "Ciudadano")]
        // 7. public async Task<IActionResult> Index(Queja queja, int topRegistro = 10)
        //    Se define una acción asíncrona llamada Index que devuelve un IActionResult.
        //    Recibe un objeto Queja como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> Index(Queja queja, int topRegistro = 10)
        {
            // 8. var quejas = await _context.Quejas.ToListAsync();
            //    Se realiza una consulta asíncrona a la base de datos para obtener todos los registros de la tabla Quejas
            //    y se convierten en una lista (List<Queja>).
            var quejas = await _context.Quejas.ToListAsync();
            // 9. var quejasQ1 = quejas
            //         .Where(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value));
            //    Se filtra la lista de todas las quejas (quejas) utilizando LINQ para seleccionar solo aquellas
            //    cuya propiedad IdCiudadano coincida con el ID del usuario actual. Se asume que User representa
            //    el usuario autenticado y FindFirst("Id") busca la primera claim con el tipo "Id". El valor de la claim
            //    se convierte a un entero antes de la comparación. Esto asegura que los ciudadanos solo vean sus propias quejas.
            var quejasQ1 = quejas
                .Where(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value));

            // 10. var query = quejasQ1.AsQueryable();
            //     Se convierte la colección filtrada de quejas (quejasQ1) a un IQueryable<Queja>. Esto permite construir
            //     consultas LINQ de forma más flexible y diferida.
            var query = quejasQ1.AsQueryable();
            // 11. if (!string.IsNullOrWhiteSpace(queja.Titulo))
            //     Se verifica si la propiedad Titulo del objeto queja (que puede contener criterios de filtro proporcionados
            //     por el usuario a través del formulario) no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(queja.Titulo))
                // 12. query = query.Where(s => s.Titulo.Contains(queja.Titulo));
                //     Si la propiedad Titulo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     las quejas cuyo Titulo contenga el valor proporcionado.
                query = query.Where(s => s.Titulo.Contains(queja.Titulo));
            // 13. if (topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 14. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 15. query = query
            //         .Include(p => p.IdCiudadanoNavigation);
            //     Se agrega una cláusula Include a la consulta para cargar de forma eager la propiedad de navegación
            //     IdCiudadanoNavigation del objeto Queja. Esto asume que existe una relación entre Queja y Ciudadano
            //     y que IdCiudadanoNavigation permite acceder a la información del ciudadano asociado a la queja.
            query = query
                .Include(p => p.IdCiudadanoNavigation);
            // 16. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar las quejas de forma
            //     descendente según su propiedad Id, mostrando las más recientes primero.
            query = query.OrderByDescending(s => s.Id);

            // 17. return View(query.OrderByDescending(s => s.Id).ToList());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Queja>) de forma síncrona (ToList()). Esta lista de quejas se pasa a la vista
            //     asociada a la acción Index para ser mostrada al usuario.
            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // 1. [Authorize(Roles = "Supervisor")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //    pueden acceder a esta acción List.
        [Authorize(Roles = "Supervisor")]
        // 2. public async Task<IActionResult> List(Queja queja, int topRegistro = 10)
        //    Se define una acción asíncrona llamada List que devuelve un IActionResult.
        //    Recibe un objeto Queja como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> List(Queja queja, int topRegistro = 10)
        {
            // 3. var quejas = await _context.Quejas.Include(s => s.IdCiudadanoNavigation.Zona).ToListAsync();
            //    Se realiza una consulta asíncrona a la base de datos para obtener todos los registros de la tabla Quejas.
            //    Se utiliza Include para cargar de forma eager la propiedad de navegación IdCiudadanoNavigation (que representa el ciudadano que creó la queja)
            //    y, dentro de esa navegación, también se carga de forma eager la propiedad Zona (asumiendo que Ciudadano tiene una propiedad de navegación a Zona).
            //    Finalmente, los resultados se convierten en una lista (List<Queja>).
            var quejas = await _context.Quejas.Include(s => s.IdCiudadanoNavigation.Zona).ToListAsync();
            // 4. var quejasQ1 = quejas
            //         .Where(s => s.IdCiudadanoNavigation.Zona.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));
            //    Se filtra la lista de todas las quejas (quejas) utilizando LINQ para seleccionar solo aquellas
            //    donde la propiedad IdAlcaldia de la Zona del Ciudadano asociado coincida con el valor de la claim "Alcaldia"
            //    del usuario actual (se asume que User representa el supervisor autenticado y FindFirst("Alcaldia") busca la
            //    primera claim con el tipo "Alcaldia"). El valor de la claim se convierte a un entero antes de la comparación.
            //    Esto asegura que los supervisores solo vean las quejas de ciudadanos dentro de su alcaldía.
            var quejasQ1 = quejas
                .Where(s => s.IdCiudadanoNavigation.Zona.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));

            // 5. var query = quejasQ1.AsQueryable();
            //    Se convierte la colección filtrada de quejas (quejasQ1) a un IQueryable<Queja>. Esto permite construir
            //    consultas LINQ de forma más flexible y diferida.
            var query = quejasQ1.AsQueryable();
            // 6. if (!string.IsNullOrWhiteSpace(queja.Titulo))
            //    Se verifica si la propiedad Titulo del objeto queja (que puede contener criterios de filtro proporcionados
            //    por el usuario a través del formulario) no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(queja.Titulo))
                // 7. query = query.Where(s => s.Titulo.Contains(queja.Titulo));
                //    Si la propiedad Titulo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //    las quejas cuyo Titulo contenga el valor proporcionado.
                query = query.Where(s => s.Titulo.Contains(queja.Titulo));
            // 8. if (!string.IsNullOrWhiteSpace(queja.TipoSituacion))
            //    Se verifica si la propiedad TipoSituacion del objeto queja no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(queja.TipoSituacion))
                // 9. query = query.Where(s => s.TipoSituacion == queja.TipoSituacion);
                //    Si la propiedad TipoSituacion tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //    las quejas cuyo TipoSituacion coincida con el valor proporcionado.
                query = query.Where(s => s.TipoSituacion == queja.TipoSituacion);
            // 10. if (topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 11. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 12. query = query
            //         .Include(p => p.IdCiudadanoNavigation);
            //     Se agrega una cláusula Include a la consulta para cargar de forma eager la propiedad de navegación
            //     IdCiudadanoNavigation del objeto Queja. Aunque ya se incluyó la Zona del Ciudadano, esta línea asegura
            //     que la información básica del Ciudadano también esté disponible.
            query = query
                .Include(p => p.IdCiudadanoNavigation);
            // 13. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar las quejas de forma
            //     descendente según su propiedad Id, mostrando las más recientes primero.
            query = query.OrderByDescending(s => s.Id);

            // 14. return View(query.OrderByDescending(s => s.Id).ToList());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Queja>) de forma síncrona (ToList()). Esta lista de quejas se pasa a la vista
            //     asociada a la acción List para ser mostrada a los supervisores.
            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // 15. // GET: Queja/Details/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Queja/Details/{id}, donde {id} es el ID de la queja a mostrar en detalle.
        // 16. public async Task<IActionResult> Details(int? id)
        //     Se define una acción asíncrona llamada Details que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la queja a mostrar.
        public async Task<IActionResult> Details(int? id)
        {
            // 17. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 18. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 19. var queja = await _context.Quejas
            //         .Include(q => q.IdCiudadanoNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //     Se realiza una consulta asíncrona a la base de datos para obtener la queja cuyo Id coincide con el valor del parámetro id.
            //     Se utiliza Include para cargar de forma eager la propiedad de navegación IdCiudadanoNavigation (que representa el ciudadano que creó la queja).
            //     FirstOrDefaultAsync devuelve la primera queja que coincida con el predicado o null si no se encuentra ninguna.
            var queja = await _context.Quejas
                .Include(q => q.IdCiudadanoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            // 20. if (queja == null)
            //     Se verifica si la queja recuperada de la base de datos es nula (no se encontró ninguna queja con el ID proporcionado).
            if (queja == null)
            {
                // 21. return NotFound();
                //     Si queja es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 22. return View(queja);
            //     Se devuelve la vista asociada a la acción Details, pasando el objeto queja (que incluye la información del ciudadano)
            //     como modelo para que se muestren los detalles de la queja al usuario.
            return View(queja);
        }

        // 23. // GET: Queja/Create
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a la ruta /Queja/Create. Esta acción típicamente muestra un formulario para crear una nueva queja.
        // 24. [Authorize(Roles = "Ciudadano")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //     pueden acceder a esta acción Create.
        [Authorize(Roles = "Ciudadano")]
        // 25. public IActionResult Create()
        //     Se define una acción pública llamada Create que devuelve un IActionResult.
        public IActionResult Create()
        {
            // 26. return View();
            //     Se devuelve la vista asociada a la acción Create. Por convención, buscará una vista llamada Create.cshtml
            //     en la carpeta Views/Queja. Generalmente, esta vista contendrá el formulario para crear una nueva queja.
            return View();
        }

        // 1. // POST: Queja/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a la ruta /Queja/Create. Esta acción se invoca cuando se envía el formulario de creación de una queja.
        // 2. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //    Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 3. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 4. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 5. [ValidateAntiForgeryToken]
        //    Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 6. [Authorize(Roles = "Ciudadano")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //    pueden acceder a esta acción Create (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ciudadano")]
        // 7. public async Task<IActionResult> Create(Queja queja)
        //    Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //    Recibe un objeto Queja como parámetro. ASP.NET Core intentará crear e inicializar este objeto
        //    a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Create(Queja queja)
        {
            // 8. try { ... } catch { ... }
            //    Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de creación de la queja.
            try
            {
                // 9. queja.Estado = 1;
                //    Se establece la propiedad Estado de la nueva queja en 1. Esto podría representar un estado inicial (por ejemplo, "Pendiente").
                queja.Estado = 1;
                // 10. queja.Motivo = "";
                //     Se inicializa la propiedad Motivo de la nueva queja con una cadena vacía. El motivo podría ser llenado posteriormente por un supervisor.
                queja.Motivo = "";
                // 11. queja.IdCiudadano = Convert.ToInt32(User.FindFirst("Id").Value);
                //     Se obtiene el valor de la claim "Id" del usuario actual (se asume que representa el ID del ciudadano)
                //     y se convierte a un entero. Este valor se asigna a la propiedad IdCiudadano de la nueva queja,
                //     estableciendo la relación entre la queja y el ciudadano que la creó.
                queja.IdCiudadano = Convert.ToInt32(User.FindFirst("Id").Value);
                // 12. if (queja.File != null)
                //     Se verifica si la propiedad File del objeto queja (que se espera que sea un IFormFile para la carga de archivos) no es nula.
                if (queja.File != null)
                    // 13. queja.Archivo = await GenerarByteImage(queja.File);
                    //     Se llama a una función asíncrona llamada GenerarByteImage (se asume que existe en el proyecto)
                    //     que toma el archivo cargado (queja.File) y devuelve su representación como un array de bytes.
                    //     El resultado se asigna a la propiedad Archivo de la queja, permitiendo almacenar un archivo adjunto.
                    queja.Archivo = await GenerarByteImage(queja.File);

                // 14. _context.Add(queja);
                //     Se agrega el objeto queja al contexto de la base de datos (_context). Esto marca el objeto para ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.Add(queja);
                // 15. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la inserción de la nueva queja en la tabla Quejas.
                await _context.SaveChangesAsync();
                // 16. return RedirectToAction(nameof(Index));
                //     Si la creación de la queja se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Queja, que probablemente muestra la lista de quejas del ciudadano.
                return RedirectToAction(nameof(Index));
            }
            // 17. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 18. return View(queja);
                //     En caso de excepción durante el proceso de creación, se devuelve la vista asociada a la acción Create,
                //     pasando el objeto queja que se intentó crear como modelo. Esto permite mostrar los errores de validación
                //     y los datos que el usuario ya había ingresado, facilitando la corrección y reintento.
                return View(queja);
            }
        }

        // 19. // GET: Queja/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Queja/Edit/{id}, donde {id} es el ID de la queja a editar.
        //     Esta acción típicamente muestra un formulario para editar una queja existente.
        // 20. [Authorize(Roles = "Supervisor")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //     pueden acceder a esta acción Edit (GET).
        [Authorize(Roles = "Supervisor")]
        // 21. public async Task<IActionResult> Edit(int? id)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la queja a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 22. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 23. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 24. var queja = await _context.Quejas.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Quejas utilizando el método FindAsync del contexto de la base de datos
            //     para obtener la queja con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var queja = await _context.Quejas.FindAsync(id);
            // 25. if (queja == null)
            //     Se verifica si la queja recuperada de la base de datos es nula (no se encontró ninguna queja con el ID proporcionado).
            if (queja == null)
            {
                // 26. return NotFound();
                //     Si queja es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }
            // 27. return View(queja);
            //     Se devuelve la vista asociada a la acción Edit, pasando el objeto queja como modelo para que el formulario de edición pueda mostrar los datos existentes.
            return View(queja);
        }

        // 28. // POST: Queja/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a una ruta como /Queja/Edit/{id}, donde {id} es el ID de la queja a editar.
        //     Esta acción se invoca cuando se envía el formulario de edición.
        // 29. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //     Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 30. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //     Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 31. [HttpPost]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 32. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 33. [Authorize(Roles = "Supervisor")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //     pueden acceder a esta acción Edit (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        // 34. public async Task<IActionResult> Edit(int id, Queja queja)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe dos parámetros:
        //     - id: El ID de la queja a editar, que se espera que coincida con el ID en la ruta.
        //     - queja: Un objeto Queja que contiene los datos modificados del formulario, que ASP.NET Core intenta crear
        //              e inicializar a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Edit(int id, Queja queja)
        {
            // 35. if (id != queja.Id)
            //     Se verifica si el ID proporcionado en la ruta (id) no coincide con el ID de la queja que se está intentando editar (queja.Id).
            if (id != queja.Id)
            {
                // 36. return NotFound();
                //     Si los IDs no coinciden, se devuelve un resultado NotFound (código de estado HTTP 404), indicando una posible manipulación de la solicitud.
                return NotFound();
            }

            // 37. try { ... } catch (DbUpdateConcurrencyException) { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de edición de la queja,
            //     específicamente capturando DbUpdateConcurrencyException, que ocurre cuando varios usuarios intentan editar el mismo registro simultáneamente.
            try
            {
                // 38. var quejaUpdate = await _context.Quejas.FirstOrDefaultAsync(s => s.Id == queja.Id);
                //     Se realiza una consulta asíncrona a la base de datos para obtener el registro de la queja existente
                //     cuyo ID coincida con el ID de la queja que se está editando. Se utiliza FirstOrDefaultAsync para obtener el primer resultado o null si no se encuentra ninguno.
                var quejaUpdate = await _context.Quejas.FirstOrDefaultAsync(s => s.Id == queja.Id);
                // 39. quejaUpdate.Estado = queja.Estado;
                //     Se actualiza la propiedad Estado del objeto queja existente (quejaUpdate) con el valor proporcionado en el objeto queja recibido del formulario.
                quejaUpdate.Estado = queja.Estado;
                // 40. quejaUpdate.Motivo = queja.Motivo;
                //     Se actualiza la propiedad Motivo de manera similar.
                quejaUpdate.Motivo = queja.Motivo;

                // 41. _context.Update(quejaUpdate);
                //     Se marca el objeto queja existente (quejaUpdate) en el contexto de la base de datos como modificado.
                _context.Update(quejaUpdate);
                // 42. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos, lo que incluye la actualización de los datos de la queja en la tabla Quejas.
                await _context.SaveChangesAsync();
                // 43. return RedirectToAction(nameof(List));
                //     Si la edición de la queja se realiza con éxito, se redirige al usuario a la acción List del mismo controlador Queja, que probablemente muestra la lista de quejas para los supervisores.
                return RedirectToAction(nameof(List));
            }
            // 44. catch (DbUpdateConcurrencyException)
            //     Se captura la excepción DbUpdateConcurrencyException, que ocurre cuando los datos han sido modificados por otro usuario desde que se cargaron para su edición.
            catch (DbUpdateConcurrencyException)
            {
                // 45. if (!QuejaExists(queja.Id))
                //     Se llama a una función (se asume que existe en el proyecto) llamada QuejaExists para verificar si la queja con el ID proporcionado todavía existe en la base de datos.
                if (!QuejaExists(queja.Id))
                {
                    // 46. return NotFound();
                    //     Si la queja ya no existe, se devuelve un resultado NotFound.
                    return NotFound();
                }
                // 47. else
                //     Si la queja todavía existe (la excepción se debió a una edición concurrente), se ejecuta este bloque.
                else
                {
                    // 48. return View(queja);
                    //     Se devuelve la vista de edición nuevamente, pasando el objeto queja recibido del formulario como modelo.
                    //     Esto permite mostrar un mensaje al usuario indicando que los datos han sido modificados por otro usuario y que debe revisar los cambios.
                    return View(queja);
                }
            }
        }

        // 1. // GET: Mantenimiento/Delete/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a una ruta como /Mantenimiento/Delete/{id}, donde {id} es el ID de la queja a eliminar.
        //    Aunque la ruta dice "Mantenimiento", el controlador probablemente sea "QuejaController" y se está intentando eliminar una queja.
        //    Esta acción típicamente muestra una confirmación antes de la eliminación.
        // 2. [Authorize(Roles = "Ciudadano")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //    pueden acceder a esta acción Delete (GET). Se asume que los ciudadanos pueden eliminar sus propias quejas.
        [Authorize(Roles = "Ciudadano")]
        // 3. public async Task<IActionResult> Delete(int? id)
        //    Se define una acción asíncrona llamada Delete que devuelve un IActionResult.
        //    Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la queja a eliminar.
        public async Task<IActionResult> Delete(int? id)
        {
            // 4. if (id == null)
            //    Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 5. return NotFound();
                //    Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 6. var queja = await _context.Quejas
            //         .Include(m => m.IdCiudadanoNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //    Se realiza una consulta asíncrona a la base de datos para obtener la queja cuyo Id coincide con el valor del parámetro id.
            //    Se utiliza Include para cargar de forma eager la propiedad de navegación IdCiudadanoNavigation (que representa el ciudadano que creó la queja).
            //    FirstOrDefaultAsync devuelve la primera queja que coincida con el predicado o null si no se encuentra ninguna.
            var queja = await _context.Quejas
                .Include(m => m.IdCiudadanoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            // 7. if (queja == null)
            //    Se verifica si la queja recuperada de la base de datos es nula (no se encontró ninguna queja con el ID proporcionado).
            if (queja == null)
            {
                // 8. return NotFound();
                //    Si queja es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 9. return View(queja);
            //    Se devuelve la vista asociada a la acción Delete, pasando el objeto queja (que incluye la información del ciudadano)
            //    como modelo para que se muestren los detalles de la queja que se va a eliminar y se pida confirmación al usuario.
            return View(queja);
        }

        // 10. // POST: Queja/Delete/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a la ruta /Queja/Delete/{id}. Esta acción se invoca cuando el usuario confirma la eliminación de la queja.
        //     **Nota:** La ruta en el comentario es /Queja/Delete, mientras que la acción GET tenía una ruta que comenzaba con /Mantenimiento. Esto podría ser una inconsistencia en el código o un error en el comentario.
        // 11. [Authorize(Roles = "Ciudadano")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //     pueden acceder a esta acción DeleteConfirmed (POST).
        // 12. [HttpPost, ActionName("Delete")]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //     ActionName("Delete") especifica que aunque el nombre del método es DeleteConfirmed, la acción responde a la ruta /Queja/Delete.
        // 13. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes para proteger contra ataques CSRF.
        [Authorize(Roles = "Ciudadano")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 14. public async Task<IActionResult> DeleteConfirmed(int id)
        //     Se define una acción asíncrona llamada DeleteConfirmed que devuelve un IActionResult.
        //     Recibe un parámetro id del tipo entero, que es el ID de la queja a eliminar (generalmente el mismo ID que se pasó en la acción GET).
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 15. var queja = await _context.Quejas.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Quejas utilizando el método FindAsync del contexto de la base de datos
            //     para obtener la queja con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var queja = await _context.Quejas.FindAsync(id);
            // 16. if (queja != null)
            //     Se verifica si se encontró una queja con el ID proporcionado en la base de datos.
            if (queja != null)
            {
                // 17. _context.Quejas.Remove(queja);
                //     Si se encontró la queja, se marca para su eliminación del contexto de la base de datos.
                _context.Quejas.Remove(queja);
            }
            // 18. await _context.SaveChangesAsync();
            //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la eliminación de la queja (si se encontró) de la tabla Quejas.
            await _context.SaveChangesAsync();
            // 19. return RedirectToAction(nameof(Index));
            //     Después de la eliminación (o intento de eliminación), se redirige al usuario a la acción Index del mismo controlador (se asume que es QuejaController), que probablemente muestra la lista de sus quejas.
            return RedirectToAction(nameof(Index));
        }

        // 20. private bool QuejaExists(int id)
        //     Se define un método privado llamado QuejaExists que devuelve un valor booleano.
        //     Este método se utiliza para verificar si una queja con el ID especificado existe en la base de datos.
        // 21. return _context.Quejas.Any(e => e.Id == id);
        //     Se realiza una consulta a la base de datos utilizando LINQ y el método Any para verificar si existe algún registro en la tabla Quejas
        //     cuya propiedad Id coincida con el ID proporcionado. Devuelve true si existe al menos un registro coincidente, y false en caso contrario.
        private bool QuejaExists(int id)
        {
            return _context.Quejas.Any(e => e.Id == id);
        }

        // 22. public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        //     Se define un método público asíncrono llamado GenerarByteImage que toma dos parámetros:
        //     - file: Un objeto IFormFile opcional (puede ser nulo) que representa un archivo cargado a través de un formulario.
        //     - bytesImage: Un array de bytes opcional (puede ser nulo) que representa una imagen existente (por ejemplo, para mantener la imagen anterior si no se carga una nueva).
        //     El método devuelve un Task que contiene un array de bytes nullable (byte[]?), que representa los bytes de la imagen.
        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            // 23. byte[]? bytes = bytesImage;
            //     Se inicializa una variable local bytes con el valor del parámetro bytesImage. Esto significa que por defecto,
            //     si no se carga un nuevo archivo, se mantendrá la imagen existente (si se proporcionó).
            byte[]? bytes = bytesImage;
            // 24. if (file != null && file.Length > 0)
            //     Se verifica si el parámetro file no es nulo y si la longitud del archivo cargado es mayor que 0 (es decir, se cargó un archivo).
            if (file != null && file.Length > 0)
            {
                // 25. // Construir la ruta del archivo
                //     Este es un comentario (aunque la ruta del archivo no se construye explícitamente en este método;
                //     los bytes del archivo se leen directamente del IFormFile).
                // 26. using (var memoryStream = new MemoryStream())
                //     Se crea una nueva instancia de MemoryStream utilizando la instrucción using. MemoryStream es un flujo
                //     que almacena datos en la memoria. La instrucción using asegura que el MemoryStream se cierre y
                //     se liberen sus recursos correctamente al finalizar el bloque.
                using (var memoryStream = new MemoryStream())
                {
                    // 27. await file.CopyToAsync(memoryStream);
                    //     Se llama al método asíncrono CopyToAsync del objeto IFormFile (file) para copiar el contenido
                    //     del archivo cargado al MemoryStream. La palabra clave await asegura que esta operación asíncrona
                    //     se complete antes de continuar.
                    await file.CopyToAsync(memoryStream);
                    // 28. bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                    //     Una vez que el contenido del archivo se ha copiado al MemoryStream, se llama al método ToArray()
                    //     del MemoryStream para obtener los datos como un array de bytes. Este array de bytes se asigna
                    //     a la variable bytes, sobrescribiendo el valor inicial de bytesImage (si se cargó un nuevo archivo).
                    bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                }
            }
            // 29. return bytes;
            //     Finalmente, se devuelve el array de bytes que representa la imagen. Si se cargó un nuevo archivo,
            //     serán los bytes de ese archivo. Si no se cargó ningún archivo, se devolverá el valor original de
            //     bytesImage (la imagen existente) o null si bytesImage también era nulo.
            return bytes;
        }
    }
}
