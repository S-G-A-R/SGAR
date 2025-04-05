using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    // 1. [Authorize(Roles = "Operador, Supervisor")]
    //    Este atributo de autorización a nivel de clase indica que para acceder a cualquier acción (método público)
    //    dentro de este MantenimientoController, el usuario debe estar autenticado y pertenecer a uno de los roles
    //    especificados: "Operador" o "Supervisor".
    [Authorize(Roles = "Operador, Supervisor")]
    public class MantenimientoController : Controller
    {
        // 2. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public MantenimientoController(SgarDbContext context)
        //    Este es el constructor de la clase MantenimientoController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public MantenimientoController(SgarDbContext context)
        {
            // 4. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 5. // GET: Mantenimiento
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta base del controlador "Mantenimiento" (ej. /Mantenimiento o /Mantenimiento/Index).
        // 6. [Authorize(Roles = "Operador")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Operador"
        //    pueden acceder a esta acción Index, incluso si la autorización a nivel de clase permite también a "Supervisor".
        [Authorize(Roles = "Operador")]
        // 7. public async Task<IActionResult> Index(Mantenimiento mantenimiento, int topRegistro = 10)
        //    Se define una acción asíncrona llamada Index que devuelve un IActionResult.
        //    Recibe un objeto Mantenimiento como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> Index(Mantenimiento mantenimiento, int topRegistro = 10)
        {
            // 8. var mantenimientos = await _context.Mantenimientos.ToListAsync();
            //    Se realiza una consulta asíncrona a la base de datos para obtener todos los registros de la tabla Mantenimientos
            //    y se convierten en una lista (List<Mantenimiento>).
            var mantenimientos = await _context.Mantenimientos.ToListAsync();
            // 9. var mantenimientosQ1 = mantenimientos
            //         .Where(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value));
            //    Se filtra la lista de todos los mantenimientos (mantenimientos) utilizando LINQ para seleccionar solo aquellos
            //    cuya propiedad IdOperador coincida con el ID del usuario actual. Se asume que User representa
            //    el usuario autenticado y FindFirst("Id") busca la primera claim con el tipo "Id". El valor de la claim
            //    se convierte a un entero antes de la comparación. Esto asegura que los operadores solo vean sus propios mantenimientos.
            var mantenimientosQ1 = mantenimientos
                .Where(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value));

            // 10. var query = mantenimientosQ1.AsQueryable();
            //     Se convierte la colección filtrada de mantenimientos (mantenimientosQ1) a un IQueryable<Mantenimiento>.
            //     Esto permite construir consultas LINQ de forma más flexible y diferida.
            var query = mantenimientosQ1.AsQueryable();
            // 11. if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
            //     Se verifica si la propiedad Titulo del objeto mantenimiento (que puede contener criterios de filtro proporcionados
            //     por el usuario a través del formulario) no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
                // 12. query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
                //     Si la propiedad Titulo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     los mantenimientos cuyo Titulo contenga el valor proporcionado.
                query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
            // 13. if (topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 14. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 15. query = query
            //         .Include(p => p.IdOperadorNavigation);
            //     Se agrega una cláusula Include a la consulta para cargar de forma eager la propiedad de navegación
            //     IdOperadorNavigation del objeto Mantenimiento. Esto asume que existe una relación entre Mantenimiento y Operador
            //     y que IdOperadorNavigation permite acceder a la información del operador asociado al mantenimiento.
            query = query
                .Include(p => p.IdOperadorNavigation);
            // 16. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar los mantenimientos de forma
            //     descendente según su propiedad Id, mostrando los más recientes primero.
            query = query.OrderByDescending(s => s.Id);

            // 17. return View(query.OrderByDescending(s => s.Id).ToList());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Mantenimiento>) de forma síncrona (ToList()). Esta lista de mantenimientos se pasa a la vista
            //     asociada a la acción Index para ser mostrada al operador.
            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // 18. [Authorize(Roles = "Supervisor")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //     pueden acceder a esta acción List.
        [Authorize(Roles = "Supervisor")]
        // 19. public async Task<IActionResult> List(Mantenimiento mantenimiento, int topRegistro = 10)
        //     Se define una acción asíncrona llamada List que devuelve un IActionResult.
        //     Recibe un objeto Mantenimiento como parámetro (para posibles filtros a través de model binding)
        //     y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> List(Mantenimiento mantenimiento, int topRegistro = 10)
        {
            // 20. var mantenimientos = await _context.Mantenimientos.Include(s => s.IdOperadorNavigation).ToListAsync();
            //     Se realiza una consulta asíncrona a la base de datos para obtener todos los registros de la tabla Mantenimientos.
            //     Se utiliza Include para cargar de forma eager la propiedad de navegación IdOperadorNavigation (que representa el operador que realizó el mantenimiento).
            //     Finalmente, los resultados se convierten en una lista (List<Mantenimiento>).
            var mantenimientos = await _context.Mantenimientos.Include(s => s.IdOperadorNavigation).ToListAsync();
            // 21. var mantenimientosQ1 = mantenimientos
            //         .Where(s => s.IdOperadorNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));
            //     Se filtra la lista de todos los mantenimientos (mantenimientos) utilizando LINQ para seleccionar solo aquellos
            //     donde la propiedad IdAlcaldia del Operador asociado coincida con el valor de la claim "Alcaldia"
            //     del usuario actual (se asume que User representa el supervisor autenticado y FindFirst("Alcaldia") busca la
            //     primera claim con el tipo "Alcaldia"). El valor de la claim se convierte a un entero antes de la comparación.
            //     Esto asegura que los supervisores solo vean los mantenimientos realizados por operadores dentro de su alcaldía.
            var mantenimientosQ1 = mantenimientos
                .Where(s => s.IdOperadorNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));

            // 22. var query = mantenimientosQ1.AsQueryable();
            //     Se convierte la colección filtrada de mantenimientos (mantenimientosQ1) a un IQueryable<Mantenimiento>.
            //     Esto permite construir consultas LINQ de forma más flexible y diferida.
            var query = mantenimientosQ1.AsQueryable();
            // 23. if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
            //     Se verifica si la propiedad Titulo del objeto mantenimiento no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
                // 24. query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
                //     Si la propiedad Titulo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     los mantenimientos cuyo Titulo contenga el valor proporcionado.
                query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
            // 25. if (!string.IsNullOrWhiteSpace(mantenimiento.TipoSituacion))
            //     Se verifica si la propiedad TipoSituacion del objeto mantenimiento no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(mantenimiento.TipoSituacion))
                // 26. query = query.Where(s => s.TipoSituacion == mantenimiento.TipoSituacion);
                //     Si la propiedad TipoSituacion tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     los mantenimientos cuyo TipoSituacion coincida con el valor proporcionado.
                query = query.Where(s => s.TipoSituacion == mantenimiento.TipoSituacion);
            // 27. if (topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 28. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 29. query = query
            //         .Include(p => p.IdOperadorNavigation);
            //     Se agrega una cláusula Include a la consulta para cargar de forma eager la propiedad de navegación
            //     IdOperadorNavigation del objeto Mantenimiento. Aunque ya se incluyó, esta línea asegura que la información
            //     del operador esté disponible.
            query = query
                .Include(p => p.IdOperadorNavigation);
            // 30. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar los mantenimientos de forma
            //     descendente según su propiedad Id, mostrando los más recientes primero.
            query = query.OrderByDescending(s => s.Id);

            // 31. return View(query.OrderByDescending(s => s.Id).ToList());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Mantenimiento>) de forma síncrona (ToList()). Esta lista de mantenimientos se pasa a la vista
            //     asociada a la acción List para ser mostrada al supervisor.
            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // 1. // GET: Mantenimiento/Details/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a una ruta como /Mantenimiento/Details/{id}, donde {id} es el ID del mantenimiento a mostrar en detalle.
        public async Task<IActionResult> Details(int? id)
        {
            // 2. if (id == null)
            //    Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 3. return NotFound();
                //    Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 4. var mantenimiento = await _context.Mantenimientos
            //         .Include(m => m.IdOperadorNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //    Se realiza una consulta asíncrona a la base de datos para obtener el mantenimiento cuyo Id coincide con el valor del parámetro id.
            //    Se utiliza Include para cargar de forma eager la propiedad de navegación IdOperadorNavigation (que representa el operador que realizó el mantenimiento).
            //    FirstOrDefaultAsync devuelve el primer mantenimiento que coincida con el predicado o null si no se encuentra ninguno.
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.IdOperadorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            // 5. if (mantenimiento == null)
            //    Se verifica si el mantenimiento recuperado de la base de datos es nulo (no se encontró ningún mantenimiento con el ID proporcionado).
            if (mantenimiento == null)
            {
                // 6. return NotFound();
                //    Si mantenimiento es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 7. return View(mantenimiento);
            //    Se devuelve la vista asociada a la acción Details, pasando el objeto mantenimiento (que incluye la información del operador)
            //    como modelo para que se muestren los detalles del mantenimiento al usuario.
            return View(mantenimiento);
        }

        // 8. // GET: Mantenimiento/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta /Mantenimiento/Create. Esta acción típicamente muestra un formulario para crear un nuevo mantenimiento.
        // 9. [Authorize(Roles = "Operador")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Operador"
        //    pueden acceder a esta acción Create (GET).
        [Authorize(Roles = "Operador")]
        // 10. public IActionResult Create()
        //     Se define una acción pública llamada Create que devuelve un IActionResult.
        public IActionResult Create()
        {
            // 11. return View();
            //     Se devuelve la vista asociada a la acción Create. Por convención, buscará una vista llamada Create.cshtml
            //     en la carpeta Views/Mantenimiento. Generalmente, esta vista contendrá el formulario para crear un nuevo mantenimiento.
            return View();
        }

        // 12. // POST: Mantenimiento/Create
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a la ruta /Mantenimiento/Create. Esta acción se invoca cuando se envía el formulario de creación de un mantenimiento.
        // 13. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //     Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 14. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //     Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 15. [HttpPost]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 16. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 17. [Authorize(Roles = "Operador")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Operador"
        //     pueden acceder a esta acción Create (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Operador")]
        // 18. public async Task<IActionResult> Create(Mantenimiento mantenimiento)
        //     Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //     Recibe un objeto Mantenimiento como parámetro. ASP.NET Core intentará crear e inicializar este objeto
        //     a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Create(Mantenimiento mantenimiento)
        {
            // 19. try { ... } catch { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de creación del mantenimiento.
            try
            {
                // 20. mantenimiento.Estado = 1;
                //     Se establece la propiedad Estado del nuevo mantenimiento en 1. Esto podría representar un estado inicial (por ejemplo, "Pendiente").
                mantenimiento.Estado = 1;
                // 21. mantenimiento.Motivo = "";
                //     Se inicializa la propiedad Motivo del nuevo mantenimiento con una cadena vacía. El motivo podría ser llenado posteriormente si es necesario.
                mantenimiento.Motivo = "";
                // 22. mantenimiento.IdOperador = Convert.ToInt32(User.FindFirst("Id").Value);
                //     Se obtiene el valor de la claim "Id" del usuario actual (se asume que representa el ID del operador)
                //     y se convierte a un entero. Este valor se asigna a la propiedad IdOperador del nuevo mantenimiento,
                //     estableciendo la relación entre el mantenimiento y el operador que lo creó.
                mantenimiento.IdOperador = Convert.ToInt32(User.FindFirst("Id").Value);
                // 23. if (mantenimiento.File != null)
                //     Se verifica si la propiedad File del objeto mantenimiento (que se espera que sea un IFormFile para la carga de archivos) no es nula.
                if (mantenimiento.File != null)
                    // 24. mantenimiento.Archivo = await GenerarByteImage(mantenimiento.File);
                    //     Se llama a una función asíncrona llamada GenerarByteImage (se asume que existe en el proyecto)
                    //     que toma el archivo cargado (mantenimiento.File) y devuelve su representación como un array de bytes.
                    //     El resultado se asigna a la propiedad Archivo del mantenimiento, permitiendo adjuntar un archivo.
                    mantenimiento.Archivo = await GenerarByteImage(mantenimiento.File);

                // 25. _context.Add(mantenimiento);
                //     Se agrega el objeto mantenimiento al contexto de la base de datos (_context). Esto marca el objeto para ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.Add(mantenimiento);
                // 26. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la inserción del nuevo mantenimiento en la tabla Mantenimientos.
                await _context.SaveChangesAsync();
                // 27. return RedirectToAction(nameof(Index));
                //     Si la creación del mantenimiento se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Mantenimiento, que probablemente muestra la lista de mantenimientos del operador.
                return RedirectToAction(nameof(Index));
            }
            // 28. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 29. return View(mantenimiento);
                //     En caso de excepción durante el proceso de creación, se devuelve la vista asociada a la acción Create,
                //     pasando el objeto mantenimiento que se intentó crear como modelo. Esto permite mostrar los errores de validación
                //     y los datos que el usuario ya había ingresado, facilitando la corrección y reintento.
                return View(mantenimiento);
            }
        }

        // 30. // GET: Mantenimiento/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Mantenimiento/Edit/{id}, donde {id} es el ID del mantenimiento a editar.
        //     Esta acción típicamente muestra un formulario para editar un mantenimiento existente.
        // 31. [Authorize(Roles = "Supervisor")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //     pueden acceder a esta acción Edit (GET).
        [Authorize(Roles = "Supervisor")]
        // 32. public async Task<IActionResult> Edit(int? id)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID del mantenimiento a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 33. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 34. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 35. var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Mantenimientos utilizando el método FindAsync del contexto de la base de datos
            //     para obtener el mantenimiento con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            // 36. if (mantenimiento == null)
            //     Se verifica si el mantenimiento recuperado de la base de datos es nulo (no se encontró ningún mantenimiento con el ID proporcionado).
            if (mantenimiento == null)
            {
                // 37. return NotFound();
                //     Si mantenimiento es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 38. return View(mantenimiento);
            //     Se devuelve la vista asociada a la acción Edit, pasando el objeto mantenimiento como modelo para que el formulario de edición pueda mostrar los datos existentes.
            return View(mantenimiento);
        }

        // 1. // POST: Mantenimiento/Edit/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a una ruta como /Mantenimiento/Edit/{id}, donde {id} es el ID del mantenimiento a editar.
        //    Esta acción se invoca cuando se envía el formulario de edición.
        // 2. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //    Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 3. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 4. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 5. [ValidateAntiForgeryToken]
        //    Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 6. [Authorize(Roles = "Supervisor")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Supervisor"
        //    pueden acceder a esta acción Edit (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        // 7. public async Task<IActionResult> Edit(int id, Mantenimiento mantenimiento)
        //    Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //    Recibe dos parámetros:
        //    - id: El ID del mantenimiento a editar, que se espera que coincida con el ID en la ruta.
        //    - mantenimiento: Un objeto Mantenimiento que contiene los datos modificados del formulario, que ASP.NET Core intenta crear
        //                   e inicializar a partir de los datos del formulario enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Edit(int id, Mantenimiento mantenimiento)
        {
            // 8. if (id != mantenimiento.Id)
            //    Se verifica si el ID proporcionado en la ruta (id) no coincide con el ID del mantenimiento que se está intentando editar (mantenimiento.Id).
            if (id != mantenimiento.Id)
            {
                // 9. return NotFound();
                //    Si los IDs no coinciden, se devuelve un resultado NotFound (código de estado HTTP 404), indicando una posible manipulación de la solicitud.
                return NotFound();
            }

            // 10. try { ... } catch (DbUpdateConcurrencyException) { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de edición del mantenimiento,
            //     específicamente capturando DbUpdateConcurrencyException, que ocurre cuando varios usuarios intentan editar el mismo registro simultáneamente.
            try
            {
                // 11. var mantenimientoUpdate = await _context.Mantenimientos.FirstOrDefaultAsync(s => s.Id == mantenimiento.Id);
                //     Se realiza una consulta asíncrona a la base de datos para obtener el registro del mantenimiento existente
                //     cuyo ID coincida con el ID del mantenimiento que se está editando. Se utiliza FirstOrDefaultAsync para obtener el primer resultado o null si no se encuentra ninguno.
                var mantenimientoUpdate = await _context.Mantenimientos.FirstOrDefaultAsync(s => s.Id == mantenimiento.Id);
                // 12. mantenimientoUpdate.Estado = mantenimiento.Estado;
                //     Se actualiza la propiedad Estado del objeto mantenimiento existente (mantenimientoUpdate) con el valor proporcionado en el objeto mantenimiento recibido del formulario.
                mantenimientoUpdate.Estado = mantenimiento.Estado;
                // 13. mantenimientoUpdate.Motivo = mantenimiento.Motivo;
                //     Se actualiza la propiedad Motivo de manera similar.
                mantenimientoUpdate.Motivo = mantenimiento.Motivo;

                // 14. _context.Update(mantenimientoUpdate);
                //     Se marca el objeto mantenimiento existente (mantenimientoUpdate) en el contexto de la base de datos como modificado.
                _context.Update(mantenimientoUpdate);
                // 15. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos, lo que incluye la actualización de los datos del mantenimiento en la tabla Mantenimientos.
                await _context.SaveChangesAsync();
                // 16. return RedirectToAction(nameof(List));
                //     Si la edición del mantenimiento se realiza con éxito, se redirige al usuario a la acción List del mismo controlador Mantenimiento, que probablemente muestra la lista de mantenimientos para los supervisores.
                return RedirectToAction(nameof(List));
            }
            // 17. catch (DbUpdateConcurrencyException)
            //     Se captura la excepción DbUpdateConcurrencyException, que ocurre cuando los datos han sido modificados por otro usuario desde que se cargaron para su edición.
            catch (DbUpdateConcurrencyException)
            {
                // 18. if (!MantenimientoExists(mantenimiento.Id))
                //     Se llama a una función (se asume que existe en el proyecto) llamada MantenimientoExists para verificar si el mantenimiento con el ID proporcionado todavía existe en la base de datos.
                if (!MantenimientoExists(mantenimiento.Id))
                {
                    // 19. return NotFound();
                    //     Si el mantenimiento ya no existe, se devuelve un resultado NotFound.
                    return NotFound();
                }
                // 20. else
                //     Si el mantenimiento todavía existe (la excepción se debió a una edición concurrente), se ejecuta este bloque.
                else
                {
                    // 21. return View(mantenimiento);
                    //     Se devuelve la vista de edición nuevamente, pasando el objeto mantenimiento recibido del formulario como modelo.
                    //     Esto permite mostrar un mensaje al usuario indicando que los datos han sido modificados por otro usuario y que debe revisar los cambios.
                    return View(mantenimiento);
                }
            }
        }

        // 22. // GET: Mantenimiento/Delete/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Mantenimiento/Delete/{id}, donde {id} es el ID del mantenimiento a eliminar.
        //     Esta acción típicamente muestra una confirmación antes de la eliminación.
        // 23. [Authorize(Roles = "Operador")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Operador"
        //     pueden acceder a esta acción Delete (GET). Se asume que los operadores pueden eliminar sus propios mantenimientos.
        [Authorize(Roles = "Operador")]
        // 24. public async Task<IActionResult> Delete(int? id)
        //     Se define una acción asíncrona llamada Delete que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID del mantenimiento a eliminar.
        public async Task<IActionResult> Delete(int? id)
        {
            // 25. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 26. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 27. var mantenimiento = await _context.Mantenimientos
            //         .Include(m => m.IdOperadorNavigation)
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //     Se realiza una consulta asíncrona a la base de datos para obtener el mantenimiento cuyo Id coincide con el valor del parámetro id.
            //     Se utiliza Include para cargar de forma eager la propiedad de navegación IdOperadorNavigation (que representa el operador que realizó el mantenimiento).
            //     FirstOrDefaultAsync devuelve el primer mantenimiento que coincida con el predicado o null si no se encuentra ninguno.
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.IdOperadorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            // 28. if (mantenimiento == null)
            //     Se verifica si el mantenimiento recuperado de la base de datos es nulo (no se encontró ningún mantenimiento con el ID proporcionado).
            if (mantenimiento == null)
            {
                // 29. return NotFound();
                //     Si mantenimiento es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 30. return View(mantenimiento);
            //     Se devuelve la vista asociada a la acción Delete, pasando el objeto mantenimiento (que incluye la información del operador)
            //     como modelo para que se muestren los detalles del mantenimiento que se va a eliminar y se pida confirmación al usuario.
            return View(mantenimiento);
        }

        // 31. // POST: Mantenimiento/Delete/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a la ruta /Mantenimiento/Delete/{id}. Esta acción se invoca cuando el usuario confirma la eliminación del mantenimiento.
        // 32. [HttpPost, ActionName("Delete")]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //     ActionName("Delete") especifica que aunque el nombre del método es DeleteConfirmed, la acción responde a la ruta /Mantenimiento/Delete.
        // 33. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes para proteger contra ataques CSRF.
        // 34. [Authorize(Roles = "Operador")]
        //     Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Operador"
        //     pueden acceder a esta acción DeleteConfirmed (POST).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Operador")]
        // 35. public async Task<IActionResult> DeleteConfirmed(int id)
        //     Se define una acción asíncrona llamada DeleteConfirmed que devuelve un IActionResult.
        //     Recibe un parámetro id del tipo entero, que es el ID del mantenimiento a eliminar (generalmente el mismo ID que se pasó en la acción GET).
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 36. var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Mantenimientos utilizando el método FindAsync del contexto de la base de datos
            //     para obtener el mantenimiento con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            // 37. if (mantenimiento != null)
            //     Se verifica si se encontró un mantenimiento con el ID proporcionado en la base de datos.
            if (mantenimiento != null)
            {
                // 38. _context.Mantenimientos.Remove(mantenimiento);
                //     Si se encontró el mantenimiento, se marca para su eliminación del contexto de la base de datos.
                _context.Mantenimientos.Remove(mantenimiento);
            }
            // 39. await _context.SaveChangesAsync();
            //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la eliminación del mantenimiento (si se encontró) de la tabla Mantenimientos.
            await _context.SaveChangesAsync();
            // 40. return RedirectToAction(nameof(Index));
            //     Después de la eliminación (o intento de eliminación), se redirige al usuario a la acción Index del mismo controlador Mantenimiento, que probablemente muestra la lista de sus mantenimientos.
            return RedirectToAction(nameof(Index));
        }

        // 41. private bool MantenimientoExists(int id)
        //     Se define un método privado llamado MantenimientoExists que devuelve un valor booleano.
        //     Este método se utiliza para verificar si un mantenimiento con el ID especificado existe en la base de datos.
        // 42. return _context.Mantenimientos.Any(e => e.Id == id);
        //     Se realiza una consulta a la base de datos utilizando LINQ y el método Any para verificar si existe algún registro en la tabla Mantenimientos
        //     cuya propiedad Id coincida con el ID proporcionado. Devuelve true si existe al menos un registro coincidente, y false en caso contrario.
        private bool MantenimientoExists(int id)
        {
            return _context.Mantenimientos.Any(e => e.Id == id);
        }

        // 43. public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        //     Se define un método público asíncrono llamado GenerarByteImage que toma dos parámetros:
        //     - file: Un objeto IFormFile opcional (puede ser nulo) que representa un archivo cargado a través de un formulario.
        //     - bytesImage: Un array de bytes opcional (puede ser nulo) que representa una imagen existente (por ejemplo, para mantener la imagen anterior si no se carga una nueva).
        //     El método devuelve un Task que contiene un array de bytes nullable (byte[]?), que representa los bytes de la imagen.
        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            // 44. byte[]? bytes = bytesImage;
            //     Se inicializa una variable local bytes con el valor del parámetro bytesImage. Esto significa que por defecto,
            //     si no se carga un nuevo archivo, se mantendrá la imagen existente (si se proporcionó).
            byte[]? bytes = bytesImage;
            // 45. if (file != null && file.Length > 0)
            //     Se verifica si el parámetro file no es nulo y si la longitud del archivo cargado es mayor que 0 (es decir, se cargó un archivo).
            if (file != null && file.Length > 0)
            {
                // 46. // Construir la ruta del archivo
                //     Este es un comentario (aunque la ruta del archivo no se construye explícitamente en este método;
                //     los bytes del archivo se leen directamente del IFormFile).
                // 47. using (var memoryStream = new MemoryStream())
                //     Se crea una nueva instancia de MemoryStream utilizando la instrucción using. MemoryStream es un flujo
                //     que almacena datos en la memoria. La instrucción using asegura que el MemoryStream se cierre y
                //     se liberen sus recursos correctamente al finalizar el bloque.
                using (var memoryStream = new MemoryStream())
                {
                    // 48. await file.CopyToAsync(memoryStream);
                    //     Se llama al método asíncrono CopyToAsync del objeto IFormFile (file) para copiar el contenido
                    //     del archivo cargado al MemoryStream. La palabra clave await asegura que esta operación asíncrona
                    //     se complete antes de continuar.
                    await file.CopyToAsync(memoryStream);
                    // 49. bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                    //     Una vez que el contenido del archivo se ha copiado al MemoryStream, se llama al método ToArray()
                    //     del MemoryStream para obtener los datos como un array de bytes. Este array de bytes se asigna
                    //     a la variable bytes, sobrescribiendo el valor inicial de bytesImage (si se cargó un nuevo archivo).
                    bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                }
            }
            // 50. return bytes;
            //     Finalmente, se devuelve el array de bytes que representa la imagen. Si se cargó un nuevo archivo,
            //     serán los bytes de ese archivo. Si no se cargó ningún archivo, se devolverá el valor original de
            //     bytesImage (la imagen existente) o null si bytesImage también era nulo.
            return bytes;
        }
    }
}
