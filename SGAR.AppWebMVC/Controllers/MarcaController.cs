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
    // 1. [Authorize(Roles = "Alcaldia")]
    //    Este atributo de autorización a nivel de clase indica que para acceder a cualquier acción (método público)
    //    dentro de este MarcaController, el usuario debe estar autenticado y pertenecer al rol "Alcaldia".
    [Authorize(Roles = "Alcaldia")]
    public class MarcaController : Controller
    {
        // 2. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 3. public MarcaController(SgarDbContext context)
        //    Este es el constructor de la clase MarcaController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public MarcaController(SgarDbContext context)
        {
            // 4. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 5. // GET: Marca
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta base del controlador "Marca" (ej. /Marca o /Marca/Index).
        // 6. public async Task<IActionResult> Index(Marca marca, int topRegistro = 10)
        //    Se define una acción asíncrona llamada Index que devuelve un IActionResult.
        //    Recibe un objeto Marca como parámetro (para posibles filtros a través de model binding)
        //    y un parámetro opcional topRegistro con un valor predeterminado de 10.
        public async Task<IActionResult> Index(Marca marca, int topRegistro = 10)
        {
            // 7. var query = _context.Marcas.AsQueryable();
            //    Se crea una consulta LINQ (IQueryable) sobre la tabla Marcas del contexto de la base de datos.
            //    AsQueryable() permite construir la consulta de forma diferida y eficiente.
            var query = _context.Marcas.AsQueryable();
            // 8. if(!string.IsNullOrWhiteSpace(marca.Nombre))
            //    Se verifica si la propiedad Nombre del objeto marca (que puede contener criterios de filtro proporcionados
            //    por el usuario a través del formulario) no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(marca.Nombre))
                // 9. query = query.Where(s => s.Nombre.Contains(marca.Nombre));
                //    Si la propiedad Nombre tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //    las marcas cuyo Nombre contenga el valor proporcionado.
                query = query.Where(s => s.Nombre.Contains(marca.Nombre));
            // 10. if(!string.IsNullOrWhiteSpace(marca.Modelo))
            //     Se verifica si la propiedad Modelo del objeto marca no es nula, vacía o contiene solo espacios en blanco.
            if (!string.IsNullOrWhiteSpace(marca.Modelo))
                // 11. query = query.Where(s => s.Modelo.Contains(marca.Modelo));
                //     Si la propiedad Modelo tiene un valor, se agrega una cláusula Where a la consulta para filtrar
                //     las marcas cuyo Modelo contenga el valor proporcionado.
                query = query.Where(s => s.Modelo.Contains(marca.Modelo));
            // 12. if(topRegistro > 0)
            //     Se verifica si el valor de topRegistro es mayor que 0.
            if (topRegistro > 0)
                // 13. query = query.Take(topRegistro);
                //     Si topRegistro es mayor que 0, se agrega una cláusula Take a la consulta para limitar el número
                //     de resultados a la cantidad especificada en topRegistro.
                query = query.Take(topRegistro);
            // 14. query = query.OrderByDescending(s => s.Id);
            //     Se agrega una cláusula OrderByDescending a la consulta para ordenar las marcas de forma
            //     descendente según su propiedad Id, mostrando las más recientes primero (asumiendo que Id es autoincremental).
            query = query.OrderByDescending(s => s.Id);
            // 15. return View(await query.OrderByDescending(s => s.Id).ToListAsync());
            //     Se ejecuta la consulta LINQ (query) ordenando nuevamente los resultados de forma descendente por Id
            //     (esto parece redundante ya que ya se ordenó en el paso anterior) y luego se convierte el resultado
            //     a una lista (List<Marca>) de forma asíncrona (ToListAsync()). Esta lista de marcas se pasa a la vista
            //     asociada a la acción Index para ser mostrada al usuario.
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // 16. // GET: Marca/Details/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Marca/Details/{id}, donde {id} es el ID de la marca a mostrar en detalle.
        // 17. public async Task<IActionResult> Details(int? id)
        //     Se define una acción asíncrona llamada Details que devuelve un IActionResult.
        //     Recibe un parámetro opcional id del tipo entero (int?), que representa el ID de la marca a mostrar.
        public async Task<IActionResult> Details(int? id)
        {
            // 18. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 19. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 20. var marca = await _context.Marcas
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //     Se realiza una consulta asíncrona a la base de datos para obtener la marca cuyo Id coincide con el valor del parámetro id.
            //     FirstOrDefaultAsync devuelve la primera marca que coincida con el predicado o null si no se encuentra ninguna.
            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == id);
            // 21. if (marca == null)
            //     Se verifica si la marca recuperada de la base de datos es nula (no se encontró ninguna marca con el ID proporcionado).
            if (marca == null)
            {
                // 22. return NotFound();
                //     Si marca es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 23. return View(marca);
            //     Se devuelve la vista asociada a la acción Details, pasando el objeto marca como modelo para que se muestren sus detalles al usuario.
            return View(marca);
        }

        // 1. // GET: Marca/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a la ruta /Marca/Create. Esta acción típicamente muestra un formulario para crear una nueva marca.
        public IActionResult Create()
        {
            // 2. return View();
            //    Se devuelve la vista asociada a la acción Create. Por convención, buscará una vista llamada Create.cshtml
            //    en la carpeta Views/Marca. Generalmente, esta vista contendrá el formulario para crear una nueva marca.
            return View();
        }

        // 3. // POST: Marca/Create
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a la ruta /Marca/Create. Esta acción se invoca cuando se envía el formulario de creación de una marca.
        // 4. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //    Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 5. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 6. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 7. [ValidateAntiForgeryToken]
        //    Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 8. public async Task<IActionResult> Create([Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        //    Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //    Recibe un objeto Marca como parámetro. El atributo [Bind] especifica las propiedades del objeto Marca
        //    que se deben incluir para el model binding desde los datos del formulario. Esto ayuda a prevenir ataques de sobrepublicación.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        {
            // 9. if (ModelState.IsValid)
            //    Se verifica si el modelo (el objeto marca) es válido según las reglas de validación definidas en la clase Marca (a través de Data Annotations u otras configuraciones).
            if (ModelState.IsValid)
            {
                // 10. _context.Add(marca);
                //     Se agrega el objeto marca al contexto de la base de datos (_context). Esto marca el objeto para ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.Add(marca);
                // 11. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la inserción de la nueva marca en la tabla Marcas.
                await _context.SaveChangesAsync();
                // 12. return RedirectToAction(nameof(Index));
                //     Si la creación de la marca se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Marca, que probablemente muestra la lista de marcas.
                return RedirectToAction(nameof(Index));
            }
            // 13. return View(marca);
            //     Si el modelo no es válido (ModelState.IsValid es false), se devuelve la vista asociada a la acción Create,
            //     pasando el objeto marca como modelo. Esto permite mostrar los errores de validación al usuario para que pueda corregirlos.
            return View(marca);
        }

        // 14. // GET: Marca/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //     a una ruta como /Marca/Edit/{id}, donde {id} es el ID de la marca a editar.
        //     Esta acción típicamente muestra un formulario para editar una marca existente.
        public async Task<IActionResult> Edit(int? id)
        {
            // 15. if (id == null)
            //     Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 16. return NotFound();
                //     Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 17. var marca = await _context.Marcas.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Marcas utilizando el método FindAsync del contexto de la base de datos
            //     para obtener la marca con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var marca = await _context.Marcas.FindAsync(id);
            // 18. if (marca == null)
            //     Se verifica si la marca recuperada de la base de datos es nula (no se encontró ninguna marca con el ID proporcionado).
            if (marca == null)
            {
                // 19. return NotFound();
                //     Si marca es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }
            // 20. return View(marca);
            //     Se devuelve la vista asociada a la acción Edit, pasando el objeto marca como modelo para que el formulario de edición pueda mostrar los datos existentes.
            return View(marca);
        }

        // 21. // POST: Marca/Edit/5
        //     Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //     a una ruta como /Marca/Edit/{id}, donde {id} es el ID de la marca a editar.
        //     Esta acción se invoca cuando se envía el formulario de edición.
        // 22. // To protect from overposting attacks, enable the specific properties you want to bind to.
        //     Este es un comentario que advierte sobre los ataques de sobrepublicación (overposting).
        // 23. // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //     Este es un comentario que proporciona un enlace a documentación sobre la protección contra sobrepublicación.
        // 24. [HttpPost]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        // 25. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        // 26. public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe dos parámetros:
        //     - id: El ID de la marca a editar, que se espera que coincida con el ID en la ruta.
        //     - marca: Un objeto Marca que contiene los datos modificados del formulario. El atributo [Bind] especifica las
        //              propiedades que se deben incluir para el model binding, ayudando a prevenir ataques de sobrepublicación.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        {
            // 27. if (id != marca.Id)
            //     Se verifica si el ID proporcionado en la ruta (id) no coincide con el ID de la marca que se está intentando editar (marca.Id).
            if (id != marca.Id)
            {
                // 28. return NotFound();
                //     Si los IDs no coinciden, se devuelve un resultado NotFound (código de estado HTTP 404), indicando una posible manipulación de la solicitud.
                return NotFound();
            }

            // 29. var marcaUpdate = await _context.Marcas
            //         .FirstOrDefaultAsync(m => m.Id == marca.Id);
            //     Se realiza una consulta asíncrona a la base de datos para obtener el registro de la marca existente
            //     cuyo ID coincida con el ID de la marca que se está editando. Se utiliza FirstOrDefaultAsync para obtener el primer resultado o null si no se encuentra ninguno.
            var marcaUpdate = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == marca.Id);
            // 30. try { ... } catch (DbUpdateConcurrencyException) { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso de edición de la marca,
            //     específicamente capturando DbUpdateConcurrencyException, que ocurre cuando varios usuarios intentan editar el mismo registro simultáneamente.
            try
            {
                // 31. marcaUpdate.Nombre = marca.Nombre;
                //     Se actualiza la propiedad Nombre del objeto marca existente (marcaUpdate) con el valor proporcionado en el objeto marca recibido del formulario.
                marcaUpdate.Nombre = marca.Nombre;
                // 32. marcaUpdate.Modelo = marca.Modelo;
                //     Se actualiza la propiedad Modelo de manera similar.
                marcaUpdate.Modelo = marca.Modelo;
                // 33. marcaUpdate.YearOfFabrication = marca.YearOfFabrication;
                //     Se actualiza la propiedad YearOfFabrication de manera similar.
                marcaUpdate.YearOfFabrication = marca.YearOfFabrication;
                // 34. _context.Update(marcaUpdate);
                //     Se marca el objeto marca existente (marcaUpdate) en el contexto de la base de datos como modificado.
                _context.Update(marcaUpdate);
                // 35. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos, lo que incluye la actualización de los datos de la marca en la tabla Marcas.
                await _context.SaveChangesAsync();
                // 36. return RedirectToAction(nameof(Index));
                //     Si la edición de la marca se realiza con éxito, se redirige al usuario a la acción Index del mismo controlador Marca, que probablemente muestra la lista de marcas.
                return RedirectToAction(nameof(Index));
            }
            // 37. catch (DbUpdateConcurrencyException)
            //     Se captura la excepción DbUpdateConcurrencyException, que ocurre cuando los datos han sido modificados por otro usuario desde que se cargaron para su edición.
            catch (DbUpdateConcurrencyException)
            {
                // 38. if (!MarcaExists(marca.Id))
                //     Se llama a una función (se asume que existe en el proyecto) llamada MarcaExists para verificar si la marca con el ID proporcionado todavía existe en la base de datos.
                if (!MarcaExists(marca.Id))
                {
                    // 39. return NotFound();
                    //     Si la marca ya no existe, se devuelve un resultado NotFound.
                    return NotFound();
                }
                // 40. else
                //     Si la marca todavía existe (la excepción se debió a una edición concurrente), se ejecuta este bloque.
                else
                {
                    // 41. return View(marca);
                    //     Se devuelve la vista de edición nuevamente, pasando el objeto marca recibido del formulario como modelo.
                    //     Esto permite mostrar un mensaje al usuario indicando que los datos han sido modificados por otro usuario y que debe revisar los cambios.
                    return View(marca);
                }
            }
        }

        // 1. // GET: Marca/Delete/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP GET
        //    a una ruta como /Marca/Delete/{id}, donde {id} es el ID de la marca a eliminar.
        //    Esta acción típicamente muestra una confirmación antes de la eliminación.
        public async Task<IActionResult> Delete(int? id)
        {
            // 2. if (id == null)
            //    Se verifica si el parámetro id es nulo.
            if (id == null)
            {
                // 3. return NotFound();
                //    Si id es nulo, se devuelve un resultado NotFound (código de estado HTTP 404), indicando que no se proporcionó un ID válido.
                return NotFound();
            }

            // 4. var marca = await _context.Marcas
            //         .FirstOrDefaultAsync(m => m.Id == id);
            //    Se realiza una consulta asíncrona a la base de datos para obtener la marca cuyo Id coincide con el valor del parámetro id.
            //    FirstOrDefaultAsync devuelve la primera marca que coincida con el predicado o null si no se encuentra ninguna.
            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == id);
            // 5. if (marca == null)
            //    Se verifica si la marca recuperada de la base de datos es nula (no se encontró ninguna marca con el ID proporcionado).
            if (marca == null)
            {
                // 6. return NotFound();
                //    Si marca es nulo, se devuelve un resultado NotFound (código de estado HTTP 404).
                return NotFound();
            }

            // 7. return View(marca);
            //    Se devuelve la vista asociada a la acción Delete, pasando el objeto marca como modelo para que se muestren los detalles de la marca que se va a eliminar y se pida confirmación al usuario.
            return View(marca);
        }

        // 8. // POST: Marca/Delete/5
        //    Este es un comentario que indica que la siguiente acción responde a una solicitud HTTP POST
        //    a la ruta /Marca/Delete/{id}. Esta acción se invoca cuando el usuario confirma la eliminación de la marca.
        // 9. [HttpPost, ActionName("Delete")]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //    ActionName("Delete") especifica que aunque el nombre del método es DeleteConfirmed, la acción responde a la ruta /Marca/Delete.
        // 10. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes para proteger contra ataques CSRF.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 11. var marca = await _context.Marcas.FindAsync(id);
            //     Se realiza una búsqueda asíncrona en la tabla Marcas utilizando el método FindAsync del contexto de la base de datos
            //     para obtener la marca con el ID especificado. FindAsync es eficiente para buscar por la clave primaria.
            var marca = await _context.Marcas.FindAsync(id);
            // 12. if (marca != null)
            //     Se verifica si se encontró una marca con el ID proporcionado en la base de datos.
            if (marca != null)
            {
                // 13. _context.Marcas.Remove(marca);
                //     Si se encontró la marca, se marca para su eliminación del contexto de la base de datos.
                _context.Marcas.Remove(marca);
            }

            // 14. await _context.SaveChangesAsync();
            //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos. Esto incluye la eliminación de la marca (si se encontró) de la tabla Marcas.
            await _context.SaveChangesAsync();
            // 15. return RedirectToAction(nameof(Index));
            //     Después de la eliminación (o intento de eliminación), se redirige al usuario a la acción Index del mismo controlador Marca, que probablemente muestra la lista de marcas.
            return RedirectToAction(nameof(Index));
        }

        // 16. private bool MarcaExists(int id)
        //     Se define un método privado llamado MarcaExists que devuelve un valor booleano.
        //     Este método se utiliza para verificar si una marca con el ID especificado existe en la base de datos.
        // 17. return _context.Marcas.Any(e => e.Id == id);
        //     Se realiza una consulta a la base de datos utilizando LINQ y el método Any para verificar si existe algún registro en la tabla Marcas
        //     cuya propiedad Id coincida con el ID proporcionado. Devuelve true si existe al menos un registro coincidente, y false en caso contrario.
        private bool MarcaExists(int id)
        {
            return _context.Marcas.Any(e => e.Id == id);
        }
    }
}
