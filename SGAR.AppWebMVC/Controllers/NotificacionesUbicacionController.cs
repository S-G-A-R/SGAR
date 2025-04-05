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
    public class NotificacionesUbicacionController : Controller
    {
        // 1. private readonly SgarDbContext _context;
        //    Se declara un campo privado de solo lectura llamado _context del tipo SgarDbContext.
        //    Se espera que SgarDbContext sea una clase que representa el contexto de la base de datos
        //    (probablemente utilizando Entity Framework Core). El uso de readonly asegura que la instancia
        //    de DbContext solo se asigne en el constructor.
        private readonly SgarDbContext _context;

        // 2. public NotificacionesUbicacionController(SgarDbContext context)
        //    Este es el constructor de la clase NotificacionesUbicacionController. Recibe una instancia de SgarDbContext
        //    como parámetro. Esto se conoce como inyección de dependencias, donde el framework (ASP.NET Core)
        //    proporciona la instancia del contexto de la base de datos al crear el controlador.
        public NotificacionesUbicacionController(SgarDbContext context)
        {
            // 3. _context = context;
            //    Dentro del constructor, la instancia de SgarDbContext recibida como parámetro se asigna
            //    al campo privado _context. Ahora, el controlador puede interactuar con la base de datos
            //    a través de esta instancia.
            _context = context;
        }

        // 4. [Authorize(Roles = "Ciudadano")]
        //    Este atributo de autorización a nivel de acción especifica que solo los usuarios con el rol "Ciudadano"
        //    pueden acceder a esta acción Index.
        [Authorize(Roles = "Ciudadano")]
        // 5. public IActionResult Index()
        //    Se define una acción pública llamada Index que devuelve un IActionResult.
        public IActionResult Index()
        {
            // 6. return View();
            //    Se devuelve la vista asociada a la acción Index. Por convención, buscará una vista llamada Index.cshtml
            //    en la carpeta Views/NotificacionesUbicacion. Generalmente, esta vista contendrá la interfaz para
            //    interactuar con las notificaciones de ubicación.
            return View();
        }

        // 7. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //    Generalmente, se utiliza para recibir datos de un formulario o una solicitud AJAX.
        [HttpPost]
        // 8. public async Task<IActionResult> Create(NotificacionesUbicacion notificacionesUbicacion)
        //    Se define una acción asíncrona llamada Create que devuelve un IActionResult.
        //    Recibe un objeto NotificacionesUbicacion como parámetro. ASP.NET Core intentará crear e inicializar
        //    este objeto a partir de los datos enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Create(NotificacionesUbicacion notificacionesUbicacion)
        {
            // 9. try { ... } catch { ... }
            //    Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso
            //    de creación de la notificación de ubicación.
            try
            {
                // 10. _context.NotificacionesUbicaciones.Add(notificacionesUbicacion);
                //     Se agrega el objeto notificacionesUbicacion al DbSet NotificacionesUbicaciones del contexto de la base de datos.
                //     Esto marca el objeto para ser insertado en la tabla correspondiente cuando se guarden los cambios.
                _context.NotificacionesUbicaciones.Add(notificacionesUbicacion);
                // 11. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos.
                //     Esto incluye la inserción de la nueva notificación de ubicación en la tabla NotificacionesUbicaciones.
                await _context.SaveChangesAsync();
                // 12. return Json(notificacionesUbicacion);
                //     Si la creación de la notificación de ubicación se realiza con éxito, se devuelve un resultado JSON
                //     que contiene el objeto notificacionesUbicacion recién creado. Esto es común en aplicaciones AJAX
                //     donde se espera una respuesta JSON.
                return Json(notificacionesUbicacion);
            }
            // 13. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 14. return Json(notificacionesUbicacion);
                //     En caso de excepción durante el proceso de creación, se devuelve un resultado JSON que contiene
                //     el objeto notificacionesUbicacion que se intentó crear. Esto permite que el cliente reciba
                //     información sobre el fallo, aunque generalmente en un escenario de error se devolvería un
                //     código de estado HTTP diferente (ej. 500) o un JSON con detalles del error.
                return Json(notificacionesUbicacion);
            }
        }

        // 15. [HttpPost]
        //     Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //     Generalmente, se utiliza para recibir datos de un formulario o una solicitud AJAX para la edición.
        [HttpPost]
        // 16. [ValidateAntiForgeryToken]
        //     Este atributo agrega una validación antifalsificación de solicitudes. Ayuda a prevenir ataques de tipo Cross-Site Request Forgery (CSRF).
        [ValidateAntiForgeryToken]
        // 17. public async Task<IActionResult> Edit(NotificacionesUbicacion notificacionesUbicacion)
        //     Se define una acción asíncrona llamada Edit que devuelve un IActionResult.
        //     Recibe un objeto NotificacionesUbicacion como parámetro. ASP.NET Core intentará crear e inicializar
        //     este objeto a partir de los datos enviados en la solicitud POST (model binding).
        public async Task<IActionResult> Edit(NotificacionesUbicacion notificacionesUbicacion)
        {
            // 18. try { ... } catch { ... }
            //     Se inicia un bloque try-catch para manejar posibles excepciones que puedan ocurrir durante el proceso
            //     de edición de la notificación de ubicación.
            try
            {
                // 19. var notiUbicacionUpdate = await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(u => u.Id == notificacionesUbicacion.Id);
                //     Se realiza una consulta asíncrona a la base de datos para obtener el registro existente de
                //     NotificacionesUbicacion cuyo Id coincida con el Id del objeto notificacionesUbicacion recibido.
                //     FirstOrDefaultAsync devuelve el primer resultado o null si no se encuentra ninguno.
                var notiUbicacionUpdate = await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(u => u.Id == notificacionesUbicacion.Id);
                // 20. notiUbicacionUpdate.Titulo = notificacionesUbicacion.Titulo;
                //     Se actualiza la propiedad Titulo del objeto notiUbicacionUpdate con el valor proporcionado en
                //     el objeto notificacionesUbicacion recibido.
                notiUbicacionUpdate.Titulo = notificacionesUbicacion.Titulo;
                // 21. notiUbicacionUpdate.Latitud = notificacionesUbicacion.Latitud;
                //     Se actualiza la propiedad Latitud de manera similar.
                notiUbicacionUpdate.Latitud = notificacionesUbicacion.Latitud;
                // 22. notiUbicacionUpdate.Estado = notificacionesUbicacion.Estado;
                //     Se actualiza la propiedad Estado de manera similar.
                notiUbicacionUpdate.Estado = notificacionesUbicacion.Estado;
                // 23. notiUbicacionUpdate.DistanciaMetros = notificacionesUbicacion.DistanciaMetros;
                //     Se actualiza la propiedad DistanciaMetros de manera similar.
                notiUbicacionUpdate.DistanciaMetros = notificacionesUbicacion.DistanciaMetros;

                // 24. _context.NotificacionesUbicaciones.Update(notiUbicacionUpdate);
                //     Se marca el objeto notiUbicacionUpdate en el contexto de la base de datos como modificado.
                //     Entity Framework Core realizará un seguimiento de los cambios en este objeto.
                _context.NotificacionesUbicaciones.Update(notiUbicacionUpdate);
                // 25. await _context.SaveChangesAsync();
                //     Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos.
                //     Esto incluye la actualización del registro de la notificación de ubicación en la tabla
                //     NotificacionesUbicaciones.
                await _context.SaveChangesAsync();
                // 26. return Json(notificacionesUbicacion);
                //     Si la edición de la notificación de ubicación se realiza con éxito, se devuelve un resultado JSON
                //     que contiene el objeto notificacionesUbicacion actualizado. Esto es común en aplicaciones AJAX.
                return Json(notificacionesUbicacion);
            }
            // 27. catch
            //     Se inicia el bloque catch para manejar cualquier excepción que haya ocurrido dentro del bloque try.
            catch
            {
                // 28. return Json(notificacionesUbicacion);
                //     En caso de excepción durante el proceso de edición, se devuelve un resultado JSON que contiene
                //     el objeto notificacionesUbicacion que se intentó editar. Al igual que en el Create, en un
                //     escenario real se manejaría el error de forma más específica (ej. devolviendo un código de
                //     estado de error o detalles del error en el JSON).
                return Json(notificacionesUbicacion);
            }
        }


        // 1. [HttpPost]
        //    Este atributo indica que esta acción solo se ejecutará cuando la solicitud HTTP sea de tipo POST.
        //    Generalmente, se utiliza para recibir datos de un formulario o una solicitud AJAX para la eliminación.
        [HttpPost]
        // 2. public async Task<IActionResult> Delete(int id)
        //    Se define una acción asíncrona llamada Delete que devuelve un IActionResult.
        //    Recibe un parámetro id del tipo entero, que representa el ID de la notificación de ubicación a eliminar.
        public async Task<IActionResult> Delete(int id)
        {
            // 3. var notificacionesUbicacion = await _context.NotificacionesUbicaciones.FindAsync(id);
            //    Se realiza una búsqueda asíncrona en la tabla NotificacionesUbicaciones utilizando el método FindAsync
            //    del contexto de la base de datos para obtener la notificación de ubicación con el ID especificado.
            //    FindAsync es eficiente para buscar por la clave primaria.
            var notificacionesUbicacion = await _context.NotificacionesUbicaciones.FindAsync(id);
            // 4. if (notificacionesUbicacion != null)
            //    Se verifica si se encontró una notificación de ubicación con el ID proporcionado en la base de datos.
            if (notificacionesUbicacion != null)
            {
                // 5. _context.NotificacionesUbicaciones.Remove(notificacionesUbicacion);
                //    Si se encontró la notificación de ubicación, se marca para su eliminación del DbSet
                //    NotificacionesUbicaciones del contexto de la base de datos.
                _context.NotificacionesUbicaciones.Remove(notificacionesUbicacion);
            }

            // 6. await _context.SaveChangesAsync();
            //    Se guardan de forma asíncrona todos los cambios realizados en el contexto de la base de datos.
            //    Esto incluye la eliminación de la notificación de ubicación (si se encontró) de la tabla
            //    NotificacionesUbicaciones.
            await _context.SaveChangesAsync();
            // 7. return RedirectToAction(nameof(Index));
            //    Después de la eliminación (o intento de eliminación), se redirige al usuario a la acción Index
            //    del mismo controlador NotificacionesUbicacionController, que probablemente muestra la lista
            //    de notificaciones de ubicación.
            return RedirectToAction(nameof(Index));
        }

        // 8. private bool NotificacionesUbicacionExists(int id)
        //    Se define un método privado llamado NotificacionesUbicacionExists que devuelve un valor booleano.
        //    Este método se utiliza para verificar si una notificación de ubicación con el ID especificado existe en la base de datos.
        private bool NotificacionesUbicacionExists(int id)
        {
            // 9. return _context.NotificacionesUbicaciones.Any(e => e.Id == id);
            //    Se realiza una consulta a la base de datos utilizando LINQ y el método Any para verificar si existe
            //    algún registro en la tabla NotificacionesUbicaciones cuya propiedad Id coincida con el ID proporcionado.
            //    Devuelve true si existe al menos un registro coincidente, y false en caso contrario.
            return _context.NotificacionesUbicaciones.Any(e => e.Id == id);
        }

        // 10. public async Task<JsonResult> GetUbicationFromCdId()
        //     Se define una acción pública asíncrona llamada GetUbicationFromCdId que devuelve un JsonResult.
        //     JsonResult se utiliza para enviar datos en formato JSON como respuesta HTTP.
        public async Task<JsonResult> GetUbicationFromCdId()
        {
            // 11. return Json(await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value)));
            //     Se realiza una consulta asíncrona a la base de datos en la tabla NotificacionesUbicaciones.
            //     Se utiliza FirstOrDefaultAsync para obtener la primera notificación de ubicación cuyo IdCiudadano
            //     coincida con el ID del ciudadano actual. Se asume que User representa el usuario autenticado
            //     y FindFirst("Id") busca la primera claim con el tipo "Id". El valor de la claim se convierte
            //     a un entero antes de la comparación.
            //     El resultado de esta consulta (la notificación de ubicación o null si no se encuentra) se
            //     envuelve en un JsonResult y se devuelve como respuesta HTTP.
            return Json(await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value)));
        }

        // 12. public async Task<JsonResult> GetOpUbication()
        //     Se define una acción pública asíncrona llamada GetOpUbication que devuelve un JsonResult.
        //     Se espera que esta acción obtenga la ubicación de un operador.
        public async Task<JsonResult> GetOpUbication()
        {
            // 13. var zona = await _context.Zonas.FirstOrDefaultAsync(x=>x.Nombre == User.FindFirst("Zona").Value);
            //     Se realiza una consulta asíncrona a la base de datos en la tabla Zonas.
            //     Se utiliza FirstOrDefaultAsync para obtener la primera zona cuyo Nombre coincida con el valor
            //     de la claim "Zona" del usuario actual. Se asume que User representa el usuario autenticado
            //     y FindFirst("Zona") busca la primera claim con el tipo "Zona".
            var zona = await _context.Zonas.FirstOrDefaultAsync(x => x.Nombre == User.FindFirst("Zona").Value);
            // 14. var horario = await _context.Horarios.FirstOrDefaultAsync(x => x.IdZona == zona.Id);
            //     Se realiza una consulta asíncrona a la base de datos en la tabla Horarios.
            //     Se utiliza FirstOrDefaultAsync para obtener el primer horario cuyo IdZona coincida con el Id
            //     de la zona obtenida en el paso anterior.
            var horario = await _context.Horarios.FirstOrDefaultAsync(x => x.IdZona == zona.Id);
            // 15. var ubicacion = await _context.Ubicaciones.FirstOrDefaultAsync(x => x.IdOperador == horario.IdOperador);
            //     Se realiza una consulta asíncrona a la base de datos en la tabla Ubicaciones.
            //     Se utiliza FirstOrDefaultAsync para obtener la primera ubicación cuyo IdOperador coincida con el
            //     IdOperador del horario obtenido en el paso anterior. Esto asume una relación entre Horario y Operador
            //     y entre Operador e Ubicacion.
            var ubicacion = await _context.Ubicaciones.FirstOrDefaultAsync(x => x.IdOperador == horario.IdOperador);

            // 16. return Json(ubicacion);
            //     El resultado de la consulta de ubicación (el objeto Ubicacion o null si no se encuentra) se
            //     envuelve en un JsonResult y se devuelve como respuesta HTTP en formato JSON.
            return Json(ubicacion);
        }
    }
}
