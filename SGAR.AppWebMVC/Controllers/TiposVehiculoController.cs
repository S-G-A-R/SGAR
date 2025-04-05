using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    // 1. public class TiposVehiculoController : Controller declara una clase llamada TiposVehiculoController que hereda de la clase Controller.
    //    Esto indica que esta clase manejará las solicitudes HTTP relacionadas con los tipos de vehículos.
    public class TiposVehiculoController : Controller
    {
        // 2. private readonly SgarDbContext _context; declara una variable privada de solo lectura llamada _context del tipo SgarDbContext.
        //    SgarDbContext probablemente es la clase que representa la conexión a la base de datos utilizando Entity Framework Core.
        //    'readonly' asegura que la instancia de _context solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public TiposVehiculoController(SgarDbContext context) es el constructor de la clase TiposVehiculoController.
        // 4. Recibe una instancia de SgarDbContext como parámetro. Esta instancia se inyecta automáticamente por el sistema de
        //    inyección de dependencias de ASP.NET Core.
        public TiposVehiculoController(SgarDbContext context)
        {
            // 5. _context = context; asigna la instancia de SgarDbContext recibida al campo privado _context.
            //    Esto permite que el controlador acceda a la base de datos a través de este contexto.
            _context = context;
        }

        // 6. // GET: TiposVehiculo es un comentario que indica que la siguiente acción responde a las solicitudes HTTP GET
        //    a la ruta base de este controlador (probablemente "/TiposVehiculo").
        // 7. public async Task<IActionResult> Index(TiposVehiculo tiposVehiculo, int topRegistro = 10) declara una acción asíncrona
        //    llamada Index que devuelve un IActionResult (que representa el resultado de una acción del controlador).
        // 8. Recibe dos parámetros:
        //    - tiposVehiculo: una instancia de la clase TiposVehiculo, que se utiliza para filtrar los resultados (model binding).
        //    - topRegistro: un entero opcional con un valor predeterminado de 10, que limita el número de registros a mostrar.
        public async Task<IActionResult> Index(TiposVehiculo tiposVehiculo, int topRegistro = 10)
        {
            // 9. var query = _context.TiposVehiculos.AsQueryable(); inicia una consulta LINQ sobre el DbSet de TiposVehiculos.
            //    'AsQueryable()' permite construir la consulta de forma dinámica antes de ejecutarla contra la base de datos.
            var query = _context.TiposVehiculos.AsQueryable();

            // 10. if (!string.IsNullOrWhiteSpace(tiposVehiculo.Descripcion)) verifica si la propiedad Descripcion del objeto
            //     tiposVehiculo no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(tiposVehiculo.Descripcion))
                // 11. query = query.Where(s => s.Descripcion.ToLower().Contains(tiposVehiculo.Descripcion.ToLower()));
                //     Si la descripción no está vacía, agrega una cláusula WHERE a la consulta para filtrar los tipos de vehículos
                //     cuya descripción (convertida a minúsculas) contenga la descripción proporcionada (también convertida a minúsculas).
                query = query.Where(s => s.Descripcion.ToLower().Contains(tiposVehiculo.Descripcion.ToLower()));

            // 12. if (topRegistro > 0) verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 13. query = query.Take(topRegistro); si topRegistro es positivo, agrega una cláusula TAKE a la consulta para
                //     limitar el número de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);

            // 14. query = query.OrderByDescending(s => s.Id); ordena la consulta de forma descendente según la propiedad Id.
            query = query.OrderByDescending(s => s.Id);

            // 15. return View(await query.OrderByDescending(s => s.Id).ToListAsync());
            //     Ejecuta la consulta LINQ de forma asíncrona contra la base de datos utilizando 'ToListAsync()', lo que devuelve
            //     una lista de objetos TiposVehiculo.
            // 16. Luego, pasa esta lista como modelo a la vista asociada a la acción Index (probablemente "Index.cshtml"),
            //     que se encargará de mostrar los tipos de vehículos filtrados y ordenados.
            // 17. Note que la ordenación descendente por Id se aplica dos veces, lo cual es redundante.
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // 18. // GET: TiposVehiculo/Details/5 es un comentario que indica que la siguiente acción responde a las solicitudes HTTP GET
        //     a una ruta como "/TiposVehiculo/Details/{id}", donde {id} es un parámetro.
        // 19. public async Task<IActionResult> Details(int? id) declara una acción asíncrona llamada Details que devuelve un IActionResult.
        // 20. Recibe un parámetro entero nullable llamado 'id', que representa el ID del tipo de vehículo a mostrar en detalle.
        public async Task<IActionResult> Details(int? id)
        {
            // 21. if (id == null) verifica si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 22. return NotFound(); si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                //     indicando que no se encontró el recurso.
                return NotFound();
            }

            // 23. var tiposVehiculo = await _context.TiposVehiculos
            //     .FirstOrDefaultAsync(m => m.Id == id);
            //     Busca de forma asíncrona en la tabla 'TiposVehiculos' la primera entidad cuyo 'Id' coincida con el valor del parámetro 'id'.
            //     'FirstOrDefaultAsync' devuelve la entidad encontrada o null si no existe ninguna con ese ID.
            var tiposVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);

            // 24. if (tiposVehiculo == null) verifica si no se encontró ningún tipo de vehículo con el ID proporcionado.
            if (tiposVehiculo == null)
            {
                // 25. return NotFound(); si no se encontró el tipo de vehículo, devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 26. return View(tiposVehiculo); pasa la entidad 'tiposVehiculo' encontrada como modelo a la vista asociada a la acción
            //     Details (probablemente "Details.cshtml"), que se encargará de mostrar los detalles del tipo de vehículo.
            return View(tiposVehiculo);
        }

        // 1. // GET: TiposVehiculo/Create es un comentario que indica que la siguiente acción responde a las solicitudes HTTP GET
        //    a la ruta "/TiposVehiculo/Create". Esta ruta se utiliza para mostrar el formulario de creación de un nuevo tipo de vehículo.
        // 2. public IActionResult Create() declara una acción síncrona llamada Create que devuelve un IActionResult.
        public IActionResult Create()
        {
            // 3. return View(); devuelve la vista asociada a la acción Create (probablemente "Create.cshtml").
            //    Esta vista contendrá el formulario para ingresar los datos de un nuevo tipo de vehículo.
            return View();
        }

        // 1. // POST: TiposVehiculo/Create es un comentario que indica que la siguiente acción responde a las solicitudes HTTP POST
        //    a la ruta "/TiposVehiculo/Create". Esta ruta se utiliza para recibir los datos del formulario de creación y guardar el nuevo tipo de vehículo.
        // 2. [HttpPost] es un atributo que especifica que esta acción solo responde a las solicitudes HTTP POST.
        // 3. [ValidateAntiForgeryToken] es un atributo que agrega protección contra ataques de falsificación de solicitudes entre sitios (CSRF).
        //    Genera un token que debe incluirse en el formulario y se valida en el servidor.
        // 4. public async Task<IActionResult> Create([Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo) declara una acción asíncrona
        //    llamada Create que devuelve un IActionResult.
        // 5. Recibe un parámetro llamado 'tiposVehiculo' del tipo TiposVehiculo.
        // 6. [Bind("Id,Tipo,Descripcion")] es un atributo que especifica qué propiedades del modelo TiposVehiculo se deben vincular
        //    a los datos enviados en la solicitud POST. Esto ayuda a prevenir la sobre-vinculación (overposting) de propiedades no deseadas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo)
        {
            // 7. if (ModelState.IsValid) verifica si el modelo 'tiposVehiculo' recibido es válido según las reglas de validación
            //    definidas en la clase TiposVehiculo (a través de Data Annotations u otras configuraciones de validación).
            if (ModelState.IsValid)
            {
                // 8. _context.Add(tiposVehiculo); agrega la instancia de 'tiposVehiculo' al DbSet de TiposVehiculos en el contexto
                //    de la base de datos. Esto marca la entidad para ser insertada en la base de datos.
                _context.Add(tiposVehiculo);

                // 9. await _context.SaveChangesAsync(); guarda de forma asíncrona todos los cambios pendientes en el contexto de la base de datos.
                //    Esto incluye la inserción del nuevo 'tiposVehiculo' en la tabla correspondiente.
                //    'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
                await _context.SaveChangesAsync();

                // 10. return RedirectToAction(nameof(Index)); redirige al usuario a la acción Index de este mismo controlador.
                //     'nameof(Index)' se utiliza para obtener el nombre de la acción como una cadena, lo que evita errores tipográficos.
                //     Normalmente, la acción Index muestra la lista de tipos de vehículos.
                return RedirectToAction(nameof(Index));
            }
            // 11. return View(tiposVehiculo); si el modelo no es válido (ModelState.IsValid es false), se vuelve a mostrar la vista de creación
            //     ("Create.cshtml") y se le pasa el objeto 'tiposVehiculo' como modelo. Esto permite al usuario ver los errores de validación
            //     y corregir los datos del formulario.
            return View(tiposVehiculo);
        }

        // 1. // GET: TiposVehiculo/Edit/5 es un comentario que indica que la siguiente acción responde a las solicitudes HTTP GET
        //    a una ruta como "/TiposVehiculo/Edit/{id}", donde {id} es un parámetro que representa el ID del tipo de vehículo a editar.
        // 2. public async Task<IActionResult> Edit(int? id) declara una acción asíncrona llamada Edit que devuelve un IActionResult.
        // 3. Recibe un parámetro entero nullable llamado 'id', que representa el ID del tipo de vehículo a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 4. if (id == null) verifica si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 5. return NotFound(); si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                //    indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 6. var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);
            //    Busca de forma asíncrona en la tabla 'TiposVehiculos' la entidad con el ID especificado.
            //    'FindAsync' es eficiente para buscar por la clave primaria. Devuelve la entidad encontrada o null si no existe.
            var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);

            // 7. if (tiposVehiculo == null) verifica si no se encontró ningún tipo de vehículo con el ID proporcionado.
            if (tiposVehiculo == null)
            {
                // 8. return NotFound(); si no se encontró el tipo de vehículo, devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 9. return View(tiposVehiculo); pasa la entidad 'tiposVehiculo' encontrada como modelo a la vista asociada a la acción Edit
            //    (probablemente "Edit.cshtml"). Esta vista contendrá un formulario pre-llenado con los datos del tipo de vehículo a editar.
            return View(tiposVehiculo);
        }

        // 1. // POST: TiposVehiculo/Edit/5 es un comentario que indica que la siguiente acción responde a las solicitudes HTTP POST
        //    a una ruta como "/TiposVehiculo/Edit/{id}", donde {id} es un parámetro que representa el ID del tipo de vehículo a editar.
        // 2. [HttpPost] es un atributo que especifica que esta acción solo responde a las solicitudes HTTP POST.
        // 3. [ValidateAntiForgeryToken] es un atributo que agrega protección contra ataques de falsificación de solicitudes entre sitios (CSRF).
        //    Genera un token que debe incluirse en el formulario y se valida en el servidor.
        // 4. public async Task<IActionResult> Edit(int id, [Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo) declara una acción
        //    asíncrona llamada Edit que devuelve un IActionResult.
        // 5. Recibe dos parámetros:
        //    - id: un entero que se espera que coincida con el ID del tipo de vehículo a editar (normalmente de la ruta).
        //    - tiposVehiculo: una instancia de la clase TiposVehiculo, cuyos datos se toman del cuerpo de la solicitud POST ([Bind]).
        // 6. [Bind("Id,Tipo,Descripcion")] es un atributo que especifica qué propiedades del modelo TiposVehiculo se deben vincular
        //    a los datos enviados en la solicitud POST. Esto ayuda a prevenir la sobre-vinculación (overposting) de propiedades no deseadas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo)
        {
            // 7. if (id != tiposVehiculo.Id) verifica si el ID proporcionado en la ruta ('id') no coincide con el ID
            //    de la entidad TiposVehiculo recibida en el cuerpo de la solicitud.
            if (id != tiposVehiculo.Id)
            {
                // 8. return NotFound(); si los IDs no coinciden, devuelve un resultado NotFound (código de estado HTTP 404),
                //    indicando una inconsistencia en la solicitud.
                return NotFound();
            }

            // 9. if (ModelState.IsValid) verifica si el modelo 'tiposVehiculo' recibido es válido según las reglas de validación
            //    definidas en la clase TiposVehiculo.
            if (ModelState.IsValid)
            {
                // 10. try bloque para envolver el código que puede generar excepciones relacionadas con la base de datos.
                try
                {
                    // 11. _context.Update(tiposVehiculo); marca la entidad 'tiposVehiculo' en el contexto de la base de datos
                    //     como modificada. Entity Framework Core realizará un seguimiento de los cambios realizados en esta entidad.
                    _context.Update(tiposVehiculo);

                    // 12. await _context.SaveChangesAsync(); guarda de forma asíncrona todos los cambios pendientes en el contexto de la base de datos.
                    //     Esto incluye la actualización de la entidad 'tiposVehiculo' en la tabla correspondiente.
                    //     'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
                    await _context.SaveChangesAsync();
                }
                // 13. catch (DbUpdateConcurrencyException) captura una excepción que ocurre cuando varios usuarios intentan
                //     editar la misma entidad al mismo tiempo, lo que resulta en conflictos de concurrencia en la base de datos.
                catch (DbUpdateConcurrencyException)
                {
                    // 14. if (!TiposVehiculoExists(tiposVehiculo.Id)) llama al método privado TiposVehiculoExists para verificar
                    //     si el tipo de vehículo con el ID proporcionado todavía existe en la base de datos.
                    if (!TiposVehiculoExists(tiposVehiculo.Id))
                    {
                        // 15. return NotFound(); si el tipo de vehículo ya no existe, devuelve un resultado NotFound (código de estado HTTP 404).
                        return NotFound();
                    }
                    // 16. else si el tipo de vehículo todavía existe, significa que hubo un conflicto de concurrencia real.
                    else
                    {
                        // 17. throw; relanza la excepción DbUpdateConcurrencyException para que sea manejada por un nivel superior
                        //     o por el middleware de manejo de errores de ASP.NET Core.
                        throw;
                    }
                }
                // 18. return RedirectToAction(nameof(Index)); si la actualización fue exitosa, redirige al usuario a la acción Index
                //     de este mismo controlador (que probablemente muestra la lista de tipos de vehículos).
                return RedirectToAction(nameof(Index));
            }
            // 19. return View(tiposVehiculo); si el modelo no es válido (ModelState.IsValid es false), se vuelve a mostrar la vista de edición
            //     (probablemente "Edit.cshtml") y se le pasa el objeto 'tiposVehiculo' como modelo. Esto permite al usuario ver los
            //     errores de validación y corregir los datos del formulario.
            return View(tiposVehiculo);
        }

        // 1. // GET: TiposVehiculo/Delete/5 es un comentario que indica que la siguiente acción responde a las solicitudes HTTP GET
        //    a una ruta como "/TiposVehiculo/Delete/{id}", donde {id} es un parámetro que representa el ID del tipo de vehículo a eliminar.
        // 2. public async Task<IActionResult> Delete(int? id) declara una acción asíncrona llamada Delete que devuelve un IActionResult.
        // 3. Recibe un parámetro entero nullable llamado 'id', que representa el ID del tipo de vehículo a eliminar.
        public async Task<IActionResult> Delete(int? id)
        {
            // 4. if (id == null) verifica si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 5. return NotFound(); si 'id' es nulo, devuelve un resultado NotFound (código de estado HTTP 404),
                //    indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 6. var tiposVehiculo = await _context.TiposVehiculos
            //     .FirstOrDefaultAsync(m => m.Id == id);
            //     Busca de forma asíncrona en la tabla 'TiposVehiculos' la primera entidad cuyo 'Id' coincida con el valor del parámetro 'id'.
            //     'FirstOrDefaultAsync' devuelve la entidad encontrada o null si no existe ninguna con ese ID.
            var tiposVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);

            // 7. if (tiposVehiculo == null) verifica si no se encontró ningún tipo de vehículo con el ID proporcionado.
            if (tiposVehiculo == null)
            {
                // 8. return NotFound(); si no se encontró el tipo de vehículo, devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 9. return View(tiposVehiculo); pasa la entidad 'tiposVehiculo' encontrada como modelo a la vista asociada a la acción Delete
            //    (probablemente "Delete.cshtml"). Esta vista se utiliza para confirmar si el usuario realmente desea eliminar el tipo de vehículo.
            return View(tiposVehiculo);
        }

        // 1. // POST: TiposVehiculo/Delete/5 es un comentario que indica que la siguiente acción responde a las solicitudes HTTP POST
        //    a una ruta como "/TiposVehiculo/Delete/{id}", donde {id} es un parámetro que representa el ID del tipo de vehículo a eliminar.
        // 2. [HttpPost, ActionName("Delete")] son atributos aplicados a esta acción.
        //    - [HttpPost] especifica que esta acción solo responde a las solicitudes HTTP POST.
        //    - [ActionName("Delete")] cambia el nombre de la acción a "Delete" para el enrutamiento, aunque el nombre del método sea "DeleteConfirmed".
        //      Esto es común en escenarios de confirmación de eliminación para mantener las convenciones de nomenclatura de las acciones CRUD.
        // 3. [ValidateAntiForgeryToken] agrega protección contra ataques CSRF.
        // 4. public async Task<IActionResult> DeleteConfirmed(int id) declara una acción asíncrona llamada DeleteConfirmed
        //    que devuelve un IActionResult.
        // 5. Recibe un parámetro entero llamado 'id', que representa el ID del tipo de vehículo a eliminar (normalmente enviado desde el formulario de confirmación).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 6. var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);
            //    Busca de forma asíncrona en la tabla 'TiposVehiculos' la entidad con el ID especificado (la clave primaria).
            var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);

            // 7. if (tiposVehiculo != null) verifica si se encontró un tipo de vehículo con el ID proporcionado.
            if (tiposVehiculo != null)
            {
                // 8. _context.TiposVehiculos.Remove(tiposVehiculo); marca la entidad 'tiposVehiculo' para ser eliminada
                //    de la base de datos cuando se guarden los cambios.
                _context.TiposVehiculos.Remove(tiposVehiculo);
            }

            // 9. await _context.SaveChangesAsync(); guarda de forma asíncrona todos los cambios pendientes en el contexto de la base de datos.
            //    En este caso, se ejecutará la eliminación del tipo de vehículo en la base de datos.
            await _context.SaveChangesAsync();

            // 10. return RedirectToAction(nameof(Index)); redirige al usuario a la acción Index de este mismo controlador,
            //     que probablemente muestra la lista actualizada de tipos de vehículos (sin el elemento eliminado).
            return RedirectToAction(nameof(Index));
        }

        // 1. private bool TiposVehiculoExists(int id) declara un método privado que devuelve un valor booleano.
        // 2. Recibe un parámetro entero llamado 'id'.
        private bool TiposVehiculoExists(int id)
        {
            // 3. return _context.TiposVehiculos.Any(e => e.Id == id);
            //    Verifica si existe alguna entidad en la tabla 'TiposVehiculos' cuyo campo 'Id' coincida con el 'id' proporcionado.
            //    'Any()' devuelve true si al menos un elemento cumple la condición, y false en caso contrario.
            return _context.TiposVehiculos.Any(e => e.Id == id);
        }
    }
}
